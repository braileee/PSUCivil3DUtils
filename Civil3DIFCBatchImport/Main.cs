using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autofac;
using Civil3DIFCBatchImport.Startup;
using Civil3DIFCBatchImport.Views;

namespace Civil3DIFCBatchImport
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DIFCBatchImport", CommandFlags.Modal)]
        public static void Start()
        {
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            var mainView = container.Resolve<MainView>();
            Application.ShowModelessWindow(mainView);
        }
    }
}
