using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
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
    }
}
