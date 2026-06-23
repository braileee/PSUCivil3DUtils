using Autodesk.AutoCAD.Colors;
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

        public Color Color { get; set; } 

        public override string ToString()
        {
            return $"{From:F3} → {To:F3}";
        }
    }
}
