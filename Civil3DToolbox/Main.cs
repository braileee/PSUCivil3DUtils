using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
