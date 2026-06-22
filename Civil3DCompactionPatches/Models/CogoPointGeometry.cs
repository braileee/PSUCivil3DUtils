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
    public class CogoPointGeometry
    {
        public CogoPoint CogoPoint { get; set; }
        public Extents3d PolylineExtent { get; set; }
        public Point2d[] PolylinePoints { get; internal set; }
        public Polyline Polyline { get; internal set; }
    }
}
