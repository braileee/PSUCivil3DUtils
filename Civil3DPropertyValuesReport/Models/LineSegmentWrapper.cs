using AutoCADUtils;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using Civil3DUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acad = Autodesk.AutoCAD.DatabaseServices;

namespace Civil3DPropertyValuesReport.Models
{
    public class LineSegmentWrapper : ElementWrapper
    {
        private string segmentType;
        private double length;

        public LineSegmentWrapper(Acad.DBObject dBObject, int selectedRoundingValue, double segmentLength, Point3d startPoint, Point3d endPoint, string segmentType) : base(dBObject, selectedRoundingValue)
        {
            SegmentType = segmentType;

            StartPoint = startPoint;
            EndPoint = endPoint;
            Length = Math.Round(segmentLength, selectedRoundingValue);
        }

        public double Length
        {
            get
            {
                return length;
            }

            set
            {
                length = value;
                RaisePropertyChanged();
            }
        }

        public string SegmentType
        {
            get
            {
                return segmentType;
            }
            set
            {
                segmentType = value;
                RaisePropertyChanged();
            }
        }

        public Point3d StartPoint { get; }
        public Point3d EndPoint { get; }

        public static List<LineSegmentWrapper> Get(Acad.DBObject dBObject, int selectedRoundingValue)
        {
            List<LineSegmentWrapper> lineSegmentWrappers = new List<LineSegmentWrapper>();

            List<LineSegment3d> lineSegments = new List<LineSegment3d>();
            List<CircularArc3d> arcSegments = new List<CircularArc3d>();

            if (dBObject is Polyline3d polyline3d)
            {
                lineSegments = polyline3d.GetSegments();
            }
            else if (dBObject is Polyline polyline)
            {
                polyline.GetSegments(ref lineSegments, ref arcSegments);
            }
            else if (dBObject is FeatureLine featureLine)
            {
                lineSegments = featureLine.GetSegments(tolerance: selectedRoundingValue);
            }

            foreach (LineSegment3d lineSegment in lineSegments)
            {
                lineSegmentWrappers.Add(new LineSegmentWrapper(dBObject, selectedRoundingValue, lineSegment.Length, lineSegment.StartPoint, lineSegment.EndPoint, "Line"));
            }

            foreach (CircularArc3d arc in arcSegments)
            {
                double length = arc.GetLength(arc.GetParameterOf(arc.StartPoint), arc.GetParameterOf(arc.EndPoint), selectedRoundingValue);
                lineSegmentWrappers.Add(new LineSegmentWrapper(dBObject, selectedRoundingValue, length, arc.StartPoint, arc.EndPoint, "Arc"));
            }

            return lineSegmentWrappers;
        }
    }
}
