using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DPropertyValuesReport.Models
{
    public class TotalWrapper : BindableBase
    {
        private const string VariesValue = "**VARIES**";

        private TotalWrapper()
        {
        }

        public static TotalWrapper CreateOneTotal(ObservableCollection<ElementWrapper> elements, int accuracy)
        {
            return new TotalWrapper
            {
                Elements = elements,
                Accurracy = accuracy
            };
        }

        public string Name
        {
            get
            {
                ElementWrapper elementWrapper = Elements.FirstOrDefault();

                if (Elements.All(element => element?.Name == elementWrapper?.Name))
                {
                    return elementWrapper?.Name;
                }
                else
                {
                    return VariesValue;
                }
            }
        }

        public string Layer
        {
            get
            {
                ElementWrapper elementWrapper = Elements.FirstOrDefault();

                if (Elements.All(element => element?.Layer == elementWrapper?.Layer))
                {
                    return elementWrapper?.Layer;
                }
                else
                {
                    return VariesValue;
                }
            }
        }

        public string Style
        {
            get
            {
                ElementWrapper elementWrapper = Elements.FirstOrDefault();

                if (Elements.All(element => element?.Style == elementWrapper?.Style))
                {
                    return elementWrapper?.Style;
                }
                else
                {
                    return VariesValue;
                }
            }
        }

        public double Volume
        {
            get
            {
                return Math.Round(Elements.Sum(element => element.Volume), Accurracy);
            }
        }

        public double Length2d
        {
            get
            {
                return Math.Round(Elements.Sum(element => element.Length2d), Accurracy);
            }
        }

        public double Length3d
        {
            get
            {
                return Math.Round(Elements.Sum(element => element.Length3d), Accurracy);
            }
        }

        public double Area2d
        {
            get
            {
                return Math.Round(Elements.Sum(element => element.Area2d), Accurracy);
            }
        }

        public double Area3d
        {
            get
            {
                return Math.Round(Elements.Sum(element => element.Area3d), Accurracy);
            }
        }

        public double Thickness
        {
            get
            {
                return Math.Round(Elements.Sum(element => element.Thickness), Accurracy);
            }
        }

        public int Count
        {
            get
            {
                return Elements.Sum(element => element.Count);
            }
        }

        public int Accurracy { get; private set; }
        public ObservableCollection<ElementWrapper> Elements { get; private set; } = new ObservableCollection<ElementWrapper>();
    }
}
