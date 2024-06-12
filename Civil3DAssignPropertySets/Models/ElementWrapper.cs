using Autodesk.Aec.PropertyData.DatabaseServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DAssignPropertySets.Models
{
    public class ElementWrapper
    {
        public DBObject DbObject { get; set; }
        public PropertySetDefinition PropertySetDefinition { get; set; }
        public PropertyDefinition PropertyDefinition { get; set; }

        public object Value { get; set; }

        public Autodesk.Aec.PropertyData.DataType DataType
        {
            get
            {
                return PropertyDefinition.DataType;
            }
        }
    }
}
