using Autodesk.Civil.DatabaseServices;
using Civil3DBatchExtractSolids.Events;
using Prism.Events;
using Prism.Mvvm;

namespace Civil3DBatchExtractSolids.Models
{
    public class ShapeWrapper : BindableBase
    {
        public ShapeWrapper(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public Corridor Corridor { get; set; }

        public string Name { get; set; }

        private bool isSelected;
        private readonly IEventAggregator _eventAggregator;

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;

                if (!StopSelectEvent)
                {
                    _eventAggregator.GetEvent<ShapeSelectEvent>().Publish(this);
                }

                RaisePropertyChanged();
            }
        }

        public bool IsSelectAllShape
        {
            get
            {
                return Name == Constants.SelectAllName;
            }
        }

        public bool StopSelectEvent { get; set; }

        public override string ToString()
        {
            return string.Empty;
        }
    }
}
