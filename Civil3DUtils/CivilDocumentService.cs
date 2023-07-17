using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DUtils
{
    public static class CivilDocumentService
    {
        public static CivilDocument CivilDocument
        {
            get
            {
                Database database = Application.DocumentManager.MdiActiveDocument.Database;
                return CivilDocument.GetCivilDocument(database);
            }
        }
    }
}
