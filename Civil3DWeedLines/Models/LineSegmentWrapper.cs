using AutoCADUtils;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DWeedLines.Models
{
    public class LineSegmentWrapper
    {
        public LineSegment3d LineSegment { get; set; }
        public PolylineVertex3d StartVertex { get; set; }
        public PolylineVertex3d EndVertex { get; set; }

        public static List<LineSegmentWrapper> Create(Polyline3d polyline)
        {
            List<PolylineVertex3d> vertexes = polyline.GetVertexes();
            List<LineSegmentWrapper> segments = new List<LineSegmentWrapper>();

            if (vertexes.Count < 4)
            {
                return new List<LineSegmentWrapper>();
            }

            for (int i = 0; i < vertexes.Count; i++)
            {
                if (i == 0)
                {
                    continue;
                }

                PolylineVertex3d startVertex = vertexes[i - 1];
                Point3d startPoint = startVertex.Position;
                PolylineVertex3d endVertex = vertexes[i];
                Point3d endPoint = endVertex.Position;

                if (startPoint.IsEqualTo(endPoint))
                {
                    continue;
                }

                LineSegment3d segment = new LineSegment3d(startPoint, endPoint);
                LineSegmentWrapper lineSegmentWrapper = new LineSegmentWrapper
                {
                    LineSegment = segment,
                    StartVertex = startVertex,
                    EndVertex = endVertex
                };
                segments.Add(lineSegmentWrapper);
            }

            if (polyline.Closed)
            {
                PolylineVertex3d startVertex = vertexes.First();
                Point3d startPoint = startVertex.Position;
                PolylineVertex3d endVertex = vertexes.Last();
                Point3d endPoint = endVertex.Position;

                if (!startPoint.IsEqualTo(endPoint))
                {
                    LineSegment3d segment = new LineSegment3d(startPoint, endPoint);

                    LineSegmentWrapper lineSegmentWrapper = new LineSegmentWrapper
                    {
                        LineSegment = segment,
                        StartVertex = startVertex,
                        EndVertex = endVertex
                    };

                    segments.Add(lineSegmentWrapper);
                }
            }

            return segments;
        }

        
        public PolylineVertex3d GetCommonVertex(LineSegmentWrapper otherSegmentWrapper)
        {
            if (StartVertex.Position.IsEqualTo(otherSegmentWrapper.StartVertex.Position))
            {
                return StartVertex;
            }
            else if (StartVertex.Position.IsEqualTo(otherSegmentWrapper.EndVertex.Position))
            {
                return StartVertex;
            }
            else if (EndVertex.Position.IsEqualTo(otherSegmentWrapper.EndVertex.Position))
            {
                return EndVertex;
            }
            else if (EndVertex.Position.IsEqualTo(otherSegmentWrapper.StartVertex.Position))
            {
                return EndVertex;
            }

            return null;
        }
    }
}
