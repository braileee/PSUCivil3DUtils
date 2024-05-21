using AutoCADUtils.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autofac;
using Civil3DBatchExtractSolids.Startup;
using Civil3DBatchExtractSolids.Views;

namespace Civil3DBatchExtractSolids
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DBatchExtractSolids", CommandFlags.Modal)]
        public static void Start()
        {
#if CIVIL3D2024DEBUG
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            var mainView = container.Resolve<MainView>();
            Application.ShowModelessWindow(mainView); 
#else
            MessageBoxUtils.ShowInfo("The app is supported in Civil 3D 2023 and higher");
            return;
#endif
        }
    }
}
