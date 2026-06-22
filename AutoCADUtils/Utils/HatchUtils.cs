using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADUtils.Utils
{
    public static class HatchUtils
    {
        public static Hatch Create(BlockTableRecord blockTableRecord, Transaction transaction, string layerName, Entity entity)
        {
            // Create Hatch
            Hatch hatch = new Hatch();
            hatch.SetDatabaseDefaults();
            hatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");

            blockTableRecord.AppendEntity(hatch);
            transaction.AddNewlyCreatedDBObject(hatch, true);

            hatch = transaction.GetObject(hatch.Id, OpenMode.ForWrite, false, true) as Hatch;

            hatch.Layer = layerName;
            hatch.Associative = true;

            // Append boundary (polyline)
            ObjectIdCollection boundaryIds = new ObjectIdCollection();
            boundaryIds.Add(entity.ObjectId);

            hatch.AppendLoop(HatchLoopTypes.Default, boundaryIds);

            // Evaluate hatch (VERY IMPORTANT)
            hatch.EvaluateHatch(true);

            return hatch;
        }
    }
}
