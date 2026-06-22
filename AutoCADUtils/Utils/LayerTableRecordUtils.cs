using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADUtils.Utils
{
    public static class LayerTableRecordUtils
    {
        public static LayerTableRecord GetOrCreateLayer(string name, Transaction tr, OpenMode openMode)
        {

            LayerTable lt = (LayerTable)tr.GetObject(AutocadDocumentService.Database.LayerTableId, OpenMode.ForRead);

            LayerTableRecord newLayer = null;

            // 2. Check if layer exists
            if (!lt.Has(name))
            {
                // Upgrade to write because we will add a new layer
                lt.UpgradeOpen();

                newLayer = new LayerTableRecord
                {
                    Name = name
                };

                lt.Add(newLayer);
                tr.AddNewlyCreatedDBObject(newLayer, true);

                return newLayer;
            }
            else
            {
                ObjectId layerId = lt[name];
                LayerTableRecord layer = tr.GetObject(layerId, openMode, false, true) as LayerTableRecord;
                return layer;
            }
        }
    }
}
