using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADUtils
{
    public static class DoubleExtensions
    {
        public static double RadiansToDegrees(this double radians)
        {
            return radians * (180 / Math.PI);
        }

        public static double DegreesToRadians(this double degress)
        {
            return degress * (Math.PI / 180);
        }
    }
}
