using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Civil = Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Civil.DatabaseServices.Styles;
using System.Windows;

namespace Civil3DAlignmentStationCoordinatesTable.Utils
{
    public static class PointUtils
    {
        public static List<PointStyle> GetPointStyles(OpenMode openMode)
        {
            Document document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database database = document.Database;
            CivilDocument civilDocument = CivilDocument.GetCivilDocument(document.Database);
            Editor editor = document.Editor;

            List<PointStyle> pointStyles = new List<PointStyle>();

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                PointStyleCollection pointStyleCollection = civilDocument.Styles.PointStyles;

                foreach (ObjectId pointStyleId in pointStyleCollection)
                {
                    PointStyle pointStyle = transaction.GetObject(pointStyleId, openMode, false, true) as PointStyle;
                    pointStyles.Add(pointStyle);
                }
                transaction.Commit();
            }

            return pointStyles;
        }

        public static List<LabelStyle> GetPointLabelStyles(OpenMode openMode)
        {
            Document document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database database = document.Database;
            CivilDocument civilDocument = CivilDocument.GetCivilDocument(document.Database);
            Editor editor = document.Editor;

            List<LabelStyle> labelStyles = new List<LabelStyle>();

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                LabelStyleCollection labelStyleCollection = civilDocument.Styles.LabelStyles.PointLabelStyles.LabelStyles;

                foreach (ObjectId labelStyleId in labelStyleCollection)
                {
                    LabelStyle labelStyle = transaction.GetObject(labelStyleId, openMode, false, true) as LabelStyle;
                    labelStyles.Add(labelStyle);
                }
                transaction.Commit();
            }

            return labelStyles;
        }
    }
}
