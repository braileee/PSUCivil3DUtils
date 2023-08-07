using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autofac;
using Civil3DExtractCorridorLinksToSurfaces.Startup;
using Civil3DExtractCorridorLinksToSurfaces.Views;

namespace Civil3DExtractCorridorLinksToSurfaces
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DExtractCorridorLinksToSurfaces", CommandFlags.Modal)]
        public static void Start()
        {
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            var mainView = container.Resolve<MainView>();
            Application.ShowModelessWindow(mainView);
        }
    }
}
