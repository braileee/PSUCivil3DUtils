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
        public static Point3d? GetIntersectionPoint(LineSegment3d line1, LineSegment3d line2, int tolerance)
        {
            return GetIntersectionPoints(new List<LineSegment3d> { line1 }, new List<LineSegment3d> { line2 }, tolerance).FirstOrDefault();
        }

        public static List<Point3d> GetIntersectionPoints(List<LineSegment3d> linesSet1, List<LineSegment3d> linesSet2, int tolerance)
        {
            Dictionary<string, Point3d> intersectionPoints = new Dictionary<string, Point3d>();

            foreach (LineSegment3d line1 in linesSet1)
            {
                foreach (LineSegment3d line2 in linesSet2)
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

        public static List<Point3d> GetIntersectionPoints(List<LineSegment3d> linesSet1, List<CircularArc3d> arcsSet1, List<LineSegment3d> linesSet2, List<CircularArc3d> arcsSet2, int tolerance)
        {
            Dictionary<string, Point3d> intersectionPoints = new Dictionary<string, Point3d>();

            foreach (LineSegment3d line1 in linesSet1)
            {
                foreach (CircularArc3d arc1 in arcsSet2)
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

            foreach (LineSegment3d line1 in linesSet1)
            {
                foreach (LineSegment3d line2 in linesSet2)
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


            foreach (LineSegment3d line1 in linesSet2)
            {
                foreach (CircularArc3d arc1 in arcsSet1)
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

            foreach (CircularArc3d arc1 in arcsSet1)
            {
                foreach (CircularArc3d arc2 in arcsSet2)
                {
                    Point3d[] points = arc2.IntersectWith(arc1);

                    if (points == null)
                    {
                        continue;
                    }

                    foreach (Point3d point in points)
                    {
                        if (arc1.StartPoint.IsEqualTo(point) ||
                            arc1.EndPoint.IsEqualTo(point) ||
                            arc2.StartPoint.IsEqualTo(point) ||
                            arc2.EndPoint.IsEqualTo(point))
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
