using AutoCADUtils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Autocad = Autodesk.AutoCAD.DatabaseServices;
using Civil = Autodesk.Civil.DatabaseServices;

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
            // Selection prompt
            PromptSelectionOptions pso = new PromptSelectionOptions
            {
                MessageForAdding = "\nSelect COGO Points: ",
                AllowDuplicates = false
            };

            // Filter for COGO points
            TypedValue[] filterValues = new TypedValue[]
            {
                    new TypedValue((int)DxfCode.Start, "AECC_COGO_POINT")
            };

            SelectionFilter filter = new SelectionFilter(filterValues);

            // Get selection
            PromptSelectionResult psr = AutocadDocumentService.Editor.GetSelection(pso, filter);

            if (psr.Status != PromptStatus.OK)
                return new List<CogoPoint>();

            List<CogoPoint> points = new List<CogoPoint>();

            using (Transaction tr = AutocadDocumentService.Database.TransactionManager.StartTransaction())
            {
                foreach (SelectedObject selObj in psr.Value)
                {
                    if (selObj == null)
                    {
                        continue;
                    }

                    CogoPoint point = tr.GetObject(selObj.ObjectId, openMode, false, true) as CogoPoint;

                    points.Add(point);
                }

                tr.Commit();
            }

            return points;
        }

        public static PointGroup CreateCogoPointGroup(string pointGroupName)
        {
            PointGroup pointGroup = null;

            using (AutocadDocumentService.LockActiveDocument())
            {
                using (Autocad.Transaction ts = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    ObjectId pointGroupId = ObjectId.Null;

                    if (CivilDocumentService.CivilDocument.PointGroups.Contains(pointGroupName))
                    {
                        pointGroupId = CivilDocumentService.CivilDocument.PointGroups[pointGroupName];
                    }
                    else
                    {
                        pointGroupId = CivilDocumentService.CivilDocument.PointGroups.Add(pointGroupName);
                    }

                    if (pointGroupId == ObjectId.Null)
                    {
                        return pointGroup;
                    }

                    pointGroup = ts.GetObject(pointGroupId, OpenMode.ForWrite, false, true) as PointGroup;
                    ts.Commit();
                }
            }

            return pointGroup;
        }
    }
}
