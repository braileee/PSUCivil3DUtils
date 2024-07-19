using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DWeedLines.Models
{
    public class LineSegmentGroup
    {
        public LineSegmentWrapper LineSegmentWrapper1 { get; set; }
        public LineSegmentWrapper LineSegmentWrapper2 { get; set; }

    }
}
