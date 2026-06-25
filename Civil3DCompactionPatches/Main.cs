using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DatabaseServices;
using Civil3DCompactionPatches.Models;
using Civil3DUtils;
using Civil3DUtils.Utils;
using CoreUtils;
using MahApps.Metro.Controls;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Civil3DCompactionPatches
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DCompactionPatchesReport", CommandFlags.Modal)]
        public void Civil3DCompactionPatchesReport()
        {
            Log log = null;

            try
            {
                Document doc = AutocadDocumentService.ActiveDocument;
                Editor ed = doc.Editor;

                string drawingPath = doc.Name;
                string drawingFolder = Path.GetDirectoryName(drawingPath);

                log = new Log(drawingFolder, "Civil3DCompactionPatchesReport");
                log.Information("Command started.");

                TinSurface mainSurface = SurfaceUtils.PromptTinSurface(OpenMode.ForRead, "\nSelect MAIN TIN Surface: ");
                if (mainSurface == null)
                {
                    log.Warning("Main surface not selected.");
                    return;
                }
                ed.WriteMessage($"\nMain Surface: {mainSurface.Name}");

                TinSurface comparisonSurface = SurfaceUtils.PromptTinSurface(OpenMode.ForRead, "\nSelect COMPARISON TIN Surface: ");
                if (comparisonSurface == null)
                {
                    log.Warning("Comparison surface not selected.");
                    return;
                }
                ed.WriteMessage($"\nComparison Surface: {comparisonSurface.Name}");

                Alignment alignment = AlignmentUtils.GetAlignment("\nSelect alignment");

                if (comparisonSurface == null)
                {
                    log.Warning("Alignment not selected.");
                    return;
                }

                log.Information($"Alignment: {alignment.Name}");

                List<CogoPoint> points = CogoPointUtils.PromptMultipleCogoPoints(OpenMode.ForRead);
                log.Information($"Selected COGO points: {points?.Count ?? 0}");

                // 5️⃣ Ascending / Descending
                PromptKeywordOptions promptHeatmap = new PromptKeywordOptions("\nGenerate Heatmap [Yes/No]: ", "Yes No");
                promptHeatmap.AllowNone = false;

                var heatmapResult = AutocadDocumentService.Editor.GetKeywords(promptHeatmap);
                if (heatmapResult.Status != PromptStatus.OK) return;

                bool doGenerateHeatmap = heatmapResult.StringResult == "Yes";

                List<BoxArea> boxAreas = new List<BoxArea>();

                List<CogoPointGeometry> cogoPointGeometries = new List<CogoPointGeometry>();

                TinSurfaceTriangleCollection triangles = mainSurface.GetTriangles(false);

                using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
                {
                    log.Information("Transaction started.");

                    log.Information($"Triangles count: {triangles.Count}");

                    int i = 0;

                    foreach (CogoPoint point in points)
                    {
                        log.Information($"Detect extents for COGO point, number: {point.PointNumber} (Handle: {point.Handle})");

                        BlockTableRecord btr = point.GetMarkerBlockTableReceord(tr);
                        var blockEntities = btr.GetEntitiesInside(tr);

                        Polyline closedPolyline = blockEntities
                            .OfType<Polyline>()
                            .FirstOrDefault(p => p.Closed);

                        if (closedPolyline == null)
                        {
                            log.Warning("No closed polyline found for point.");
                            continue;
                        }

                        Polyline newPl = point.TransformEntityToCogoPointLocation(closedPolyline) as Polyline;

                        Extents3d ext = newPl.GeometricExtents;

                        Point2d[] polylinePoints = new Point2d[newPl.NumberOfVertices];
                        for (int k = 0; k < polylinePoints.Length; k++)
                            polylinePoints[k] = newPl.GetPoint2dAt(k);

                        cogoPointGeometries.Add(new CogoPointGeometry
                        {
                            CogoPoint = point,
                            PolylineExtent = ext,
                            PolylinePoints = polylinePoints,
                            Polyline = newPl
                        });
                    }

                    tr.Commit();
                    log.Information("Transaction committed.");
                }

                log.Information($"Filter triangles per total cogo points extents");

                List<Point3d> extentPoints = new List<Point3d>();
                extentPoints.AddRange(cogoPointGeometries.Select(item => item.PolylineExtent.MinPoint));
                extentPoints.AddRange(cogoPointGeometries.Select(item => item.PolylineExtent.MaxPoint));

                double minX = extentPoints.MinBy(item => item.X).X;
                double maxX = extentPoints.MaxBy(item => item.X).X;

                double minY = extentPoints.MinBy(item => item.Y).Y;
                double maxY = extentPoints.MaxBy(item => item.Y).Y;

                Extents3d totalExtent = new Extents3d(new Point3d(minX, minY, 0), new Point3d(maxX, maxY, 0));

                List<TriangleData> filteredTriangles = triangles
                    .Cast<TinSurfaceTriangle>()
                    .Select(triangle => TriangleData.ToTriangleData(triangle))
                    .Where(triangle => !TriangleData.TriangleOutsideExtents(triangle, totalExtent))
                    .ToList();

                log.Information($"Triangles have been filtered, initial amount: {triangles.Count}, filtered amount: {filteredTriangles.Count}");

                double averageBoxSize = GetAverageBoxSize(cogoPointGeometries);
                double cellSize = Math.Max(averageBoxSize, 1.0);

                TriangleSpatialIndex triangleIndex = new TriangleSpatialIndex(filteredTriangles, cellSize);

                log.Information($"Triangles indexed: {filteredTriangles.Count}, Cell size: {cellSize}");

                using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
                {
                    log.Information("Transaction started.");

                    int i = 0;

                    foreach (CogoPointGeometry cogoPointGeometry in cogoPointGeometries)
                    {
                        log.Information($"Detect MAIN surface points inside cogo point extents #{i + 1} (Handle: {cogoPointGeometry.CogoPoint.Handle})");

                        var uniquePoints = new HashSet<Point3d>(new Point3dComparer(0.01));

                        int trianglesChecked = 0;
                        int pointsInside = 0;

                        IEnumerable<TriangleData> candidateTriangles = triangleIndex.Query(cogoPointGeometry.PolylineExtent);

                        foreach (TriangleData triangle in candidateTriangles)
                        {
                            trianglesChecked++;

                            if (TriangleData.TriangleOutsideExtents(triangle, cogoPointGeometry.PolylineExtent))
                                continue;

                            Point3d[] triPts =
                            {
                                    triangle.P1,
                                    triangle.P2,
                                    triangle.P3
                                };

                            foreach (Point3d pt in triPts)
                            {
                                if (uniquePoints.Contains(pt))
                                    continue;

                                if (IsPointInsidePolyline(cogoPointGeometry.PolylinePoints, new Point2d(pt.X, pt.Y)))
                                {
                                    uniquePoints.Add(pt);
                                    pointsInside++;
                                }
                            }
                        }

                        log.Information($"Triangles checked: {trianglesChecked}, Points inside: {pointsInside}");

                        List<MainComparisonPointPair> pairs = new List<MainComparisonPointPair>();

                        foreach (Point3d p in uniquePoints)
                        {
                            double compZ = comparisonSurface.FindElevationAtXY(p.X, p.Y);

                            pairs.Add(new MainComparisonPointPair
                            {
                                MainPoint = p,
                                ComparisonPoint = new Point3d(p.X, p.Y, compZ)
                            });
                        }

                        log.Information($"Pairs created: {pairs.Count}");

                        double stationCenter = 0;
                        double offsetCenter = 0;
                        alignment.StationOffset(cogoPointGeometry.CogoPoint.Location.X, cogoPointGeometry.CogoPoint.Location.Y, ref stationCenter, ref offsetCenter);

                        double offsetMin = 0;
                        double stationMin = 0;
                        Point2d boxPointMinByX = cogoPointGeometry.PolylinePoints.MinBy(item => item.X);
                        alignment.StationOffset(boxPointMinByX.X, boxPointMinByX.Y, ref stationMin, ref offsetMin);

                        double offsetMax = 0;
                        double stationMax = 0;
                        Point2d boxPointMaxByX = cogoPointGeometry.PolylinePoints.MaxBy(item => item.X);
                        alignment.StationOffset(boxPointMaxByX.X, boxPointMaxByX.Y, ref stationMax, ref offsetMax);

                        boxAreas.Add(new BoxArea
                        {
                            Id = cogoPointGeometry.CogoPoint.PointNumber,
                            MainCogoPoint = cogoPointGeometry.CogoPoint,
                            Polyline = cogoPointGeometry.Polyline,
                            MainComparisonPointPairs = pairs,
                            StationBoxCenter = stationCenter,
                            StationBoxMin = stationMin < stationMax ? stationMin : stationMax,
                            StationBoxMax = stationMax > stationMin ? stationMax : stationMin
                        });
                    }

                    List<BoxAreaReportRow> boxAreaReportRows = new List<BoxAreaReportRow>();

                    int position = 1;

                    boxAreas = boxAreas.OrderBy(item => item.ElementName).ThenByDescending(item => item.SegmentName).ThenBy(item => item.DivisionName).ThenBy(item => item.RowPosition).ToList();

                    foreach (BoxArea boxArea in boxAreas)
                    {
                        boxAreaReportRows.Add(new BoxAreaReportRow
                        {
                            Position = position.ToString("D6"),
                            RowPosition = boxArea.RowPosition,
                            Model = boxArea,
                            Id = boxArea.Id,
                            Description = boxArea.Description,
                            ElementName = boxArea.ElementName,
                            SegmentName = boxArea.SegmentName,
                            DivisionName = boxArea.DivisionName,
                            BoxCenterX = boxArea.MainCogoPoint.Location.X,
                            BoxCenterY = boxArea.MainCogoPoint.Location.Y,
                            Count = boxArea.MainComparisonPointPairs.Count,
                            StationCenter = boxArea.StationBoxCenter,
                            ElevationDifferenceSum = boxArea.MainComparisonPointPairs.Sum(item => item.ElevationDifference),
                            ElevationDifferenceAverage = boxArea.MainComparisonPointPairs.Average(item => item.ElevationDifference),
                            ElevationDifferenceMax = boxArea.MainComparisonPointPairs.Max(item => item.ElevationDifference),
                            ElevationDifferenceMin = boxArea.MainComparisonPointPairs.Min(item => item.ElevationDifference),
                            Length = Math.Abs(boxArea.StationBoxMax - boxArea.StationBoxMin),
                            StationMax = boxArea.StationBoxMax,
                            StationMin = boxArea.StationBoxMin
                        });

                        position++;
                    }

                    ExportBoxAreasToExcel(boxAreaReportRows, drawingFolder, log);

                    if (doGenerateHeatmap)
                    {
                        List<HeightRange> heightRanges = GenerateHeatmap(boxAreaReportRows, log);

                        Point3d? legendPoint = PromptUtils.PromptPoint();

                        if (legendPoint.HasValue)
                        {
                            HeightRangeGenerator.DrawLegend(heightRanges, legendPoint.Value);
                        }
                    }

                    tr.Commit();
                    log.Information("Transaction committed.");
                }

                log.Information($"Finished. Total BoxAreas: {boxAreas.Count}");
            }
            catch (System.Exception ex)
            {
                log?.LogError("Unexpected error occurred.", ex);
                MessageBox.Show("Unexpected error");
            }
        }

        [CommandMethod("PSV", "Civil3DCompactionPatchesIndexPerStation", CommandFlags.Modal)]
        public void Civil3DCompactionPatchesIndexPerStation()
        {
            // 1️⃣ Select COGO points
            PromptSelectionOptions selOpts = new PromptSelectionOptions();
            selOpts.MessageForAdding = "\nSelect COGO points: ";

            TypedValue[] filter = new TypedValue[]
            {
                new TypedValue((int)DxfCode.Start, "AECC_COGO_POINT")
            };

            SelectionFilter selFilter = new SelectionFilter(filter);
            PromptSelectionResult selRes = AutocadDocumentService.Editor.GetSelection(selOpts, selFilter);

            if (selRes.Status != PromptStatus.OK) return;

            // 2️⃣ Select alignment
            PromptEntityOptions alignOpts = new PromptEntityOptions("\nSelect alignment: ");
            alignOpts.SetRejectMessage("\nMust be an alignment.");
            alignOpts.AddAllowedClass(typeof(Alignment), true);

            PromptEntityResult alignRes = AutocadDocumentService.Editor.GetEntity(alignOpts);
            if (alignRes.Status != PromptStatus.OK) return;

            // 3️⃣ Description prefix
            PromptStringOptions prefixOpts = new PromptStringOptions("\nEnter description prefix: ");
            prefixOpts.AllowSpaces = false;

            var prefixRes = AutocadDocumentService.Editor.GetString(prefixOpts);
            if (prefixRes.Status != PromptStatus.OK) return;
            string prefix = prefixRes.StringResult;

            // 4️⃣ Starting index
            PromptIntegerOptions startOpts = new PromptIntegerOptions("\nEnter starting index: ");
            startOpts.DefaultValue = 1;

            var startRes = AutocadDocumentService.Editor.GetInteger(startOpts);
            if (startRes.Status != PromptStatus.OK) return;

            int stationIndex = startRes.Value;


            using (Transaction tr = AutocadDocumentService.Database.TransactionManager.StartTransaction())
            {
                Alignment alignment = tr.GetObject(alignRes.ObjectId, OpenMode.ForRead) as Alignment;

                var stationGroups = new Dictionary<double, List<CogoPoint>>();

                // 6️⃣ Collect stations
                foreach (SelectedObject selObj in selRes.Value)
                {
                    if (selObj == null) continue;

                    CogoPoint pt = tr.GetObject(selObj.ObjectId, OpenMode.ForWrite) as CogoPoint;
                    if (pt == null) continue;

                    double station = 0, offset = 0;

                    try
                    {
                        alignment.StationOffset(pt.Easting, pt.Northing, ref station, ref offset);
                    }
                    catch
                    {
                        continue; // Skip if fails
                    }

                    double roundedStation = Math.Round(station, 2);

                    if (!stationGroups.ContainsKey(roundedStation))
                        stationGroups[roundedStation] = new List<CogoPoint>();

                    stationGroups[roundedStation].Add(pt);
                }

                // 7️⃣ Sort groups by station
                var sortedGroups = stationGroups.OrderBy(g => g.Key);

                // Assign index along the stations
                foreach (var group in sortedGroups)
                {
                    int columnIndex = 0;

                    AutocadDocumentService.Editor.WriteMessage($"\nProcess point group, station {group.Key}, points count: {group.Value.Count}.");
                    // Optional: sort inside group (e.g. by offset or point number)

                    List<CogoPoint> points = group.Value.OrderBy(item => item.Location.X).ToList();

                    foreach (var pt in points)
                    {
                        columnIndex++;
                        pt.RawDescription = prefix + stationIndex.ToString("00") + "-" + columnIndex.ToString("00");
                    }

                        stationIndex++;
                }

                tr.Commit();
            }

            AutocadDocumentService.Editor.WriteMessage("\nCOGO points renamed successfully.");
        }

        public List<HeightRange> GenerateHeatmap(List<BoxAreaReportRow> boxAreaReportRows, Log log)
        {
            List<HeightRange> heightRanges = new List<HeightRange>();

            if (boxAreaReportRows.Count == 0)
            {
                return heightRanges;
            }

            log.Information("Heatmap generation started.");

            List<CogoPointWithBoundary> cogoPointWithBoundaries = new List<CogoPointWithBoundary>();

            LayerTableRecord layerBoundary = null;
            LayerTableRecord layerHatch = null;

            using (Transaction tr = AutocadDocumentService.TransactionManager.StartTransaction())
            {
                layerBoundary = LayerTableRecordUtils.GetOrCreateLayer("Civil3DCompactionPatchesHeatmap_Boundary", tr, OpenMode.ForRead);
                layerHatch = LayerTableRecordUtils.GetOrCreateLayer("Civil3DCompactionPatchesHeatmap_Hatch", tr, OpenMode.ForRead);
                tr.Commit();
            }

            using (Transaction tr = AutocadDocumentService.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = (BlockTable)tr.GetObject(AutocadDocumentService.Database.BlockTableId, OpenMode.ForRead);

                BlockTableRecord blockTableRecord = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                double heightMin = boxAreaReportRows.MinBy(row => row.ElevationDifferenceAverage).ElevationDifferenceAverage;
                double heightMax = boxAreaReportRows.MaxBy(row => row.ElevationDifferenceAverage).ElevationDifferenceAverage;

                heightRanges = HeightRangeGenerator.Generate();

                foreach (BoxAreaReportRow row in boxAreaReportRows)
                {
                    blockTableRecord.AppendEntity(row.Model.Polyline);
                    tr.AddNewlyCreatedDBObject(row.Model.Polyline, add: true);

                    var polyline = tr.GetObject(row.Model.Polyline.Id, OpenMode.ForWrite, false, true) as Polyline;

                    polyline.Layer = layerBoundary.Name;
                    Hatch hatch = AutoCADUtils.Utils.HatchUtils.Create(blockTableRecord, tr, layerHatch.Name, polyline);
                    hatch = tr.GetObject(hatch.Id, OpenMode.ForWrite, false, true) as Hatch;

                    HeightRange currentRange = HeightRangeGenerator.FindRange(row.ElevationDifferenceAverage, heightRanges);

                    hatch.Color = currentRange.Color;
                }

                tr.Commit();
            }

            return heightRanges;
        }

        private static double GetAverageBoxSize(List<CogoPointGeometry> geometries)
        {
            if (geometries == null || geometries.Count == 0)
                return 10.0;

            double avgWidth = geometries.Average(g => g.PolylineExtent.MaxPoint.X - g.PolylineExtent.MinPoint.X);
            double avgHeight = geometries.Average(g => g.PolylineExtent.MaxPoint.Y - g.PolylineExtent.MinPoint.Y);

            return Math.Max((avgWidth + avgHeight) * 0.5, 1.0);
        }

        private static void ExportBoxAreasToExcel(List<BoxAreaReportRow> rows, string defaultFolder, Log log)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string defaultFileName = $"BermHeightReport_{timestamp}.xlsx";

                // Show save dialog

                SaveFileDialog dialog = new SaveFileDialog
                {
                    Title = "Save Berm Height Report",
                    FileName = defaultFileName,
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    InitialDirectory = defaultFolder
                };


                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    log?.Warning("User canceled file save dialog.");
                    return;
                }

                string filePath = dialog.FileName;

                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet("Berm Heights");

                int rowIndex = 0;

                // Header
                IRow header = sheet.CreateRow(rowIndex++);
                string[] headers =
                {
            "Position", "Id", "StationCenter", "StationMin", "StationMax",
            "Full Name", "ElementName", "SegmentName", "DivisionName", "Row", "Length", "Count",
            "Sum", "Average", "Min", "Max",
            "X(center)", "Y(center)"
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    header.CreateCell(i).SetCellValue(headers[i]);
                }

                // Data
                foreach (var row in rows)
                {
                    IRow excelRow = sheet.CreateRow(rowIndex++);

                    excelRow.CreateCell(0).SetCellValue(row.Position);
                    excelRow.CreateCell(1).SetCellValue(row.Id);
                    excelRow.CreateCell(2).SetCellValue(row.StationCenter);
                    excelRow.CreateCell(3).SetCellValue(row.StationMin);
                    excelRow.CreateCell(4).SetCellValue(row.StationMax);
                    excelRow.CreateCell(5).SetCellValue(row.Description ?? "");
                    excelRow.CreateCell(6).SetCellValue(row.ElementName ?? "");
                    excelRow.CreateCell(7).SetCellValue(row.SegmentName ?? "");
                    excelRow.CreateCell(8).SetCellValue(row.DivisionName ?? "");
                    excelRow.CreateCell(9).SetCellValue(row.RowPosition ?? "");
                    excelRow.CreateCell(10).SetCellValue(Math.Round(row.Length, 6));
                    excelRow.CreateCell(11).SetCellValue(row.Count);
                    excelRow.CreateCell(12).SetCellValue(row.ElevationDifferenceSum);
                    excelRow.CreateCell(13).SetCellValue(row.ElevationDifferenceAverage);
                    excelRow.CreateCell(14).SetCellValue(row.ElevationDifferenceMin);
                    excelRow.CreateCell(15).SetCellValue(row.ElevationDifferenceMax);
                    excelRow.CreateCell(16).SetCellValue(row.BoxCenterX);
                    excelRow.CreateCell(17).SetCellValue(row.BoxCenterY);
                }

                sheet.SetAutoFilter(new NPOI.SS.Util.CellRangeAddress(0, rowIndex - 1, 0, headers.Length - 1));

                // Auto-size columns
                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(fs);
                }

                workbook.Close();

                log?.Information($"Excel exported: {filePath}");
            }
            catch (System.Exception ex)
            {
                log?.LogError("Failed to export Excel.", ex);
            }
        }

        private static bool IsPointInsidePolyline(Point2d[] polyPts, Point2d pt)
        {
            int num = polyPts.Length;
            bool inside = false;

            for (int i = 0, j = num - 1; i < num; j = i++)
            {
                var pi = polyPts[i];
                var pj = polyPts[j];

                bool intersect = ((pi.Y > pt.Y) != (pj.Y > pt.Y)) &&
                                 (pt.X < (pj.X - pi.X) *
                                 (pt.Y - pi.Y) / (pj.Y - pi.Y + 1e-12) + pi.X);

                if (intersect)
                    inside = !inside;
            }

            return inside;
        }
    }
}
