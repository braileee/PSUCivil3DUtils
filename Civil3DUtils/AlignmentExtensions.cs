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
    public static class AlignmentExtensions
    {
        public static List<SampleLineGroup> GetSampleLineGroups(this Alignment alignment, OpenMode openMode)
        {
            ObjectIdCollection sampleLineGroupIds = alignment.GetSampleLineGroupIds();

            List<SampleLineGroup> groups = new List<SampleLineGroup>();

            var adoc = Application.DocumentManager.MdiActiveDocument;
            var db = adoc.Database;
            var cdoc = CivilDocument.GetCivilDocument(adoc.Database);

            using (adoc.LockDocument())
            {
                using (Transaction transaction = db.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId groupId in sampleLineGroupIds)
                    {
                        SampleLineGroup sampleLineGroup = transaction.GetObject(groupId, openMode, false, true) as SampleLineGroup;
                        groups.Add(sampleLineGroup);
                    }

                    transaction.Commit();
                }
            }

            return groups;
        }
    }
}
