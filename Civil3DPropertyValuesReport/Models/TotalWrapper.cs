using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DPropertyValuesReport.Models
{
    public class TotalWrapper : BindableBase
    {
        public static TotalWrapper CreateOneTotal(List<ElementWrapper> elements)
        {
            return new TotalWrapper
            {
                Name = "-",
                Layer = "-",
                Style = "-",
                Volume = elements.Sum(item => item.Volume),

            };
        }


        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged();
            }
        }

        private string layer;
        public string Layer
        {
            get { return layer; }
            set
            {
                layer = value;
                RaisePropertyChanged();
            }
        }

        private string style;
        public string Style
        {
            get { return style; }
            set
            {
                style = value;
                RaisePropertyChanged();
            }
        }

        private double volume;
        private double length2d;
        private double length3d;
        private double area2d;
        private double area3d;
        private double thickness;
        private int count;

        public double Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        public double Length2d
        {
            get { return length2d; }
            set
            {
                length2d = value;
                RaisePropertyChanged();
            }
        }

        public double Length3d
        {
            get
            {
                return length3d;
            }
            set
            {
                length3d = value;
                RaisePropertyChanged();
            }
        }

        public double Area2d
        {
            get
            {
                return area2d;
            }
            set
            {
                area2d = value;
                RaisePropertyChanged();
            }
        }

        public double Area3d
        {
            get
            {
                return area3d;
            }
            set
            {
                area3d = value;
                RaisePropertyChanged();
            }
        }

        public double Thickness
        {
            get
            {
                return thickness;
            }
            set
            {
                thickness = value;
                RaisePropertyChanged();
            }
        }

        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                RaisePropertyChanged();
            }
        }

    }
}
