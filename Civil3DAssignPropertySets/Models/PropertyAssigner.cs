using Autodesk.Aec.PropertyData.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DAssignPropertySets.Models
{
    public class PropertyAssigner
    {
        public PropertySetDefinition PropertySetDefinition { get; set; }
        public PropertyDefinition PropertyDefinition { get; set; }
        public PropertySetItem PropertySetItem { get; set; }
    }
}
