using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADUtils.Utils
{
    public static class DimensionUtils
    {
        public static void CreateLinearDimension(Point3d pointStart, Point3d pointEnd, double dimLinkOffsetY)
        {
            // Get the current database
            Document document = Application.DocumentManager.MdiActiveDocument;
            Database database = document.Database;
            // Start a transaction

            using (document.LockDocument())
            {
                using (Transaction transaction = database.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read
                    BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write
                    BlockTableRecord blockTableRecord = transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    // Create the rotated dimension
                    RotatedDimension acRotDim = new RotatedDimension();
                    acRotDim.SetDatabaseDefaults();
                    acRotDim.XLine1Point = pointStart;
                    acRotDim.XLine2Point = pointEnd;
                    acRotDim.DimLinePoint = pointStart.GetMiddlePointWith(pointEnd).Add(new Vector3d(0, dimLinkOffsetY, 0));
                    acRotDim.DimensionStyle = database.Dimstyle;
                    // Add the new object to Model space and the transaction
                    blockTableRecord.AppendEntity(acRotDim);
                    transaction.AddNewlyCreatedDBObject(acRotDim, true);
                    // Commit the changes and dispose of the transaction
                    transaction.Commit();
                }
            }
        }
    }
}
