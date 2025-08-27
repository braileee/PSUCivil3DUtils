using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Civil3DDataExport.Models;
using Civil3DDataExport.Utils;
using Civil3DDataExport.Views;
using System;
using System.IO;
using System.Windows.Controls;

namespace Civil3DDataExport
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DDataExport", CommandFlags.Modal)]
        public void Start()
        {
            string logDirectory = Path.Combine(AssemblyUtils.GetFolder(typeof(Main)), Constants.LogsFolder);

            string dwgFileName = Path.GetFileNameWithoutExtension(App.ActiveDocumentAutocad.Name);
            Log log = new Log(logDirectory, $"{dwgFileName}_TBMApp");

            log.Information("Civil 3D Data Export App Started");
            log.Information($"User: {Environment.UserName}");

            MainView mainView = new MainView(log);
            Application.ShowModelessWindow(mainView);
        }
    }
}
