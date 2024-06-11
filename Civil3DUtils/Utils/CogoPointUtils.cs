using Autodesk.AutoCAD.ApplicationServices;
using Autocad = Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Civil = Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using AutoCADUtils;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DatabaseServices;

namespace Civil3DUtils.Utils
{
    public static class CogoPointUtils
    {
        public static Civil.CogoPoint CreateCogoPoint(Point3d point)
        {
            Document adoc = Application.DocumentManager.MdiActiveDocument;
            Autocad.Database db = adoc.Database;
            CivilDocument cdoc = CivilDocument.GetCivilDocument(adoc.Database);
            Editor ed = adoc.Editor;
            Civil.CogoPoint cogoPoint = null;

            using (AutocadDocumentService.LockActiveDocument())
            {
                using (Autocad.Transaction ts = db.TransactionManager.StartTransaction())
                {
                    Civil.CogoPointCollection cogoPoints = CivilApplication.ActiveDocument.CogoPoints;
                    var cogoPointId = cogoPoints.Add(point, true);
                    cogoPoint = ts.GetObject(cogoPointId, Autocad.OpenMode.ForWrite, false, true) as Civil.CogoPoint;
                    ts.Commit();
                }
            }

            return cogoPoint;
        }

        public static Civil.CogoPoint CreateCogoPoints(List<Point3d> points)
        {
            Document adoc = Application.DocumentManager.MdiActiveDocument;
            Autocad.Database db = adoc.Database;
            CivilDocument cdoc = CivilDocument.GetCivilDocument(adoc.Database);
            Editor ed = adoc.Editor;
            Civil.CogoPoint cogoPoint = null;

            using (AutocadDocumentService.LockActiveDocument())
            {
                using (Autocad.Transaction ts = db.TransactionManager.StartTransaction())
                {
                    foreach (Point3d point in points)
                    {
                        Civil.CogoPointCollection cogoPoints = CivilApplication.ActiveDocument.CogoPoints;
                        var cogoPointId = cogoPoints.Add(point, true);
                        cogoPoint = ts.GetObject(cogoPointId, Autocad.OpenMode.ForWrite, false, true) as Civil.CogoPoint;
                    }

                    ts.Commit();
                }
            }

            return cogoPoint;
        }

        public static List<Civil.CogoPoint> CreateCogoPoints(List<Point3d> points, string description)
        {
            Document adoc = Application.DocumentManager.MdiActiveDocument;
            Autocad.Database db = adoc.Database;
            CivilDocument cdoc = CivilDocument.GetCivilDocument(adoc.Database);
            Editor ed = adoc.Editor;

            List<CogoPoint> cogoPoints = new List<CogoPoint>();

            using (AutocadDocumentService.LockActiveDocument())
            {
                using (Autocad.Transaction ts = db.TransactionManager.StartTransaction())
                {
                    foreach (Point3d point in points)
                    {
                        Civil.CogoPointCollection cogoPointCollection = CivilApplication.ActiveDocument.CogoPoints;
                        ObjectId cogoPointId = cogoPointCollection.Add(point, description, true);
                        CogoPoint cogoPoint = ts.GetObject(cogoPointId, Autocad.OpenMode.ForWrite, false, true) as Civil.CogoPoint;
                        cogoPoints.Add(cogoPoint);
                    }

                    ts.Commit();
                }
            }

            return cogoPoints;
        }

        public static List<CogoPoint> PromptMultipleCogoPoints(OpenMode openMode, string messageForAdding = "Select COGO points")
        {
            List<CogoPoint> points = new List<CogoPoint>();
            PromptSelectionOptions opt = new PromptSelectionOptions();
            opt.MessageForAdding = messageForAdding;
            PromptSelectionResult promptResult = AutocadDocumentService.Editor.GetSelection(opt);

            // If the prompt status is OK, objects were selected
            if (promptResult.Status != PromptStatus.OK)
            {
                return points;
            }

            SelectionSet selectionSet = promptResult.Value;

            using (AutocadDocumentService.LockActiveDocument())
            {
                // Step through the objects in the selection set
                using (Transaction ts = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    foreach (SelectedObject selectedObject in selectionSet)
                    {
                        if (selectedObject.ObjectId.ObjectClass.Name == RXClass.GetClass(typeof(CogoPoint)).Name)
                        {
                            // Check to make sure a valid SelectedObject object was returned
                            if (selectedObject != null)
                            {
                                CogoPoint point = ts.GetObject(selectedObject.ObjectId, openMode, false, true) as CogoPoint;
                                points.Add(point);
                            }
                        }
                    }
                    ts.Commit();
                }
            }

            return points;
        }
    }
}
