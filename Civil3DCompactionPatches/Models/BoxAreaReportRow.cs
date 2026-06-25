using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DCompactionPatches.Models
{
    public class BoxAreaReportRow
    {
        public uint Id { get; set; }

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
        public string ElementName { get; internal set; }
        public string DivisionName { get; internal set; }
        public string SegmentName { get; internal set; }
        public BoxArea Model { get; internal set; }
        public string Position { get; internal set; }
        public string RowPosition { get; internal set; }
    }
}
