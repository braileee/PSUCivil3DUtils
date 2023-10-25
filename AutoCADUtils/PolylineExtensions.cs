using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
