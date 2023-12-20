using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;
using Autodesk.Civil.DatabaseServices;
using Civil3DUtils.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

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
                            ObjectIdCollection xrefIdCollection = new ObjectIdCollection();

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

                                foreach (ObjectId xrefId in xrefIdCollection)
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


        [CommandMethod("PSV", "GetPolylineSelfintersection", CommandFlags.Modal)]
        public static void GetPolylineSelfintersection()
        {
            try
            {
                Document document = null;

                Autodesk.AutoCAD.DatabaseServices.DBObject dbObject = SelectionUtils.GetDbObject("Select polyline", OpenMode.ForRead);

                int tolerance = 8;

                if (!(dbObject is Polyline))
                {
                    return;
                }

                Polyline polyline = (Polyline)dbObject;

                List<CircularArc3d> arcs = new List<CircularArc3d>();
                List<LineSegment3d> lines = new List<LineSegment3d>();
                polyline.GetSegments(ref lines, ref arcs);

                Dictionary<string, Point3d> intersectionPoints = new Dictionary<string, Point3d>();

                foreach (LineSegment3d line1 in lines)
                {
                    foreach (CircularArc3d arc1 in arcs)
                    {
                        Point3d[] points = arc1.IntersectWith(line1);

                        if(points == null)
                        {
                            continue;
                        }

                        foreach (Point3d point in points)
                        {
                            if (line1.StartPoint.IsEqualTo(point) ||
                                line1.EndPoint.IsEqualTo(point) ||
                                arc1.StartPoint.IsEqualTo(point) ||
                                arc1.EndPoint.IsEqualTo(point))
                            {
                                continue;
                            }

                            string pointAsString = point.AsString(tolerance);

                            if (intersectionPoints.ContainsKey(pointAsString))
                            {
                                continue;
                            }

                            intersectionPoints.Add(pointAsString, point);
                        }
                    }
                }

                foreach (LineSegment3d line1 in lines)
                {
                    foreach (LineSegment3d line2 in lines)
                    {

                        if (line1 == line2)
                        {
                            continue;
                        }

                        Point3d[] points = line2.IntersectWith(line1);

                        if (points == null)
                        {
                            continue;
                        }

                        List<Point3d> pointsToAdd = new List<Point3d>();

                        foreach (Point3d point in points)
                        {
                            if (line1.StartPoint.IsEqualTo(point) ||
                               line1.EndPoint.IsEqualTo(point) ||
                               line2.StartPoint.IsEqualTo(point) ||
                               line2.EndPoint.IsEqualTo(point))
                            {
                                continue;
                            }

                            string pointAsString = point.AsString(tolerance);

                            if (intersectionPoints.ContainsKey(pointAsString))
                            {
                                continue;
                            }

                            intersectionPoints.Add(pointAsString, point);
                        }
                    }
                }

                CogoPointUtils.CreateCogoPoints(intersectionPoints.Values.ToList());

                MessageBox.Show($"Points found: {intersectionPoints.Count}");
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message, "Error");
            }
        }
    }
}
