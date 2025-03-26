using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DUtils
{
    public static class PipeExtensions
    {
        public static Point3d? GetIntersectionPoint(this Pipe pipe, Pipe otherPipe, int tolerance)
        {
            LineSegment3d segment1 = new LineSegment3d(pipe.StartPoint, pipe.EndPoint);
            LineSegment3d segment2 = new LineSegment3d(otherPipe.StartPoint, otherPipe.EndPoint);

            return LineSegment3dUtils.GetIntersectionPoint(segment1, segment2, tolerance);
        }

        public static Point3d? GetIntersectionPointsBy2d(this Pipe pipe, Pipe otherPipe, int tolerance, double elevation)
        {
            LineSegment3d segment1 = new LineSegment3d(pipe.StartPoint.ToElevation(elevation), pipe.EndPoint.ToElevation(elevation));
            LineSegment3d segment2 = new LineSegment3d(otherPipe.StartPoint.ToElevation(elevation), otherPipe.EndPoint.ToElevation(elevation));

            return LineSegment3dUtils.GetIntersectionPoint(segment1, segment2, tolerance);
        }
    }
}
