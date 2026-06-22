using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DCompactionPatches.Models
{
    public sealed class TriangleData
    {
        public Point3d P1 { get; init; }
        public Point3d P2 { get; init; }
        public Point3d P3 { get; init; }

        public double MinX { get; init; }
        public double MaxX { get; init; }
        public double MinY { get; init; }
        public double MaxY { get; init; }

        public static TriangleData ToTriangleData(TinSurfaceTriangle tri)
        {
            Point3d p1 = tri.Vertex1.Location;
            Point3d p2 = tri.Vertex2.Location;
            Point3d p3 = tri.Vertex3.Location;

            return new TriangleData
            {
                P1 = p1,
                P2 = p2,
                P3 = p3,
                MinX = Math.Min(p1.X, Math.Min(p2.X, p3.X)),
                MaxX = Math.Max(p1.X, Math.Max(p2.X, p3.X)),
                MinY = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y)),
                MaxY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y))
            };
        }

        public static bool TriangleOutsideExtents(TriangleData tri, Extents3d ext)
        {
            return tri.MaxX < ext.MinPoint.X ||
                   tri.MinX > ext.MaxPoint.X ||
                   tri.MaxY < ext.MinPoint.Y ||
                   tri.MinY > ext.MaxPoint.Y;
        }

    }
}