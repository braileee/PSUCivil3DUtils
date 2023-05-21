using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DSubassemblyAnnotations.Models
{
    public class LinkGroup
    {
        public CalculatedLink Link { get; set; }
        public AppliedAssembly AppliedAssembly { get; set; }
        public CalculatedPoint Point1 { get; internal set; }
        public CalculatedPoint Point2 { get; internal set; }
        public double Station { get; internal set; }
    }
}
