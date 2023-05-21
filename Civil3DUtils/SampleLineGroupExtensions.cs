using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DUtils
{
    public static class SampleLineGroupExtensions
    {
        public static List<SampleLine> GetSampleLines(this SampleLineGroup sampleLineGroup, OpenMode openMode)
        {
            List<SampleLine> sampleLines = new List<SampleLine>();

            var adoc = Application.DocumentManager.MdiActiveDocument;
            var db = adoc.Database;
            var cdoc = CivilDocument.GetCivilDocument(adoc.Database);

            using (adoc.LockDocument())
            {
                using (Transaction transaction = db.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId objectId in sampleLineGroup.GetSampleLineIds())
                    {
                        SampleLine sampleLine = transaction.GetObject(objectId, openMode, false, true) as SampleLine;
                        sampleLines.Add(sampleLine);
                    }

                    transaction.Commit();
                }
            }

            return sampleLines;
        }
    }
}
