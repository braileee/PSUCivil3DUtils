using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Civil = Autodesk.Civil.DatabaseServices;

namespace Civil3DUtils.Utils
{
    public static class AlignmentUtils
    {
        public static List<Civil.Alignment> GetAlignments(OpenMode openMode)
        {
            Document document = Application.DocumentManager.MdiActiveDocument;
            Database database = document.Database;
            CivilDocument civilDocument = CivilDocument.GetCivilDocument(document.Database);
            Editor editor = document.Editor;
            List<Civil.Alignment> oAlignmentList = new List<Civil.Alignment>();

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                ObjectIdCollection oAlignmentIdCollection = civilDocument.GetAlignmentIds();
                foreach (ObjectId oAlignmentId in oAlignmentIdCollection)
                {
                    Civil.Alignment oAlignment = transaction.GetObject(oAlignmentId, openMode, false, true) as Civil.Alignment;
                    oAlignmentList.Add(oAlignment);
                }
                transaction.Commit();
            }

            return oAlignmentList.OrderBy(item => item.Name).ToList();
        }


        public static Alignment GetAlignment(string message)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            PromptEntityOptions peo = new PromptEntityOptions($"\n{message}");
            peo.SetRejectMessage("\nPlease select a valid Alignment.");
            peo.AddAllowedClass(typeof(Alignment), exactMatch: true);

            PromptEntityResult result = ed.GetEntity(peo);

            if (result.Status != PromptStatus.OK)
                return null;

            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                Alignment alignment = tr.GetObject(result.ObjectId, OpenMode.ForRead) as Alignment;
                tr.Commit();
                return alignment;
            }
        }

    }
}
