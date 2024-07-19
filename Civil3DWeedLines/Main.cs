using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autofac;
using Civil3DWeedLines.Startup;
using Civil3DWeedLines.Views;

namespace Civil3DWeedLines
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DWeedLines", CommandFlags.Modal)]
        public static void Start()
        {
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            var mainView = container.Resolve<MainView>();
            Application.ShowModelessWindow(mainView);
        }
    }
}
