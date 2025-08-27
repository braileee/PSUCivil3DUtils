using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;

namespace Civil3DDataExport
{
    public static class App
    {
        public static Document ActiveDocumentAutocad
        {
            get
            {
                return Application.DocumentManager.MdiActiveDocument;
            }
        }

        public static Database Database
        {
            get
            {
                return ActiveDocumentAutocad.Database;
            }
        }

        public static Autodesk.AutoCAD.ApplicationServices.TransactionManager TransactionManager
        {
            get
            {
                return ActiveDocumentAutocad.TransactionManager;
            }
        }


        public static CivilDocument ActiveDocumentCivil
        {
            get
            {
                return CivilDocument.GetCivilDocument(Database);
            }
        }

        public static Editor Editor
        {
            get
            {
                return ActiveDocumentAutocad.Editor;
            }
        }
    }
}
