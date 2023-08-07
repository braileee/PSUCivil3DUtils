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
    public class LinkCodeWrapper : BindableBase
    {
        private bool isSelected;

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
                    EventAggregator.GetEvent<SelectAllLinksEvent>().Publish(IsSelected);
                }

                RaisePropertyChanged();
            }
        }

        public string Name { get; set; }
        public IEventAggregator EventAggregator { get; }

        public LinkCodeWrapper(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
        }
    }
}
