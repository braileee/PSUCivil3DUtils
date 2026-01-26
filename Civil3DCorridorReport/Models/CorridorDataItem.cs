using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DCorridorReport.Models
{
    public class CorridorDataItem
    {
        public double StartStation { get; set; }
        public string StartStationFormatted
        {
            get
            {
                return GetFormattedStation(StartStation);
            }
        }

        private string GetFormattedStation(double station)
        {
            int km = (int)(station / 1000);
            double remainder = station % 1000;

            return $"{km}+{remainder:000.00}";
        }

        public double EndStation { get; set; }
        public string EndStationFormatted
        {
            get
            {
                return GetFormattedStation(EndStation);
            }
        }
        public string AssemblyName { get; set; }

        public double TopElevation { get; set; }

        public double? SlopeFrontPercent { get; set; }

        public string SlopeFrontFormatted
        {
            get
            {
                return SlopeFrontPercent.HasValue ? FormatSlopeAsRatio(SlopeFrontPercent.Value) : string.Empty;
            }
        }

        public double? SlopeBackPercent { get; set; }

        public string SlopeBackFormatted
        {
            get
            {
                return SlopeBackPercent.HasValue ? FormatSlopeAsRatio(SlopeBackPercent.Value) : string.Empty;
            }
        }

        private static string FormatSlopeAsRatio(double slopePercent)
        {
            // percent slope means rise/run = percent/100
            double run = Math.Abs(100.0 / slopePercent);   // convert to 1 : X form

            return $"1 : {run:F2}";
        }

        public double Width { get; set; }
    }
}
