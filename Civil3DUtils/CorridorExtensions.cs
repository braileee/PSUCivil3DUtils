using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Civil = Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DUtils
{
    public static  class CorridorExtensions
    {
        public static List<Civil.Baseline> GetBaselines(this Civil.Corridor corridor)
        {
            return corridor.Baselines.ToList();
        }
    }
}
