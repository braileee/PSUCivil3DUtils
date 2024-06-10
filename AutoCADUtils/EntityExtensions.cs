using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADUtils
{
    public static class EntityExtensions
    {
        public static void Move(this Entity entity, Transaction transaction, Point3d point, Point3d newPoint)
        {
            Matrix3d mat = Matrix3d.Displacement(point.GetVectorTo(newPoint));
            entity = transaction.GetObject(entity.Id, OpenMode.ForWrite) as Entity;
            entity.TransformBy(mat);
        }
    }
}
