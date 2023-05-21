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
    public static class BaselineExtensions
    {
        public static List<AppliedAssembly> GetAppliedAssemblies(this Baseline baseline)
        {
            var appliedAssemblyList = new List<AppliedAssembly>();
            var stations = baseline.SortedStations();
            foreach (double station in stations)
            {
                var appliedAssembly = baseline.GetAppliedAssemblyAtStation(station);
                if (appliedAssembly != null)
                {
                    appliedAssemblyList.Add(appliedAssembly);
                }
            }
            return appliedAssemblyList;
        }

        public static Alignment GetAlignment(this Baseline baseline, OpenMode openMode)
        {
            var adoc = Application.DocumentManager.MdiActiveDocument;
            var db = adoc.Database;
            var cdoc = CivilDocument.GetCivilDocument(adoc.Database);

            Alignment alignment = null;

            using (adoc.LockDocument())
            {
                using (Transaction transaction = db.TransactionManager.StartTransaction())
                {
                    if (!baseline.AlignmentId.IsNull)
                    {
                        alignment = transaction.GetObject(baseline.AlignmentId, openMode, false, true) as Alignment;
                    }

                    transaction.Commit();
                }
            }

            return alignment;
        }
    }
}
