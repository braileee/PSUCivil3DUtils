using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DDataExport.Models
{
    public class StationData
    {

        public double Step { get; set; }

        public double HorizontalOffset { get; set; }
        public double VerticalOffset { get; set; }

        public double StartStation { get; set; }
        public double EndStation { get; set; }

        public List<StationItem> StationItems { get; set; } = new List<StationItem>();
        public string ProfileName { get; internal set; }
        public string AlignmentName { get; internal set; }
        public string FilePath { get; internal set; }
    }
}
