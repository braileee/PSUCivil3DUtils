using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using Civil3DToolbox.Models;
using Civil3DUtils;
using Civil3DUtils.Utils;
using ControlzEx.Standard;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Civil3DToolbox
{
    public class Main
    {
        [CommandMethod("PSV", "ReverseTextElevation", CommandFlags.Modal)]
        public static void ReverseTextElevation()
        {
            List<DBText> texts = SelectionUtils.GetElements<DBText>("Select text to reverse elevation");

            if (texts.Count == 0)
            {
                AutocadDocumentService.Editor.WriteMessage($"No text was selected");
                return;
            }

            AutocadDocumentService.Editor.WriteMessage($"Text entities selected: {texts.Count}");

            using (AutocadDocumentService.LockActiveDocument())
            {
                using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    foreach (DBText text in texts)
                    {
                        DBText textOpened = transaction.GetObject(text.Id, OpenMode.ForWrite, false, true) as DBText;
                        textOpened.Position = new Point3d(textOpened.Position.X, textOpened.Position.Y, -textOpened.Position.Z);
                    }

                    transaction.Commit();
                }
            }
        }

        [CommandMethod("PSV", "ReversePolylineElevation", CommandFlags.Modal)]
        public static void ReversePolylineElevation()
        {
            List<Polyline> polylines = SelectionUtils.GetElements<Polyline>("Select polylines to reverse elevation");

            if (polylines.Count == 0)
            {
                AutocadDocumentService.Editor.WriteMessage($"No polyline was selected");
                return;
            }

            AutocadDocumentService.Editor.WriteMessage($"Polyline entities selected: {polylines.Count}");

            using (AutocadDocumentService.LockActiveDocument())
            {
                using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    foreach (Polyline polyline in polylines)
                    {
                        Polyline openedPolyline = transaction.GetObject(polyline.Id, OpenMode.ForWrite, false, true) as Polyline;
                        openedPolyline.Elevation = -openedPolyline.Elevation;
                    }

                    transaction.Commit();
                }
            }
        }

        [CommandMethod("PSV", "ReversePolyline2dElevation", CommandFlags.Modal)]
        public static void ReversePolyline2dElevation()
        {
            List<Polyline2d> polylines = SelectionUtils.GetElements<Polyline2d>("Select polylines to reverse elevation");

            if (polylines.Count == 0)
            {
                AutocadDocumentService.Editor.WriteMessage($"No polyline was selected");
                return;
            }

            AutocadDocumentService.Editor.WriteMessage($"Polyline entities selected: {polylines.Count}");

            using (AutocadDocumentService.LockActiveDocument())
            {
                using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    foreach (Polyline2d polyline in polylines)
                    {
                        Polyline2d openedPolyline = transaction.GetObject(polyline.Id, OpenMode.ForWrite, false, true) as Polyline2d;
                        openedPolyline.Elevation = -openedPolyline.Elevation;
                    }

                    transaction.Commit();
                }
            }
        }

        [CommandMethod("PSV", "BindXrefsForDwgsInFolder", CommandFlags.Modal)]
        public static void BindXrefsForDwgsInFolder()
        {
            try
            {
                Document document = null;

                string directory = FolderUtils.GetFolderPathExtendedWindow(Environment.SpecialFolder.Desktop);

                if (!Directory.Exists(directory))
                {
                    MessageBox.Show("No such directory");
                    return;
                }

                string[] dwgFilePaths = Directory.GetFiles(directory, "*.dwg", SearchOption.AllDirectories);

                foreach (string dwgFilePath in dwgFilePaths)
                {

                    document = AutocadDocumentService.DocumentManager.Open(dwgFilePath, forReadOnly: false);
                    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument = document;

                    using (document.LockDocument())
                    {
                        using (Transaction transaction = document.TransactionManager.StartTransaction())
                        {
                            Autodesk.AutoCAD.DatabaseServices.ObjectIdCollection xrefIdCollection = new Autodesk.AutoCAD.DatabaseServices.ObjectIdCollection();

                            using (XrefGraph xrefGraph = document.Database.GetHostDwgXrefGraph(false))
                            {
                                int numOfNodes = xrefGraph.NumNodes;

                                for (int nodeIndex = 0; nodeIndex < xrefGraph.NumNodes; nodeIndex++)
                                {
                                    XrefGraphNode xNode = xrefGraph.GetXrefNode(nodeIndex);

                                    if (!xNode.Database.Filename.Equals(document.Database.Filename))
                                    {
                                        if (xNode.XrefStatus == XrefStatus.Resolved)
                                        {
                                            xrefIdCollection.Add(xNode.BlockTableRecordId);
                                        }
                                    }
                                }
                            }

                            if (xrefIdCollection.Count != 0)
                            {
                                document.Database.BindXrefs(xrefIdCollection, true);

                                foreach (Autodesk.AutoCAD.DatabaseServices.ObjectId xrefId in xrefIdCollection)
                                {
                                    document.Database.DetachXref(xrefId);
                                }
                            }

                            dynamic acadDoc = document.GetAcadDocument();
                            acadDoc.Save();
                            transaction.Commit();
                        }

                    }
                }
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message, "Error");
            }
        }

        [CommandMethod("PSV", "MoveSolidsInsertionPointToTinSurface", CommandFlags.Modal)]
        public static void MoveSolidsInsertionPointToTinSurface()
        {
            try
            {
                TinSurface surface = SurfaceUtils.PromptTinSurface(OpenMode.ForRead);

                if (surface == null)
                {
                    return;
                }

                List<Solid3d> solids = SolidUtils.PromptMultipleSolids3d(OpenMode.ForWrite);

                if (solids.Count == 0)
                {
                    return;
                }

                using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    foreach (Solid3d solid in solids)
                    {
                        Point3d maxPoint = solid.Bounds.Value.MaxPoint;
                        double surfaceElevation = 0;

                        try
                        {
                            surfaceElevation = surface.FindElevationAtXY(maxPoint.X, maxPoint.Y);
                        }
                        catch (System.Exception)
                        {
                            continue;
                        }

                        solid.Move(transaction, maxPoint, new Point3d(maxPoint.X, maxPoint.Y, surfaceElevation));
                    }

                    transaction.Commit();
                }
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }


        [CommandMethod("PSV", "AddNumberSuffixToRawDescriptionPoint", CommandFlags.Modal)]
        public static void AddNumberSuffixToRawDescriptionPoint()
        {
            try
            {
                List<CogoPoint> points = CogoPointUtils.PromptMultipleCogoPoints(OpenMode.ForWrite);

                string startNumberString = PromptUtils.PromptString("Select start number");

                if (string.IsNullOrEmpty(startNumberString) || string.IsNullOrWhiteSpace(startNumberString))
                {
                    return;
                }

                int startNumber = NumbersUtils.ParseStringToInt(startNumberString);

                using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        CogoPoint cogoPoint = points[i];

                        if (!cogoPoint.IsWriteEnabled)
                        {
                            cogoPoint = transaction.GetObject(cogoPoint.Id, OpenMode.ForWrite, false, true) as CogoPoint;
                        }

                        cogoPoint.RawDescription = $"{cogoPoint.RawDescription}-{startNumber++:000}";
                    }

                    transaction.Commit();
                }
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        [CommandMethod("PSV", "CreateCOGOPointsFromCircles", CommandFlags.Modal)]
        public static void CreateCOGOPointsFromCircles()
        {
            try
            {
                List<Circle> circles = SelectionUtils.GetElements<Circle>("Select circles");

                string description = PromptUtils.PromptString("Input raw description:");

                if (string.IsNullOrEmpty(description) || string.IsNullOrWhiteSpace(description))
                {
                    return;
                }

                List<Point3d> points = new List<Point3d>();

                using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    for (int i = 0; i < circles.Count; i++)
                    {
                        Circle circle = circles[i];
                        points.Add(circle.Center);
                    }

                    List<CogoPoint> cogoPoints = CogoPointUtils.CreateCogoPoints(points, description);

                    transaction.Commit();
                }
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        [CommandMethod("PSV", "SetMinMaxElevationsSectionViews", CommandFlags.Modal)]
        public static void SetMinMaxElevationsSectionViews()
        {
            try
            {
                List<SectionView> sectionViews = SelectionUtils.GetElements<SectionView>("Select section views");

                string minElevationString = PromptUtils.PromptString("Input min elevation:");
                double minElevation = NumbersUtils.ParseStringToDouble(minElevationString);

                string maxElevationString = PromptUtils.PromptString("Input max elevation:");
                double maxElevation = NumbersUtils.ParseStringToDouble(maxElevationString);

                if (minElevation > maxElevation)
                {
                    MessageBox.Show("Min elevation must be less than max elevation", "Error");
                    return;
                }

                using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    foreach (SectionView sectionView in sectionViews)
                    {
                        SectionView sectionViewOpened = transaction.GetObject(sectionView.Id, OpenMode.ForWrite, false, true) as SectionView;
                        sectionViewOpened.IsElevationRangeAutomatic = false;
                        sectionViewOpened.ElevationMin = minElevation;
                        sectionViewOpened.ElevationMax = maxElevation;
                    }

                    transaction.Commit();
                }
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        [CommandMethod("PSV", "ConvertGeotiffToDemSurface", CommandFlags.Modal)]
        public static void ConvertGeotiffToDemSurface()
        {
            try
            {
                Log log = new Log(AssemblyUtils.GetFolder(typeof(Main)), "ConvertGeotiffToDemSurface");

                Document acadDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Editor ed = acadDoc.Editor;
                Database db = acadDoc.Database;
                CivilDocument civDoc = CivilApplication.ActiveDocument;

                // Ask user for folder path
                PromptResult folderRes = ed.GetString(
                    "\nEnter folder path with \"\" containing GEOTIFF files (*.tif): ");
                if (folderRes.Status != PromptStatus.OK)
                    return;

                string folder = folderRes.StringResult.Trim('"');
                if (!Directory.Exists(folder))
                {
                    ed.WriteMessage("\nFolder does not exist.");
                    return;
                }

                string[] demFiles = Directory.GetFiles(folder, "*.tif",
                    SearchOption.TopDirectoryOnly);

                log.Information("Folder selected: " + folder);
                log.Information("Number of DEM files found: " + demFiles.Length);

                if (demFiles.Length == 0)
                {
                    log.Error("\nNo DEM files found.");
                    ed.WriteMessage("\nNo DEM files found.");
                    return;
                }

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    try
                    {
                        // Pick a surface style (adjust index as needed)
                        ObjectId styleId = civDoc.Styles.SurfaceStyles[0];

                        foreach (string demPath in demFiles)
                        {
                            log.Information($"Add DEM from {demPath}");

                            string name = Path.GetFileNameWithoutExtension(demPath);

                            ed.WriteMessage($"\nAdd surface: {name}");

                            // Create a GridSurface directly from DEM
                            GridSurface.CreateFromDEM(demPath, styleId);
                        }

                        tr.Commit();
                        ed.WriteMessage(
                            $"\nCreated {demFiles.Length} DEM surfaces successfully.");
                        log.Information("Successfully finished");
                    }
                    catch (System.Exception ex)
                    {
                        log.Error($"Error: {ex.Message} {ex.StackTrace}");
                        ed.WriteMessage($"\nError: {ex.Message}");
                    }
                }

            }
            catch (System.Exception)
            {

            }
        }

        [CommandMethod("PSV", "ExportTinXYZ", CommandFlags.Modal)]
        public static void ExportTinXYZ()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    // Select surface
                    PromptEntityOptions peo = new PromptEntityOptions("\nSelect TIN Surface: ");
                    peo.SetRejectMessage("TIN surfaces allowed only");
                    peo.AddAllowedClass(typeof(TinSurface), true);
                    PromptEntityResult per = ed.GetEntity(peo);
                    if (per.Status != PromptStatus.OK) return;

                    TinSurface surface = tr.GetObject(per.ObjectId, OpenMode.ForRead) as TinSurface;

                    if (surface == null)
                    {
                        ed.WriteMessage("\nSelected entity is not a TIN Surface.");
                        return;
                    }

                    // File path based on DWG folder + surface name
                    string filePath = Path.Combine(
                        Path.GetDirectoryName(doc.Name),
                        surface.Name + ".xyz"
                    );

                    // Large buffer (1 MB)
                    const int bufferSize = 1024 * 1024;

                    // Batch size (number of lines before flushing)
                    const int batchSize = 10000;

                    using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.ASCII, bufferSize))
                    {
                        StringBuilder sb = new StringBuilder(batchSize * 40);
                        int counter = 0;

                        var culture = CultureInfo.InvariantCulture;

                        foreach (TinSurfaceVertex v in surface.Vertices)
                        {
                            var p = v.Location;

                            sb.Append(p.X.ToString("F3", culture));
                            sb.Append(',');
                            sb.Append(p.Y.ToString("F3", culture));
                            sb.Append(',');
                            sb.Append(p.Z.ToString("F3", culture));
                            sb.Append('\n');

                            counter++;

                            // Flush batch to disk
                            if (counter >= batchSize)
                            {
                                sw.Write(sb.ToString());
                                sb.Clear();
                                counter = 0;
                            }
                        }

                        // Write remaining data
                        if (sb.Length > 0)
                            sw.Write(sb.ToString());
                    }

                    ed.WriteMessage($"\nSurface exported:\n{filePath}");
                    tr.Commit();
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\nError: {ex.Message}");
                }
            }
        }

        [CommandMethod("PSV", "CogoPointsStationToAlignment", CommandFlags.Modal)]

        public static void CogoPointsStationToAlignment()
        {
            CivilDocument civDoc = CivilDocument.GetCivilDocument(AutocadDocumentService.Database);

            try
            {
                // --- Select Alignment ---
                PromptEntityOptions promptAlignment = new PromptEntityOptions("\nSelect alignment: ");
                promptAlignment.SetRejectMessage("\nMust be an alignment.");
                promptAlignment.AddAllowedClass(typeof(Alignment), exactMatch: false);

                PromptEntityResult resultAlignment = AutocadDocumentService.Editor.GetEntity(promptAlignment);
                if (resultAlignment.Status != PromptStatus.OK) return;

                ObjectId alignmentId = resultAlignment.ObjectId;

                // Reverse offset sign
                PromptKeywordOptions promptReverseSign = new PromptKeywordOptions("\nReverse offset sign?");
                promptReverseSign.Keywords.Add("Yes");
                promptReverseSign.Keywords.Add("No");

                // Optional: set default value
                promptReverseSign.Keywords.Default = "No";
                promptReverseSign.AllowNone = true;

                // Display options like [Yes/No]
                promptReverseSign.AppendKeywordsToMessage = true;

                PromptResult resultReverseSign = AutocadDocumentService.Editor.GetKeywords(promptReverseSign);

                if (resultReverseSign.Status != PromptStatus.OK)
                    return;

                bool reverseSign = resultReverseSign.StringResult == "Yes";

                switch (resultReverseSign.StringResult)
                {
                    case "Yes":
                        AutocadDocumentService.Editor.WriteMessage("\nOffset sign will be reversed.");
                        break;

                    case "No":
                        AutocadDocumentService.Editor.WriteMessage("\nOffset won't change.");
                        break;
                }

                // --- Select COGO Points ---
                PromptSelectionOptions pso = new PromptSelectionOptions();
                pso.MessageForAdding = "\nSelect COGO points: ";

                SelectionFilter filter = new SelectionFilter(
                    new TypedValue[] { new TypedValue(0, "AECC_COGO_POINT") }
                );

                PromptSelectionResult psr = AutocadDocumentService.Editor.GetSelection(pso, filter);
                if (psr.Status != PromptStatus.OK) return;

                // --- Ask for output TXT file path ---
                PromptSaveFileOptions fileOptions = new PromptSaveFileOptions("\nSave output TXT file: ");
                fileOptions.Filter = "Text File (*.txt)|*.txt";

                PromptFileNameResult fileResult = AutocadDocumentService.Editor.GetFileNameForSave(fileOptions);
                if (fileResult.Status != PromptStatus.OK) return;

                string filepath = fileResult.StringResult;

                using (Transaction tr = AutocadDocumentService.Database.TransactionManager.StartTransaction())
                using (StreamWriter sw = new StreamWriter(filepath))
                {
                    Alignment alignment = tr.GetObject(alignmentId, OpenMode.ForRead) as Alignment;

                    // Header line (optional)
                    sw.WriteLine("PointNumber;Description;X;Y;AlignmentStation;Offset");

                    foreach (SelectedObject selObj in psr.Value)
                    {
                        CogoPoint pt = tr.GetObject(selObj.ObjectId, OpenMode.ForRead) as CogoPoint;

                        if (pt != null)
                        {
                            double station = 0;
                            double offset = 0;
                            // Compute closest station
                            alignment.StationOffset(pt.Location.X, pt.Location.Y, ref station, ref offset);

                            offset = reverseSign ? -offset : offset;

                            string line = string.Format(
                                "{0};{1};{2:F3};{3:F3};{4:F3};{5:F3}",
                                pt.PointNumber,
                                pt.RawDescription,
                                pt.Easting,
                                pt.Northing,
                                station,
                                offset
                            );

                            sw.WriteLine(line);
                        }
                    }

                    tr.Commit();
                    AutocadDocumentService.Editor.WriteMessage($"\nExport complete: {filepath}");
                }
            }
            catch (System.Exception ex)
            {
                AutocadDocumentService.Editor.WriteMessage("\nError: " + ex.Message);
            }
        }

        [CommandMethod("PSV", "BermHeightReport", CommandFlags.Modal)]
        public static void BermHeightReport()
        {
            Log log = null;

            try
            {
                Document doc = AutocadDocumentService.ActiveDocument;
                Editor ed = doc.Editor;

                string drawingPath = doc.Name;
                string drawingFolder = Path.GetDirectoryName(drawingPath);

                log = new Log(drawingFolder, "BermHeightReport");
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
                        log.Information($"Detect extents for COGO point #{i + 1} (Handle: {point.Handle})");

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
                            PolylinePoints = polylinePoints
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

                List<TinSurfaceTriangle> filteredTriangles = triangles.Where(triangle => !TriangleOutsideExtents(triangle, totalExtent)).ToList();

                log.Information($"Triangles have been filtered, initial amount: {triangles.Count}, filtered amount: {filteredTriangles.Count}");

                using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
                {
                    log.Information("Transaction started.");

                    int i = 0;

                    foreach (CogoPointGeometry pointGeometry in cogoPointGeometries)
                    {
                        log.Information($"Detect MAIN surface points inside cogo point extents #{i + 1} (Handle: {pointGeometry.CogoPoint.Handle})");

                        var uniquePoints = new HashSet<Point3d>(new Point3dComparer(0.01));

                        int trianglesChecked = 0;
                        int pointsInside = 0;

                        foreach (TinSurfaceTriangle triangle in filteredTriangles)
                        {
                            trianglesChecked++;

                            if (TriangleOutsideExtents(triangle, pointGeometry.PolylineExtent))
                                continue;

                            Point3d[] triPts =
                            {
                                triangle.Vertex1.Location,
                                triangle.Vertex2.Location,
                                triangle.Vertex3.Location
                            };

                            foreach (Point3d pt in triPts)
                            {
                                if (uniquePoints.Contains(pt))
                                    continue;

                                if (IsPointInsidePolyline(pointGeometry.PolylinePoints, new Point2d(pt.X, pt.Y)))
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

                        double station = 0;
                        double offset = 0;
                        alignment.StationOffset(pointGeometry.CogoPoint.Location.X, pointGeometry.CogoPoint.Location.Y, ref station, ref offset);

                        boxAreas.Add(new BoxArea
                        {
                            Id = ++i,
                            MainCogoPoint = pointGeometry.CogoPoint,
                            MainComparisonPointPairs = pairs,
                            StationBoxCenter = station,
                        });
                    }

                    List<BoxAreaReportRow> boxAreaReportRows = new List<BoxAreaReportRow>();

                    foreach (BoxArea boxArea in boxAreas)
                    {
                        boxAreaReportRows.Add(new BoxAreaReportRow
                        {
                            Id = boxArea.Id,
                            Description = boxArea.Description,
                            BoxCenterX = boxArea.MainCogoPoint.Location.X,
                            BoxCenterY = boxArea.MainCogoPoint.Location.Y,
                            Count = boxArea.MainComparisonPointPairs.Count,
                            StationCenter = boxArea.StationBoxCenter,
                            ElevationDifferenceSum = boxArea.MainComparisonPointPairs.Sum(item => item.ElevationDifference),
                            ElevationDifferenceAverage = boxArea.MainComparisonPointPairs.Average(item => item.ElevationDifference),
                            ElevationDifferenceMax = boxArea.MainComparisonPointPairs.Max(item => item.ElevationDifference),
                            ElevationDifferenceMin = boxArea.MainComparisonPointPairs.Min(item => item.ElevationDifference),
                            Length = 0,
                            StationMax = 0,
                            StationMin = 0
                        });
                    }

                    ExportBoxAreasToExcel(boxAreaReportRows, drawingFolder, log);

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
            "Id", "StationCenter", "StationMin", "StationMax",
            "Description", "Length", "Count",
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

                    excelRow.CreateCell(0).SetCellValue(row.Id);
                    excelRow.CreateCell(1).SetCellValue(row.StationCenter);
                    excelRow.CreateCell(2).SetCellValue(row.StationMin);
                    excelRow.CreateCell(3).SetCellValue(row.StationMax);
                    excelRow.CreateCell(4).SetCellValue(row.Description ?? "");
                    excelRow.CreateCell(5).SetCellValue(row.Length);
                    excelRow.CreateCell(6).SetCellValue(row.Count);
                    excelRow.CreateCell(7).SetCellValue(row.ElevationDifferenceSum);
                    excelRow.CreateCell(8).SetCellValue(row.ElevationDifferenceAverage);
                    excelRow.CreateCell(9).SetCellValue(row.ElevationDifferenceMin);
                    excelRow.CreateCell(10).SetCellValue(row.ElevationDifferenceMax);
                    excelRow.CreateCell(11).SetCellValue(row.BoxCenterX);
                    excelRow.CreateCell(12).SetCellValue(row.BoxCenterY);
                }

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


        private static bool TriangleOutsideExtents(TinSurfaceTriangle tri, Extents3d ext)
        {
            return
                (tri.Vertex1.Location.X < ext.MinPoint.X && tri.Vertex2.Location.X < ext.MinPoint.X && tri.Vertex3.Location.X < ext.MinPoint.X) ||
                (tri.Vertex1.Location.X > ext.MaxPoint.X && tri.Vertex2.Location.X > ext.MaxPoint.X && tri.Vertex3.Location.X > ext.MaxPoint.X) ||
                (tri.Vertex1.Location.Y < ext.MinPoint.Y && tri.Vertex2.Location.Y < ext.MinPoint.Y && tri.Vertex3.Location.Y < ext.MinPoint.Y) ||
                (tri.Vertex1.Location.Y > ext.MaxPoint.Y && tri.Vertex2.Location.Y > ext.MaxPoint.Y && tri.Vertex3.Location.Y > ext.MaxPoint.Y);
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
