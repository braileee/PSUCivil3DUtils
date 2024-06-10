using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADUtils.Utils
{
    public static class SolidUtils
    {
        public static List<Solid3d> PromptMultipleSolids3d(OpenMode openMode, string messageForAdding = "Select solids")
        {
            List<Solid3d> solids = new List<Solid3d>();
            PromptSelectionOptions opt = new PromptSelectionOptions();
            opt.MessageForAdding = messageForAdding;
            PromptSelectionResult promptResult = AutocadDocumentService.Editor.GetSelection(opt);

            // If the prompt status is OK, objects were selected
            if (promptResult.Status != PromptStatus.OK)
            {
                return solids;
            }

            SelectionSet selectionSet = promptResult.Value;

            using (AutocadDocumentService.LockActiveDocument())
            {
                // Step through the objects in the selection set
                using (Transaction ts = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    foreach (SelectedObject selectedObject in selectionSet)
                    {
                        if (selectedObject.ObjectId.ObjectClass.Name == RXClass.GetClass(typeof(Solid3d)).Name)
                        {
                            // Check to make sure a valid SelectedObject object was returned
                            if (selectedObject != null)
                            {
                                Solid3d solid3d = (Entity)ts.GetObject(selectedObject.ObjectId, openMode, false, true) as Solid3d;
                                solids.Add(solid3d);
                            }
                        }
                    }
                    ts.Commit();
                }
            }

            return solids;
        }
    }
}
