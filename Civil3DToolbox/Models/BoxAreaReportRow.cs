using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DToolbox.Models
{
    public class BoxAreaReportRow
    {
        public int Id { get; set; }

        public double StationCenter { get; set; }
        public double StationMax { get; set; }
        public double StationMin { get; set; }
        public string Description { get; set; }
        public double Length    { get; set; }
        public int Count { get; set; }

        public double ElevationDifferenceAverage { get; set; }
        public double ElevationDifferenceMin { get; set; }
        public double ElevationDifferenceMax { get; set; }

        public double BoxCenterX { get; set; }
        public double BoxCenterY { get; set; }
        public double ElevationDifferenceSum { get; internal set; }
    }
}
