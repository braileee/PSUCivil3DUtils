using AutoCADUtils;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Civil3DCompactionPatches.Models
{
    public static class HeightRangeGenerator
    {

        private static readonly (byte r, byte g, byte b)[] GradientAnchors =
        {
 (0,   0, 255),   // Blue
    (0, 255, 255),   // Cyan
    (0, 255,   0),   // Green
    (255, 255, 0),   // Yellow
    (255, 150, 0),   // Orange
    (255,   0,   0), // Red
    (255,   0, 200), // Magenta
    (160,   0, 255)  // Purple
};

        private static readonly Color[] DistinctPalette =
      {
    Color.FromRgb(0, 0, 255),
    Color.FromRgb(0, 120, 255),
    Color.FromRgb(0, 200, 255),
    Color.FromRgb(0, 200, 120),
    Color.FromRgb(0, 180, 0),
    Color.FromRgb(120, 220, 0),
    Color.FromRgb(200, 255, 0),
    Color.FromRgb(255, 255, 0),
    Color.FromRgb(255, 200, 0),
    Color.FromRgb(255, 150, 0),
    Color.FromRgb(255, 80, 0),
    Color.FromRgb(255, 0, 0),
    Color.FromRgb(255, 0, 120),
    Color.FromRgb(255, 0, 200),
    Color.FromRgb(200, 0, 255),
    Color.FromRgb(160, 0, 255)
};


        public static List<HeightRange> Generate(
     double min,
     double max,
     double step = 0.1)
        {
            var ranges = new List<HeightRange>();

            double roundedMin = Math.Floor(min / step) * step;
            double roundedMax = Math.Ceiling(max / step) * step;

            int stepsCount = (int)Math.Ceiling((roundedMax - roundedMin) / step);

            // UNDERFLOW
            ranges.Add(new HeightRange
            {
                From = double.MinValue,
                To = roundedMin,
                Color = Color.FromRgb(0, 0, 150) // dark blue
            });

            int index = 0;

            for (double start = roundedMin; start < roundedMax; start += step)
            {
                double end = Math.Round(start + step, 10);

                Color color = GetGradientColor(index, stepsCount);

                ranges.Add(new HeightRange
                {
                    From = Math.Round(start, 10),
                    To = end,
                    Color = color
                });

                index++;
            }

            // OVERFLOW
            ranges.Add(new HeightRange
            {
                From = roundedMax,
                To = double.MaxValue,
                Color = Color.FromRgb(120, 0, 120) // purple
            });

            return ranges;
        }

        public static HeightRange FindRange(double value, List<HeightRange> ranges)
        {
            foreach (var r in ranges)
            {
                if (value >= r.From && value < r.To)
                    return r;
            }
            return null;
        }


        public static void DrawLegend(
    List<HeightRange> ranges,
    Point3d basePoint,
    double squareSize = 2.0,
    double rowSpacing = 3.0,
    double textOffsetX = 3.0)
        {
            var doc = AutocadDocumentService.ActiveDocument;
            var db = doc.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                var groupDict = (DBDictionary)tr.GetObject(db.GroupDictionaryId, OpenMode.ForWrite);

                var allIds = new ObjectIdCollection();

                double y = basePoint.Y;

                foreach (var range in ranges)
                {
                    // ----------------------------------
                    // ✅ Polyline square
                    // ----------------------------------
                    var pl = new Polyline(4);
                    pl.AddVertexAt(0, new Point2d(basePoint.X, y), 0, 0, 0);
                    pl.AddVertexAt(1, new Point2d(basePoint.X + squareSize, y), 0, 0, 0);
                    pl.AddVertexAt(2, new Point2d(basePoint.X + squareSize, y + squareSize), 0, 0, 0);
                    pl.AddVertexAt(3, new Point2d(basePoint.X, y + squareSize), 0, 0, 0);
                    pl.Closed = true;

                    btr.AppendEntity(pl);
                    tr.AddNewlyCreatedDBObject(pl, true);

                    // ----------------------------------
                    // ✅ Hatch
                    // ----------------------------------
                    var hatch = new Hatch();
                    hatch.SetDatabaseDefaults();
                    hatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
                    hatch.Color = range.Color;

                    btr.AppendEntity(hatch);
                    tr.AddNewlyCreatedDBObject(hatch, true);

                    hatch.Associative = true;
                    hatch.AppendLoop(HatchLoopTypes.Default, new ObjectIdCollection { pl.ObjectId });
                    hatch.EvaluateHatch(true);

                    // ----------------------------------
                    // ✅ Text
                    // ----------------------------------
                    var pos = new Point3d(basePoint.X + textOffsetX, y + squareSize / 2, 0);

                    var text = new DBText
                    {
                        Position = pos,
                        Height = 1.5,
                        TextString = FormatRange(range),
                        VerticalMode = TextVerticalMode.TextVerticalMid,
                        HorizontalMode = TextHorizontalMode.TextLeft,
                        AlignmentPoint = pos
                    };

                    btr.AppendEntity(text);
                    tr.AddNewlyCreatedDBObject(text, true);

                    // ✅ Collect all entities
                    allIds.Add(pl.ObjectId);
                    allIds.Add(hatch.ObjectId);
                    allIds.Add(text.ObjectId);

                    y -= rowSpacing;
                }

                // ----------------------------------
                // ✅ Single group for entire legend
                // ----------------------------------
                if (allIds.Count > 0)
                {
                    string groupName = $"HR_LEGEND_{Guid.NewGuid()}";

                    var group = new Group("Height Range Legend", true);
                    groupDict.SetAt(groupName, group);
                    tr.AddNewlyCreatedDBObject(group, true);

                    foreach (ObjectId id in allIds)
                    {
                        group.Append(id);
                    }
                }

                tr.Commit();
            }
        }

        private static double ColorDistance((byte r, byte g, byte b) c1, (byte r, byte g, byte b) c2)
        {
            int dr = c1.r - c2.r;
            int dg = c1.g - c2.g;
            int db = c1.b - c2.b;

            return Math.Sqrt(dr * dr + dg * dg + db * db);
        }

        private static Color GetGradientColor(int index, int total)
        {
            // ✅ safety fallback
            if (GradientAnchors == null || GradientAnchors.Length < 2)
                return Color.FromRgb(255, 0, 0);

            if (total <= 1)
                return Color.FromRgb(
                    GradientAnchors[0].r,
                    GradientAnchors[0].g,
                    GradientAnchors[0].b);

            int segmentCount = GradientAnchors.Length - 1;

            // normalize
            double t = (double)index / (total - 1);

            // clamp properly
            t = Math.Max(0.0, Math.Min(1.0, t));

            double scaled = t * segmentCount;

            int segIndex = (int)Math.Floor(scaled);

            // ✅ critical fix: clamp index safely
            if (segIndex >= segmentCount)
                segIndex = segmentCount - 1;

            double localT = scaled - segIndex;

            var c1 = GradientAnchors[segIndex];
            var c2 = GradientAnchors[segIndex + 1];

            byte r = (byte)(c1.r + (c2.r - c1.r) * localT);
            byte g = (byte)(c1.g + (c2.g - c1.g) * localT);
            byte b = (byte)(c1.b + (c2.b - c1.b) * localT);

            return Color.FromRgb(r, g, b);
        }

        private static Color HsvToRgb(double h, double s, double v)
        {
            double c = v * s;
            double x = c * (1 - Math.Abs((h / 60.0 % 2) - 1));
            double m = v - c;

            double r = 0, g = 0, b = 0;

            if (h < 60) { r = c; g = x; b = 0; }
            else if (h < 120) { r = x; g = c; b = 0; }
            else if (h < 180) { r = 0; g = c; b = x; }
            else if (h < 240) { r = 0; g = x; b = c; }
            else if (h < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            byte R = (byte)((r + m) * 255);
            byte G = (byte)((g + m) * 255);
            byte B = (byte)((b + m) * 255);

            return Color.FromRgb(R, G, B);
        }


        private static string FormatRange(HeightRange range)
        {
            if (range.From == double.MinValue)
                return $"< {range.To:F3}";

            if (range.To == double.MaxValue)
                return $"> {range.From:F3}";

            return $"{range.From:F3} - {range.To:F3}";
        }
    }
}
