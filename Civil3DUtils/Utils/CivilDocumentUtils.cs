using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;

namespace Civil3DUtils.Utils
{
    public static class CivilDocumentUtils
    {
        public static CivilDocument CivilDocument
        {
            get
            {
                Database database = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Database;
                return CivilDocument.GetCivilDocument(database);
            }
        }

    }
}
