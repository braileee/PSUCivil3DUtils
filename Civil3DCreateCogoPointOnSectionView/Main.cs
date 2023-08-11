using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autofac;
using Civil3DCreateCogoPointOnSectionView.Startup;
using Civil3DCreateCogoPointOnSectionView.Views;

namespace Civil3DCreateCogoPointOnSectionView
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DCreateCogoPointOnSectionView", CommandFlags.Modal)]
        public static void Start()
        {
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            var mainView = container.Resolve<MainView>();
            Application.ShowModelessWindow(mainView);
        }
    }
}
