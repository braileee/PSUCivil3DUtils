using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autofac;
using Civil3DCutSolid.Startup;
using Civil3DCutSolid.Views;

namespace Civil3DCutSolid
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DCutSolid", CommandFlags.Modal)]
        public static void Start()
        {
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            var mainView = container.Resolve<MainView>();
            Application.ShowModelessWindow(mainView);
        }
    }
}
