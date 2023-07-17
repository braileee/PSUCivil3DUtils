using Autodesk.AutoCAD.Runtime;
using Autofac;
using Civil3DAssignPropertySets.Startup;
using Civil3DAssignPropertySets.Views;
using Autodesk.AutoCAD.ApplicationServices;

namespace Civil3DAssignPropertySets
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DAssignPropertySets", CommandFlags.Modal)]
        public static void Start()
        {
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            var mainView = container.Resolve<MainView>();
            Application.ShowModelessWindow(mainView);
        }
    }
}
