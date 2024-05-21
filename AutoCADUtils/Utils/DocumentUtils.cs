using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADUtils.Utils
{
    public static class DocumentUtils
    {
        public static Autodesk.AutoCAD.DatabaseServices.TransactionManager TransactionManager
        {
            get
            {
                return AutocadDocumentService.TransactionManager;
            }
        }

        public static Document Document
        {
            get
            {
                return AutocadDocumentService.ActiveDocument;
            }
        }

        public static Editor Editor
        {
            get
            {
                return AutocadDocumentService.ActiveDocument.Editor;
            }
        }

        public static DocumentLock LockActiveDocument()
        {
           return Document.LockDocument();
        }
    }
}
