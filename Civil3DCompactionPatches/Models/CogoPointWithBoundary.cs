using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DCompactionPatches.Models
{
    public class CogoPointWithBoundary
    {
        public CogoPoint CogoPoint { get; set; }
        public Polyline Polyline { get; set; }
    }
}
