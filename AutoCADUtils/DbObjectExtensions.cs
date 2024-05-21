using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADUtils
{
    public static class DbObjectExtensions
    {
        public static bool IsBad(this DBObject dBObject)
        {
            return dBObject == null || dBObject.IsErased;
        }

        public static bool IsValid(this DBObject dBObject)
        {
            return !IsBad(dBObject);
        }
    }
}
