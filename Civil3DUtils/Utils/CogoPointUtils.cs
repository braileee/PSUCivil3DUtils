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
    }
}
