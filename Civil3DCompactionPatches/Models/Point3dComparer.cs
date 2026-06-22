using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DCompactionPatches.Models
{
    public class Point3dComparer : IEqualityComparer<Point3d>
    {
        private readonly double tol;

        public Point3dComparer(double tolerance)
        {
            tol = tolerance;
        }

        public bool Equals(Point3d a, Point3d b)
        {
            return a.DistanceTo(b) <= tol;
        }

        public int GetHashCode(Point3d p)
        {
            return (
                Math.Round(p.X / tol),
                Math.Round(p.Y / tol),
                Math.Round(p.Z / tol)
            ).GetHashCode();
        }
    }
}
