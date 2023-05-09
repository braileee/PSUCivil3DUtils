using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Civil.DatabaseServices.Styles;
using Autodesk.AutoCAD.Geometry;

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

            return pointStyles.OrderBy(style => style.Name).ToList();
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

            return labelStyles.OrderBy(item => item.Name).ToList();
        }

        public static PointStyle CreatePointStyle(string blockName, string pointStyleName)
        {
            Document document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database database = document.Database;
            CivilDocument civilDocument = CivilDocument.GetCivilDocument(document.Database);

            PointStyle pointStyle = null;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                if (civilDocument.Styles.PointStyles.Contains(pointStyleName))
                {
                    pointStyle = transaction.GetObject(civilDocument.Styles.PointStyles[pointStyleName], OpenMode.ForWrite, false, true) as PointStyle;
                }
                else
                {
                    ObjectId pointStyleId = civilDocument.Styles.PointStyles.Add(pointStyleName);
                    pointStyle = transaction.GetObject(pointStyleId, OpenMode.ForWrite, false, true) as PointStyle;
                    pointStyle.MarkerType = PointMarkerDisplayType.UseSymbolForMarker;
                    pointStyle.MarkerSymbolName = blockName;
                    pointStyle.MarkerFixedScale = new Point3d(1, 1, 1);
                    pointStyle.Orientation = MarkerOrientationType.OrientToWCS;
                    pointStyle.MarkerRotationAngle = 0;
                }

                transaction.Commit();
            }

            return pointStyle;
        }

        public static void ImportLabelStyles(string filePath, params string[] labelStyleNames)
        {
            DocumentCollection documentCollection = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            Document currentDocument = documentCollection.MdiActiveDocument;

            Editor ed = currentDocument.Editor;
            Database destinationDatabase = documentCollection.MdiActiveDocument.Database;
            Database sourceDatabase = new Database(false, true);
            CivilDocument sourceCivilDocument = CivilDocument.GetCivilDocument(sourceDatabase);

            try
            {
                // Read the DWG into a side database
                sourceDatabase.ReadDwgFile(filePath, System.IO.FileShare.Read, true, "");

                // Create a variable to store the list of block identifiers
                List<LabelStyle> labelStyles = new List<LabelStyle>();

                Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = sourceDatabase.TransactionManager;

                using (Transaction transaction = transactionManager.StartTransaction())
                {
                    LabelStyleCollection pointLabelStyles = sourceCivilDocument.Styles.LabelStyles.PointLabelStyles.LabelStyles;

                    foreach (string labelStyleName in labelStyleNames)
                    {
                        if (pointLabelStyles.Contains(labelStyleName))
                        {
                            LabelStyle labelStyle = transaction.GetObject(pointLabelStyles[labelStyleName], OpenMode.ForWrite, false, true) as LabelStyle;
                            labelStyles.Add(labelStyle);
                        }
                    }

                    transaction.Commit();
                }

                using (DocumentLock documentLock = currentDocument.LockDocument())
                {
                    foreach (LabelStyle labelStyle in labelStyles)
                    {
                        labelStyle.ExportTo(destinationDatabase, Autodesk.Civil.StyleConflictResolverType.Override);
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                ed.WriteMessage("\nError during copy: " + ex.Message);
            }
            sourceDatabase.Dispose();
        }

        public static void ImportStyles(string filePath, params string[] labelStyleNames)
        {
            DocumentCollection documentCollection = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            Document currentDocument = documentCollection.MdiActiveDocument;

            Editor ed = currentDocument.Editor;
            Database destinationDatabase = documentCollection.MdiActiveDocument.Database;
            Database sourceDatabase = new Database(false, true);
            CivilDocument sourceCivilDocument = CivilDocument.GetCivilDocument(sourceDatabase);
            CivilDocument destinationCivilDocument = CivilDocument.GetCivilDocument(destinationDatabase);

            try
            {
                // Read the DWG into a side database
                sourceDatabase.ReadDwgFile(filePath, System.IO.FileShare.Read, true, "");

                // Create a variable to store the list of block identifiers
                List<PointStyle> pointStyles = new List<PointStyle>();

                PointStyleCollection sourceLabelStyles = sourceCivilDocument.Styles.PointStyles;

                using (Transaction transaction = sourceDatabase.TransactionManager.StartTransaction())
                {
                    foreach (string labelStyleName in labelStyleNames)
                    {
                        if (sourceLabelStyles.Contains(labelStyleName))
                        {
                            PointStyle pointStyle = transaction.GetObject(sourceLabelStyles[labelStyleName], OpenMode.ForWrite, false, true) as PointStyle;
                            pointStyles.Add(pointStyle);
                        }
                    }

                    transaction.Commit();
                }

                using (DocumentLock documentLock = currentDocument.LockDocument())
                {
                    foreach (PointStyle pointStyle in pointStyles)
                    {
                        pointStyle.ExportTo(destinationDatabase, Autodesk.Civil.StyleConflictResolverType.Override);
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                ed.WriteMessage("\nError during copy: " + ex.Message);
            }
            sourceDatabase.Dispose();
        }
    }
}
