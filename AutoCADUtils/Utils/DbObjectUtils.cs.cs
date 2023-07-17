using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace AutoCADUtils.Utils
{
    public class DbObjectUtils
    {
        public static List<DBObject> GetObjectsFromModel(string objectClassName, OpenMode openMode)
        {
            var oElements = new List<DBObject>();


            var doc = Application.DocumentManager.MdiActiveDocument;
            using (var ts = doc.TransactionManager.StartOpenCloseTransaction())
            {
                var modelspace = (BlockTableRecord)ts.GetObject(
                            SymbolUtilityServices.GetBlockModelSpaceId(doc.Database), openMode, false, true);

                var elementIds = (from id in modelspace.Cast<ObjectId>()
                                  where id.ObjectClass.Name.Equals(objectClassName)
                                  select id).ToList();

                foreach (ObjectId polyId in elementIds)
                {
                    var oElement = ts.GetObject(polyId, OpenMode.ForWrite, false, true) as DBObject;
                    oElements.Add(oElement);
                }
                ts.Commit();
            }
            return oElements;
        }

        public static List<DBObject> GetObjectsFromModelByDxfName(string dxfName, OpenMode openMode)
        {
            var oElements = new List<DBObject>();


            var doc = Application.DocumentManager.MdiActiveDocument;
            using (var ts = doc.TransactionManager.StartOpenCloseTransaction())
            {
                var modelspace = (BlockTableRecord)ts.GetObject(
                            SymbolUtilityServices.GetBlockModelSpaceId(doc.Database), openMode, false, true);

                var elementIds = (from id in modelspace.Cast<ObjectId>()
                                  where id.ObjectClass.DxfName.Equals(dxfName)
                                  select id).ToList();

                foreach (ObjectId polyId in elementIds)
                {
                    var oElement = ts.GetObject(polyId, OpenMode.ForWrite, false, true) as DBObject;
                    oElements.Add(oElement);
                }
                ts.Commit();
            }
            return oElements;
        }
    }
}
