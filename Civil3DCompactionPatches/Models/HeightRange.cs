using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DCompactionPatches.Models
{
    public class HeightRange
    {
        public double From { get; set; }
        public double To { get; set; }
        public short ColorIndex { get; set; } // AutoCAD ACI color (1–255)

        public override string ToString()
        {
            return $"{From:F3} → {To:F3} (Color {ColorIndex})";
        }

    }
}
