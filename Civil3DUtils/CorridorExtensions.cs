using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DUtils
{
    public static  class CorridorExtensions
    {
        public static List<Baseline> GetBaselines(this Corridor corridor)
        {
            return corridor.Baselines.ToList();
        }
    }
}
