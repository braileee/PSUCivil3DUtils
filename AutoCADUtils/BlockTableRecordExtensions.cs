using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADUtils
{
    public static class BlockTableRecordExtensions
    {
        public static List<Autodesk.AutoCAD.DatabaseServices.Entity> GetEntitiesInside(this BlockTableRecord blockTableRecord, Transaction tr)
        {
            List<Autodesk.AutoCAD.DatabaseServices.Entity> entities = new List<Autodesk.AutoCAD.DatabaseServices.Entity>();

            foreach (ObjectId entId in blockTableRecord)
            {
                Autodesk.AutoCAD.DatabaseServices.Entity ent = tr.GetObject(entId, OpenMode.ForRead) as Autodesk.AutoCAD.DatabaseServices.Entity;
                entities.Add(ent);
            }

            return entities;
        }
    }
}
