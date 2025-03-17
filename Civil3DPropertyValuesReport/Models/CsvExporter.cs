using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DPropertyValuesReport.Models
{
    public class CsvExporter
    {
        public static void ExportToCsv<T>(IEnumerable<T> elements, string filePath)
        {
            if (elements == null || !elements.Any())
            {
                return;
            }

            // Create a StringBuilder to hold the CSV data
            var csvBuilder = new StringBuilder();

            // Get properties of the type T
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Write the headers
            var headerLine = string.Join(";", properties.Select(p => EscapeCsvValue(p.Name)));
            csvBuilder.AppendLine(headerLine);

            // Write each element's property values
            foreach (var element in elements)
            {
                var line = string.Join(";", properties.Select(p => EscapeCsvValue(p.GetValue(element)?.ToString())));
                csvBuilder.AppendLine(line);
            }

            // Write the CSV data to a file
            File.WriteAllText(filePath, csvBuilder.ToString());
        }

        private static string EscapeCsvValue(string value)
        {
            if (value == null) return ""; // Handle null values
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                // Escape the value for CSV
                return "\"" + value.Replace("\"", "\"\"") + "\""; // Escape quotes
            }
            return value;
        }
    }
}
