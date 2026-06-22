using System;
using System.Collections.Generic;


namespace Civil3DCompactionPatches.Models
{
    public static class HeightRangeGenerator
    {
        public static List<HeightRange> Generate(
            double min,
            double max,
            double step = 0.1)
        {
            var ranges = new List<HeightRange>();

            // Round bounds to step grid
            double roundedMin = Math.Floor(min / step) * step;
            double roundedMax = Math.Ceiling(max / step) * step;

            // AutoCAD ACI palette sample (distinct colors)
            short[] aciColors = new short[]
            {
                1, 3, 5, 7, 9, 2, 4, 6, 140, 30, 200, 50, 120
            };

            int colorIndex = 0;

            // UNDERFLOW (less than lowest rounded step)
            ranges.Add(new HeightRange
            {
                From = double.MinValue,
                To = roundedMin,
                ColorIndex = 250 // grey-ish fallback
            });

            // MAIN RANGES
            for (double start = roundedMin; start < roundedMax; start += step)
            {
                double end = Math.Round(start + step, 10);

                ranges.Add(new HeightRange
                {
                    From = Math.Round(start, 10),
                    To = end,
                    ColorIndex = aciColors[colorIndex % aciColors.Length]
                });

                colorIndex++;
            }

            // OVERFLOW (greater than highest rounded step)
            ranges.Add(new HeightRange
            {
                From = roundedMax,
                To = double.MaxValue,
                ColorIndex = 10 // another distinct color
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
    }
}
