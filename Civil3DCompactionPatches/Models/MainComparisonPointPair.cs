using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DCompactionPatches.Models
{
    public class MainComparisonPointPair
    {
        public Point3d MainPoint { get; set; }
        public Point3d ComparisonPoint { get; set; }

        public double ElevationDifference
        {
            get
            {
                return MainPoint.Z - ComparisonPoint.Z;
            }
        }
    }
}
