using Civil3DSubassemblyAnnotations.Events;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DSubassemblyAnnotations.Models
{
    public class CorridorCode : BindableBase
    {
        public CorridorCode(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        private bool isSelected;
        private readonly IEventAggregator eventAggregator;

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }

            set
            {
                isSelected = value;
                eventAggregator.GetEvent<CorridorCodeUpdateEvent>().Publish(this);
                RaisePropertyChanged();
            }
        }
        public string Code { get; set; }
    }
}
