using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Civil3DToolbox.Models;
using Civil3DUtils.Utils;
using CoreUtils;
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

        [CommandMethod("PSV", "CreateCOGOPointsFromBlocks", CommandFlags.Modal)]
        public static void CreateCOGOPointsFromBlocks()
        {
            try
            {
                List<BlockReference> blockReferences =
                    SelectionUtils.GetElements<BlockReference>("Select blocks");

                string description =
                    PromptUtils.PromptString("Input raw description:");

                if (string.IsNullOrWhiteSpace(description))
                    return;

                PromptKeywordOptions promptRotation = new PromptKeywordOptions("\nApply block reference rotation to cogo points [Yes/No]: ", "Yes No")
                {
                    AllowNone = false
                };

                var rotationResult = AutocadDocumentService.Editor.GetKeywords(promptRotation);
                if (rotationResult.Status != PromptStatus.OK) return;

                bool applyRotation = rotationResult.StringResult == "Yes";

                List<Point3d> points = new List<Point3d>();
                List<double> rotations = new List<double>();

                using (Transaction transaction =
                       AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    foreach (BlockReference blockReference in blockReferences)
                    {
                        points.Add(blockReference.Position);

                        if (applyRotation)
                        {
                            // Store rotation (in radians)
                            rotations.Add(blockReference.Rotation);
                        }
                        else
                        {
                            rotations.Add(0.0);
                        }
                    }

                    List<CogoPoint> cogoPoints =
                        CogoPointUtils.CreateCogoPoints(points, description);

                    // ✅ Apply rotation to created COGO points
                    if (applyRotation && cogoPoints != null)
                    {
                        for (int i = 0; i < cogoPoints.Count; i++)
                        {
                            cogoPoints[i].LabelRotation = rotations[i];
                            cogoPoints[i].MarkerRotation = rotations[i];
                        }
                    }

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

        
    }
}
