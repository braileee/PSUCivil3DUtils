using Autodesk.Civil.DatabaseServices;
using Civil3DExtractCorridorLinksToSurfaces.Events;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DExtractCorridorLinksToSurfaces.Models
{
    public class CorridorSurfaceWrapper : BindableBase
    {
        private bool isSelected;

        public CorridorSurfaceWrapper(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
        }

        public Corridor Corridor { get; set; }
        public string Name { get; set; }
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;

                if (Name == "<Select All>")
                {
                    EventAggregator.GetEvent<SelectAllSurfacesEvent>().Publish(IsSelected);
                }

                RaisePropertyChanged();
            }
        }
        public IEventAggregator EventAggregator { get; }
        public CorridorSurface Surface { get; internal set; }
    }
}
