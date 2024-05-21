using Autofac;
using Civil3DBatchExtractSolids.ViewModels;
using Civil3DBatchExtractSolids.Views;
using Prism.Events;

namespace Civil3DBatchExtractSolids.Startup
{
    public class Bootstrapper
    {
        public IContainer Bootstrap()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<MainViewViewModel>().AsSelf();
            builder.RegisterType<MainView>().AsSelf();

            return builder.Build();
        }
    }
}
