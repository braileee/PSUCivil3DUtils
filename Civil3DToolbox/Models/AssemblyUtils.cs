using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DToolbox.Models
{
    public static class AssemblyUtils
    {
        public static string GetFolder(Type type)
        {
            return Path.GetDirectoryName(GetFilePath(type));
        }

        public static string GetFilePath(Type type)
        {
            return Assembly.GetAssembly(type).Location;
        }
    }
}
