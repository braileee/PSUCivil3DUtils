using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using System;

namespace Civil3DUtils.Utils
{
    public static class CorridorUtils
    {
        public static Corridor PromptCorridor(OpenMode openMode, string promptMessage = "Select a corridor")
        {
            var adoc = Application.DocumentManager.MdiActiveDocument;
            var db = adoc.Database;
            var cdoc = CivilDocument.GetCivilDocument(adoc.Database);
            Editor ed = adoc.Editor;

            Corridor corridor = null;

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

                    corridor = ts.GetObject(corridorObjId, openMode, false, true) as Corridor;
                    ts.Commit();
                }
            }

            return corridor;
        }
    }
}
