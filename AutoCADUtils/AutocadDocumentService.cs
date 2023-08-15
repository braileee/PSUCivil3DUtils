using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADUtils
{
    public static class AutocadDocumentService
    {
        public static Document ActiveDocument
        {
            get
            {
                return Application.DocumentManager.MdiActiveDocument;
            }
        }

        public static DocumentCollection DocumentManager
        {
            get
            {
                return Application.DocumentManager;
            }
        }

        public static string ActiveDocumentFullPath
        {
            get
            {
                return ActiveDocument.Name;
            }
        }

        public static string DocumentNameWithoutExtension
        {
            get
            {
                return Path.GetFileNameWithoutExtension(Application.DocumentManager?.MdiActiveDocument?.Name);
            }
        }

        public static Database Database
        {
            get
            {
                return Application.DocumentManager.MdiActiveDocument.Database;
            }
        }

        public static Editor Editor
        {
            get
            {
                return Application.DocumentManager.MdiActiveDocument.Editor;
            }
        }

        public static Autodesk.AutoCAD.DatabaseServices.TransactionManager TransactionManager
        {
            get
            {
                return Database.TransactionManager;
            }
        }

        public static DocumentLock LockActiveDocument()
        {
            return ActiveDocument.LockDocument();
        }
    }
}
