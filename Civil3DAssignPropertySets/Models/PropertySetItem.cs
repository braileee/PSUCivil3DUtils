using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DAssignPropertySets.Models
{
    public class PropertySetItem
    {
        public string Group { get; set; }
        public string PropertySet { get; set; }
        public string Property { get; set; }
        public string Value { get; set; }

        public static List<PropertySetItem> Convert(DataSet dataSet)
        {
            if (dataSet.Tables.Count == 0)
            {
                return null;
            }

            DataTable dataTable = dataSet.Tables[0];
            DataRowCollection dataRowCollection = dataTable.Rows;

            List<PropertySetItem> propertySetItems = new List<PropertySetItem>();

            for (int i = 0; i < dataRowCollection.Count; i++)
            {
                DataRow dataRow = dataRowCollection[i];

                if (i == 0)
                {
                    // Skip headers
                    continue;
                }

                // Not enough values in the row
                if(dataRow.ItemArray.Length < 4)
                {
                    continue;
                }

                propertySetItems.Add(new PropertySetItem
                {
                    Group = dataRow.ItemArray[0].ToString(),
                    PropertySet = dataRow.ItemArray[1].ToString(),
                    Property = dataRow.ItemArray[2].ToString(),
                    Value = dataRow.ItemArray[3].ToString(),
                });
            }

            return propertySetItems;
        }
    }
}
