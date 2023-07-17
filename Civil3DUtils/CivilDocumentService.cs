using AutoCADUtils;
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
                Database database = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
                return CivilDocument.GetCivilDocument(database);
            }
        }

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
    }
}
