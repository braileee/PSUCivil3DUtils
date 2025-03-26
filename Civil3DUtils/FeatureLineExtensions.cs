using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DUtils
{
    public static class FeatureLineExtensions
    {
        public static List<LineSegment3d> GetSegments(this FeatureLine featureLine, int tolerance)
        {
            Point3dCollection points = featureLine.GetPoints(Autodesk.Civil.FeatureLinePointType.AllPoints);
            List<LineSegment3d> segments = new List<LineSegment3d>();

            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0)
                {
                    continue;
                }

                Point3d previousPoint = points[i - 1];
                Point3d currentPoint = points[i];

                if (previousPoint.IsEqualByXYZ(currentPoint, tolerance))
                {
                    continue;
                }

                LineSegment3d segment = new LineSegment3d(previousPoint, currentPoint);
                segments.Add(segment);
            }

            return segments;
        }

        public static List<LineSegment3d> GetSegmentsWithElevation(this FeatureLine featureLine, int tolerance, double elevation)
        {
            Point3dCollection points = featureLine.GetPoints(Autodesk.Civil.FeatureLinePointType.AllPoints);
            List<LineSegment3d> segments = new List<LineSegment3d>();

            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0)
                {
                    continue;
                }

                Point3d previousPoint = points[i - 1];
                Point3d currentPoint = points[i];

                Point3d flattenPreviousPoint = new Point3d(previousPoint.X, previousPoint.Y, elevation);
                Point3d flattenCurrentPoint = new Point3d(currentPoint.X, currentPoint.Y, elevation);

                if (flattenPreviousPoint.IsEqualByXYZ(flattenCurrentPoint, tolerance))
                {
                    continue;
                }

                LineSegment3d segment = new LineSegment3d(flattenPreviousPoint, currentPoint);
                segments.Add(segment);
            }

            return segments;
        }

        public static List<Point3d> GetSelfIntersectionPoints(this FeatureLine featureLine, int tolerance)
        {
            List<LineSegment3d> segments = featureLine.GetSegments(tolerance);
            return LineSegment3dUtils.GetSelfIntersectionPoints(segments, tolerance);
        }

        public static List<Point3d> GetIntersectionPoints(this FeatureLine featureLine, FeatureLine otherFeatureLine, int tolerance)
        {
            List<LineSegment3d> segments1 = featureLine.GetSegments(tolerance);
            List<LineSegment3d> segments2 = otherFeatureLine.GetSegments(tolerance);

            return LineSegment3dUtils.GetIntersectionPoints(segments1, segments2, tolerance);
        }

        public static List<Point3d> GetSelfIntersectionPointsBy2d(this FeatureLine featureLine, int tolerance, double elevation)
        {
            List<LineSegment3d> lines = featureLine.GetSegmentsWithElevation(tolerance, elevation);
            return LineSegment3dUtils.GetSelfIntersectionPoints(lines, tolerance);
        }

        public static List<Point3d> GetIntersectionPointsBy2d(this FeatureLine featureLine, FeatureLine otherFeatureLine, int tolerance, double elevation)
        {
            List<LineSegment3d> lines1 = featureLine.GetSegmentsWithElevation(tolerance, elevation);
            List<LineSegment3d> lines2 = otherFeatureLine.GetSegmentsWithElevation(tolerance, elevation);

            return LineSegment3dUtils.GetIntersectionPoints(lines1, lines2, tolerance);
        }
    }
}
