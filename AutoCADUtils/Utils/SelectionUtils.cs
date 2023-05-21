using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADUtils.Utils
{
    public static class SelectionUtils
    {
        public static T GetElement<T>(string promptMessage, string rejectMessage = "Error")
        {
            Document adoc = Application.DocumentManager.MdiActiveDocument;
            Database db = adoc.Database;
            Editor ed = adoc.Editor;
            T oEntity = default;

            using (adoc.LockDocument())
            {
                using (Transaction ts = db.TransactionManager.StartTransaction())
                {
                    PromptEntityOptions opt = new PromptEntityOptions(promptMessage)
                    {
                        Message = promptMessage
                    };

                    opt.SetRejectMessage(rejectMessage);
                    ObjectId selectedObjId = ed.GetEntity(opt).ObjectId;

                    if (selectedObjId.IsNull)
                    {
                        ts.Commit();
                        return default;
                    }

                    object objEntity = ts.GetObject(selectedObjId, OpenMode.ForWrite, false, true) as object;

                    if (objEntity == null)
                    {
                        return default;
                    }

                    if (!objEntity.GetType().Name.Equals(typeof(T).Name))
                    {
                        return default;
                    }

                    oEntity = (T)objEntity;
                    if (oEntity == null)
                    {
                        return default;
                    }

                    ts.Commit();
                }
            }

            return oEntity;
        }
    }
}
