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
        public uint Id { get; set; }
        public string Description
        {
            get
            {
                return MainCogoPoint?.RawDescription;
            }
        }

        public string ElementName
        {
            get
            {
                if (string.IsNullOrEmpty(Description))
                {
                    return string.Empty;
                }

                string[] parts = Description.Split('-');

                if (parts.Length > 0)
                {
                    return parts[0];
                }

                return string.Empty;
            }
        }

        public string SegmentName
        {
            get
            {
                if (string.IsNullOrEmpty(Description))
                {
                    return string.Empty;
                }

                string[] parts = Description.Split('-');

                if (parts.Length > 1)
                {
                    return parts[1];
                }

                return string.Empty;
            }
        }

        public string DivisionName
        {
            get
            {
                if (string.IsNullOrEmpty(Description))
                {
                    return string.Empty;
                }

                string[] parts = Description.Split('-');

                if (parts.Length > 2)
                {
                    return parts[2];
                }

                return string.Empty;
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
