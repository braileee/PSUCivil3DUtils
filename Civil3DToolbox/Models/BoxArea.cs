using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DToolbox.Models
{
    public class BoxArea
    {
        public int Id { get; set; }
        public string Description
        {
            get
            {
                return MainCogoPoint?.RawDescription;
            }
        }

        public CogoPoint MainCogoPoint { get; set; }

        public List<MainComparisonPointPair> MainComparisonPointPairs { get; set; } = new List<MainComparisonPointPair>();

        public double StationBoxCenter { get; set; }
        public double StationBoxMin { get; set; }
        public double StationBoxMax { get; set; }

        public double GetAverage()
        {
            double total = MainComparisonPointPairs.Sum(item => item.ElevationDifference);
            return total / MainComparisonPointPairs.Count;
        }
    }
}
