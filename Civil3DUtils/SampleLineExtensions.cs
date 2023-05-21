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
    public static class SampleLineExtensions
    {
        public static List<SectionView> GetSectionViews(this SampleLine line, OpenMode openMode)
        {
            List<SectionView> sectionViews = new List<SectionView>();

            var adoc = Application.DocumentManager.MdiActiveDocument;
            var db = adoc.Database;
            var cdoc = CivilDocument.GetCivilDocument(adoc.Database);

            using (Transaction transaction = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId sectionViewId in line.GetSectionViewIds())
                {
                    SectionView sectionView = transaction.GetObject(sectionViewId, openMode, false, true) as SectionView;
                    sectionViews.Add(sectionView);
                }

                transaction.Commit();
            }

            return sectionViews;
        }
    }
}