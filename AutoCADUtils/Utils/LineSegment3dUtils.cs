using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADUtils.Utils
{
    public static class LineSegment3dUtils
    {
        public static List<Point3d> GetSelfIntersectionPoints(List<LineSegment3d> lines, int tolerance)
        {
            Dictionary<string, Point3d> intersectionPoints = new Dictionary<string, Point3d>();

            foreach (LineSegment3d line1 in lines)
            {
                foreach (LineSegment3d line2 in lines)
                {

                    if (line1 == line2)
                    {
                        continue;
                    }

                    Point3d[] points = line2.IntersectWith(line1);

                    if (points == null)
                    {
                        continue;
                    }

                    List<Point3d> pointsToAdd = new List<Point3d>();

                    foreach (Point3d point in points)
                    {
                        if (line1.StartPoint.IsEqualTo(point) ||
                           line1.EndPoint.IsEqualTo(point) ||
                           line2.StartPoint.IsEqualTo(point) ||
                           line2.EndPoint.IsEqualTo(point))
                        {
                            continue;
                        }

                        string pointAsString = point.AsString(tolerance);

                        if (intersectionPoints.ContainsKey(pointAsString))
                        {
                            continue;
                        }

                        intersectionPoints.Add(pointAsString, point);
                    }
                }
            }

            return intersectionPoints.Values.ToList();
        }

        public static List<Point3d> GetSelfIntersectionPoints(List<LineSegment3d> lines, List<CircularArc3d> arcs, int tolerance)
        {
            Dictionary<string, Point3d> intersectionPoints = new Dictionary<string, Point3d>();

            foreach (LineSegment3d line1 in lines)
            {
                foreach (CircularArc3d arc1 in arcs)
                {
                    Point3d[] points = arc1.IntersectWith(line1);

                    if (points == null)
                    {
                        continue;
                    }

                    foreach (Point3d point in points)
                    {
                        if (line1.StartPoint.IsEqualTo(point) ||
                            line1.EndPoint.IsEqualTo(point) ||
                            arc1.StartPoint.IsEqualTo(point) ||
                            arc1.EndPoint.IsEqualTo(point))
                        {
                            continue;
                        }

                        string pointAsString = point.AsString(tolerance);

                        if (intersectionPoints.ContainsKey(pointAsString))
                        {
                            continue;
                        }

                        intersectionPoints.Add(pointAsString, point);
                    }
                }
            }

            foreach (LineSegment3d line1 in lines)
            {
                foreach (LineSegment3d line2 in lines)
                {

                    if (line1 == line2)
                    {
                        continue;
                    }

                    Point3d[] points = line2.IntersectWith(line1);

                    if (points == null)
                    {
                        continue;
                    }

                    List<Point3d> pointsToAdd = new List<Point3d>();

                    foreach (Point3d point in points)
                    {
                        if (line1.StartPoint.IsEqualTo(point) ||
                           line1.EndPoint.IsEqualTo(point) ||
                           line2.StartPoint.IsEqualTo(point) ||
                           line2.EndPoint.IsEqualTo(point))
                        {
                            continue;
                        }

                        string pointAsString = point.AsString(tolerance);

                        if (intersectionPoints.ContainsKey(pointAsString))
                        {
                            continue;
                        }

                        intersectionPoints.Add(pointAsString, point);
                    }
                }
            }

            return intersectionPoints.Values.ToList();
        }
    }
}
