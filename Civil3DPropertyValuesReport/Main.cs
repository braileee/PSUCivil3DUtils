using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autofac;
using Civil3DPropertyValuesReport.Startup;
using Civil3DPropertyValuesReport.Views;

namespace Civil3DPropertyValuesReport
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DPropertyValuesReport", CommandFlags.Modal)]
        public static void Start()
        {
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            var mainView = container.Resolve<MainView>();
            Application.ShowModelessWindow(mainView);
        }
    }
}
