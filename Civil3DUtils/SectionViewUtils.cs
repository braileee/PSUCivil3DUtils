using Autodesk.AutoCAD.ApplicationServices;
using Autocad = Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Civil = Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using AutoCADUtils;

namespace Civil3DUtils
{
    public static class SectionViewUtils
    {
        public static Civil.SectionView PromptASectionView(string promptMessage, OpenMode openMode, string rejectMessage = "Error")
        {
            Document adoc = Application.DocumentManager.MdiActiveDocument;
            Autocad.Database db = adoc.Database;
            CivilDocument cdoc = CivilDocument.GetCivilDocument(adoc.Database);
            Editor ed = adoc.Editor;
            PromptEntityOptions opt = new PromptEntityOptions(promptMessage);
            opt.Message = promptMessage;
            opt.SetRejectMessage(rejectMessage);
            Civil.SectionView oSectionView = null;

            using (AutocadDocumentService.LockActiveDocument())
            {
                using (Autocad.Transaction ts = db.TransactionManager.StartTransaction())
                {
                    Civil.Entity sectionViewEnt = (Civil.Entity)ts.GetObject(ed.GetEntity(opt).ObjectId, openMode, false, true);
                    oSectionView = ts.GetObject(sectionViewEnt.ObjectId, Autocad.OpenMode.ForWrite, true, true) as Civil.SectionView;
                    ts.Commit();
                }
            }

            return oSectionView;
        }

    }
}
