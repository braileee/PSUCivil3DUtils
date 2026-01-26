using AutoCADUtils.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autofac;
using Civil3DCorridorReport.Models.Json;
using Civil3DCorridorReport.Startup;
using Civil3DCorridorReport.Views;

namespace Civil3DCorridorReport
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DCorridorReport", CommandFlags.Modal)]
        public static void Start()
        {
            try
            {
                Settings settings = SettingsLoader.Load();

                if (settings == null)
                {
                    MessageBoxUtils.ShowError("Can't load settings file");
                    return;
                }

                var bootstrapper = new Bootstrapper();
                var container = bootstrapper.Bootstrap();
                var mainView = container.Resolve<MainView>();
                Application.ShowModelessWindow(mainView);

            }
            catch (System.Exception)
            {
                MessageBoxUtils.ShowError("Unexpected error");
            }
        }
    }
}
