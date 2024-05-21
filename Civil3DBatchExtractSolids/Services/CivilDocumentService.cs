using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DBatchExtractSolids.Services
{
    public static class CivilDocumentService
    {
        public static CivilDocument CivilDocument
        {
            get
            {
                return CivilDocument.GetCivilDocument(AutocadDocumentService.Database);
            }
        }

    }
}
