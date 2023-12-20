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

                    object objEntity = ts.GetObject(selectedObjId, OpenMode.ForWrite, false, true);

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

        public static DBObject GetDbObject(string promptMessage, OpenMode openMode, string rejectMessage = "Error")
        {
            Document adoc = Application.DocumentManager.MdiActiveDocument;
            Database db = adoc.Database;
            Editor ed = adoc.Editor;
            DBObject oEntity = null;

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

                    oEntity = ts.GetObject(selectedObjId, openMode, false, true);

                    ts.Commit();
                }
            }

            return oEntity;
        }

        public static List<T> GetElements<T>(string message)
        {
            Document adoc = Application.DocumentManager.MdiActiveDocument;
            Database db = adoc.Database;
            Editor ed = adoc.Editor;
            PromptSelectionOptions opt = new PromptSelectionOptions();

            opt.MessageForAdding = message;


            List<T> elements = new List<T>();

            using (adoc.LockDocument())
            {
                using (Transaction ts = db.TransactionManager.StartTransaction())
                {
                    PromptSelectionResult pipesPrompt = ed.GetSelection();
                    if (pipesPrompt.Status == PromptStatus.OK)
                    {
                        SelectionSet selectionSet = pipesPrompt.Value;
                        foreach (SelectedObject selectedElement in selectionSet)
                        {
                            if (selectedElement == null)
                            {
                                continue;
                            }

                            object objEntity = ts.GetObject(selectedElement.ObjectId, OpenMode.ForWrite, false, true);

                            if (objEntity == null)
                            {
                                continue;
                            }

                            if (!objEntity.GetType().Name.Equals(typeof(T).Name))
                            {
                                continue;
                            }

                            T castedElement = (T)objEntity;

                            if (castedElement == null)
                            {
                                continue;
                            }

                            elements.Add(castedElement);
                        }
                    }
                    ts.Commit();
                }
            }

            return elements;
        }

        public static List<Entity> GetEntities(string message)
        {
            Document adoc = Application.DocumentManager.MdiActiveDocument;
            Database db = adoc.Database;
            Editor ed = adoc.Editor;
            PromptSelectionOptions opt = new PromptSelectionOptions();

            opt.MessageForAdding = message;


            List<Entity> elements = new List<Entity>();

            using (adoc.LockDocument())
            {
                using (Transaction ts = db.TransactionManager.StartTransaction())
                {
                    PromptSelectionResult pipesPrompt = ed.GetSelection();
                    if (pipesPrompt.Status == PromptStatus.OK)
                    {
                        SelectionSet selectionSet = pipesPrompt.Value;
                        foreach (SelectedObject selectedElement in selectionSet)
                        {
                            if (selectedElement == null)
                            {
                                continue;
                            }

                            object objEntity = ts.GetObject(selectedElement.ObjectId, OpenMode.ForWrite, false, true);

                            if (objEntity == null)
                            {
                                return default;
                            }

                            Entity entity = objEntity as Entity;

                            if (entity != null)
                            {
                                elements.Add(entity);
                            }

                        }
                    }
                    ts.Commit();
                }
            }

            return elements;
        }
    }
}
