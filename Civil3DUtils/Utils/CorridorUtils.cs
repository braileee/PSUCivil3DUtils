using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Civil = Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Civil3DUtils.Utils
{
    public static class CorridorUtils
    {
        public static Civil.Corridor PromptCorridor(OpenMode openMode, string promptMessage = "Select a corridor")
        {
            var adoc = Application.DocumentManager.MdiActiveDocument;
            var db = adoc.Database;
            var cdoc = CivilDocument.GetCivilDocument(adoc.Database);
            Editor ed = adoc.Editor;

            Civil.Corridor corridor = null;

            using (adoc.LockDocument())
            {
                using (Transaction ts = db.TransactionManager.StartTransaction())
                {
                    var opt = new PromptEntityOptions($"{Environment.NewLine}{promptMessage}{Environment.NewLine}");
                    opt.Message = promptMessage;
                    var corridorObjId = ed.GetEntity(opt).ObjectId;

                    if (corridorObjId.IsNull)
                    {
                        return null;
                    }

                    corridor = ts.GetObject(corridorObjId, openMode, false, true) as Civil.Corridor;
                    ts.Commit();
                }
            }

            return corridor;
        }

        public static List<Civil.Corridor> GetAllTheCorridors(OpenMode openMode)
        {
            Document adoc = Application.DocumentManager.MdiActiveDocument;
            Database db = adoc.Database;
            CivilDocument cdoc = CivilDocument.GetCivilDocument(adoc.Database);
            Editor ed = adoc.Editor;
            var corridorList = new List<Civil.Corridor>();
            var corridors = cdoc.CorridorCollection;

            using (Transaction ts = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId corridorId in corridors)
                {
                    var oCorridor = ts.GetObject(corridorId, openMode, false, true) as Civil.Corridor;
                    corridorList.Add(oCorridor);
                }
                ts.Commit();
            }

            return corridorList.OrderBy(s => s.Name).ToList();
        }
    }
}
