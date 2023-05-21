using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autofac;
using Civil3DSubassemblyAnnotations.Startup;
using Civil3DSubassemblyAnnotations.Views;

namespace Civil3DSubassemblyAnnotations
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DSubassemblyAnnotations", CommandFlags.Modal)]
        public static void Start()
        {
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            var mainView = container.Resolve<MainView>();
            Application.ShowModelessWindow(mainView);
        }
    }
}
