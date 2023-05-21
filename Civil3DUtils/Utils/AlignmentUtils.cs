using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
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
    }
}
