using AutoCADUtils.Utils;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoCADUtils
{
    public static class PolylineExtensions
    {
        public static void GetSegments(this Polyline polyline, ref List<LineSegment3d> lines, ref List<CircularArc3d> arcs)
        {
            for (int i = 0; i < polyline.NumberOfVertices - 1; i++)
            {
                switch (polyline.GetSegmentType(i))
                {
                    case SegmentType.Line:
                        LineSegment3d lineSegment3D = polyline.GetLineSegmentAt(i);
                        lines.Add(lineSegment3D);
                        break;
                    case SegmentType.Arc:
                        CircularArc3d circularArc = polyline.GetArcSegmentAt(i);
                        arcs.Add(circularArc);
                        break;
                    case SegmentType.Coincident:
                        break;
                    case SegmentType.Point:
                        break;
                    case SegmentType.Empty:
                        break;
                    default:
                        break;
                }
            }
        }

        public static List<PolylineVertex3d> GetVertexes(this Polyline3d polyline)
        {
            List<PolylineVertex3d> vertexes = new List<PolylineVertex3d>();

            using (DocumentUtils.LockActiveDocument())
            {
                using (Transaction transaction = DocumentUtils.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId objectId in polyline)
                    {
                        object polylineObject = transaction.GetObject(objectId, OpenMode.ForRead, false, true);

                        if (polylineObject is PolylineVertex3d vertex)
                        {
                            vertexes.Add(vertex);
                        }
                    }

                    transaction.Commit();
                }
            }

            return vertexes;
        }

        public static List<LineSegment3d> GetSegments(this Polyline3d polyline, int tolerance)
        {
            List<PolylineVertex3d> vertexes = polyline.GetVertexes();
            List<LineSegment3d> segments = new List<LineSegment3d>();

            if (vertexes.Count < 4)
            {
                return segments;
            }

            for (int i = 0; i < vertexes.Count; i++)
            {
                if (i == 0)
                {
                    continue;
                }

                Point3d startPoint = vertexes[i - 1].Position;
                Point3d endPoint = vertexes[i].Position;

                if(startPoint.IsEqualByXYZ(endPoint, tolerance))
                {
                    continue;
                }

                LineSegment3d segment = new LineSegment3d(startPoint, endPoint);
                segments.Add(segment);
            }

            if (polyline.Closed)
            {
                Point3d startPoint = vertexes.First().Position;
                Point3d endPoint = vertexes.Last().Position;

                if (!startPoint.IsEqualByXYZ(endPoint, tolerance))
                {
                    LineSegment3d segment = new LineSegment3d(startPoint, endPoint);
                    segments.Add(segment);
                }
            }

            return segments;
        }

        public static List<LineSegment3d> GetSegmentsWithElevation(this Polyline3d polyline, int tolerance, double elevation)
        {
            List<PolylineVertex3d> vertexes = polyline.GetVertexes();
            List<LineSegment3d> segments = new List<LineSegment3d>();

            if (vertexes.Count < 4)
            {
                return segments;
            }

            for (int i = 0; i < vertexes.Count; i++)
            {
                if (i == 0)
                {
                    continue;
                }

                Point3d startPoint = vertexes[i - 1].Position;
                Point3d endPoint = vertexes[i].Position;

                Point3d flattenStartPoint = new Point3d(startPoint.X, startPoint.Y, elevation);
                Point3d flattenEndPoint = new Point3d(endPoint.X, endPoint.Y, elevation);

                if (flattenStartPoint.IsEqualByXYZ(flattenEndPoint, tolerance))
                {
                    continue;
                }

                LineSegment3d segment = new LineSegment3d(flattenStartPoint, flattenEndPoint);
                segments.Add(segment);
            }

            if (polyline.Closed)
            {
                Point3d startPoint = vertexes.First().Position;
                Point3d endPoint = vertexes.Last().Position;

                Point3d flattenStartPoint = new Point3d(startPoint.X, startPoint.Y, elevation);
                Point3d flattenEndPoint = new Point3d(endPoint.X, endPoint.Y, elevation);

                if (!flattenStartPoint.IsEqualByXYZ(flattenEndPoint, tolerance))
                {
                    LineSegment3d segment = new LineSegment3d(flattenStartPoint, flattenEndPoint);
                    segments.Add(segment);
                }
            }

            return segments;
        }

        public static List<Point3d> GetSelfIntersectionPoints(this Polyline3d polyline, int tolerance)
        {
            List<LineSegment3d> lines = polyline.GetSegments(tolerance);
            return LineSegment3dUtils.GetSelfIntersectionPoints(lines, tolerance);
        }

        public static List<Point3d> GetSelfIntersectionPointsBy2d(this Polyline3d polyline, int tolerance, double elevation)
        {
            List<LineSegment3d> lines = polyline.GetSegmentsWithElevation(tolerance, elevation);

            return LineSegment3dUtils.GetSelfIntersectionPoints(lines, tolerance);
        }

        public static List<Point3d> GetSelfIntersectionPoints(this Polyline polyline, int tolerance)
        {
            List<CircularArc3d> arcs = new List<CircularArc3d>();
            List<LineSegment3d> lines = new List<LineSegment3d>();
            polyline.GetSegments(ref lines, ref arcs);

            return LineSegment3dUtils.GetSelfIntersectionPoints(lines, arcs, tolerance);
        }
    }
}
