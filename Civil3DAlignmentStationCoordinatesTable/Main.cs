using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autofac;
using Civil3DAlignmentStationCoordinatesTable.Startup;
using Civil3DAlignmentStationCoordinatesTable.Views;

namespace Civil3DAlignmentStationCoordinatesTable
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DAlignmentStationCoordinatesTable", CommandFlags.Modal)]
        public static void Start()
        {
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            var mainView = container.Resolve<MainView>();
            Application.ShowModelessWindow(mainView);
        }
    }
}
