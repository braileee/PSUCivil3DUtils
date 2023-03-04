using Autofac;
using Civil3DAlignmentStationCoordinatesTable.ViewModels;
using Civil3DAlignmentStationCoordinatesTable.Views;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DAlignmentStationCoordinatesTable.Startup
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
