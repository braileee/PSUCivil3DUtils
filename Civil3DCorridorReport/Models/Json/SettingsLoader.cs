using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Civil3DCorridorReport.Models.Json
{
    public static class SettingsLoader
    {
        public static Settings Load()
        {
            // Assembly containing Main class
            Assembly assembly = typeof(Main).Assembly;

            // Folder: ...\Contents\
            string assemblyDir = Path.GetDirectoryName(assembly.Location);

            // Build path to Settings.json
            string settingsPath = Path.Combine(
                assemblyDir,
                "Files",
                "Civil3DCorridorReport",
                "Settings.json"
            );

            if (!File.Exists(settingsPath))
            {
                return null;
            }

            string json = File.ReadAllText(settingsPath);

            return JsonSerializer.Deserialize<Settings>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}
