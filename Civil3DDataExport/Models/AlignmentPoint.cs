using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DDataExport.Models
{
    public class AlignmentPoint
    {
        public Point3d Point { get; set; }
        public double Station { get; private set; }

        public static List<AlignmentPoint> Create(Alignment alignment, Profile profile, double startStation, double endStation, double step, double planOffset, double verticalOffset, Log log)
        {
            var alignmentPoints = new List<AlignmentPoint>();

            if (alignment == null || profile == null)
            {
                log.Error("Alignment or Profile is null. Cannot create alignment points.");
                return alignmentPoints;
            }

            if (startStation >= endStation)
            {
                log.Error("Start station must be less than end station.");
                return new List<AlignmentPoint>();
            }

            double northing = 0;
            double easting = 0;

            double station = startStation;

            while (station <= endStation)
            {
                log.Information($"Find easting and northing on station {station}, plan offset {planOffset}");
                alignment.PointLocation(station, planOffset, ref easting, ref northing);

                log.Information($"Easting: {easting}, Northing: {northing}");

                log.Information($"Find elevation on station {station}, vertical offset {verticalOffset}");
                double elevation = profile.ElevationAt(station);
                log.Information($"Elevation: {elevation}");

                Point3d point = new Point3d(easting, northing, elevation + verticalOffset);

                log.Information($"Calculated point: {point}");

                alignmentPoints.Add(new AlignmentPoint
                {
                    Point = point,
                    Station = station
                });

                station += step;
            }

            return alignmentPoints;
        }

        public string Output
        {
            get
            {
                return $"{Point.X},{Point.Y},{Point.Z},{Station}";
            }
        }
    }
}
