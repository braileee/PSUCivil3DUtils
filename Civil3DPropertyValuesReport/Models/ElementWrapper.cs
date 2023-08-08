using Acad = Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsSystem;
using Civil = Autodesk.Civil.DatabaseServices;
using Civil3DUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Civil3DPropertyValuesReport.Models
{
    public class ElementWrapper : BindableBase
    {
        private int accuracy;
        private string name;

        public ElementWrapper(Acad.DBObject dBObject, int accurracy)
        {
            DbObject = dBObject;
            Accuracy = accurracy;
            Name = GetName();
        }

        public Acad.DBObject DbObject { get; set; }
        public int Accuracy
        {
            get { return accuracy; }
            set
            {
                accuracy = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Volume));
                RaisePropertyChanged(nameof(Length2d));
                RaisePropertyChanged(nameof(Length3d));
                RaisePropertyChanged(nameof(Area2d));
                RaisePropertyChanged(nameof(Area3d));
                RaisePropertyChanged(nameof(Thickness));
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                RaisePropertyChanged();
            }
        }

        private string GetName()
        {
            Autodesk.Aec.DatabaseServices.Entity aecDbObject = DbObject as Autodesk.Aec.DatabaseServices.Entity;

            if (aecDbObject == null)
            {
                return DbObject?.GetType()?.Name;
            }

            return aecDbObject?.DisplayName;
        }

        public string Layer
        {
            get
            {
                Acad.Entity entity = DbObject as Acad.Entity;
                return entity?.Layer;
            }
        }

        public string Style
        {
            get
            {
                try
                {
                    Civil.Entity entity = DbObject as Civil.Entity;
                    return entity?.StyleName;
                }
                catch (Exception)
                {
                    return string.Empty;
                }

            }
        }

        public double Volume
        {
            get
            {
                if (DbObject is Acad.Solid3d solid3d)
                {
                    Acad.Solid3dMassProperties massProperties = solid3d.MassProperties;
                    return Math.Round(massProperties.Volume, Accuracy);
                }

                return 0;
            }
        }

        public double Length2d
        {
            get
            {
                if (DbObject is Civil.FeatureLine featureLine)
                {
                    return Math.Round(featureLine.Length2D, Accuracy);
                }
                else if (DbObject is Civil.Alignment alignment)
                {
                    return Math.Round(alignment.Length, Accuracy);
                }
                else if (DbObject is Civil.Profile profile)
                {
                    return Math.Round(profile.Length, Accuracy);
                }
                else if (DbObject is Civil.Pipe pipe)
                {
                    return Math.Round(pipe.Length2DCenterToCenter, Accuracy);
                }
                else if (DbObject is Acad.Polyline polyline)
                {
                    return Math.Round(polyline.Length, Accuracy);
                }
                else if (DbObject is Acad.Polyline2d polyline2d)
                {
                    return Math.Round(polyline2d.Length, Accuracy);
                }
                else if (DbObject is Acad.Polyline3d polyline3d)
                {
                    return Math.Round(polyline3d.Length, Accuracy);
                }

                return 0;
            }
        }

        public double Length3d
        {
            get
            {
                if (DbObject is Civil.FeatureLine featureLine)
                {
                    return Math.Round(featureLine.Length3D, Accuracy);
                }
                else if (DbObject is Civil.Pipe pipe)
                {
                    return Math.Round(pipe.Length3DCenterToCenter, Accuracy);
                }

                return Length2d;
            }
        }

        public double Area2d
        {
            get
            {
                if (DbObject is Acad.Solid3d solid3d)
                {
                    if (solid3d == null)
                    {
                        return 0;
                    }

                    return Math.Round(solid3d.Area, Accuracy);
                }
                else if (DbObject is Civil.Parcel parcel)
                {
                    return Math.Round(parcel.Area, Accuracy);
                }
                else if (DbObject is Civil.TinSurface tinSurface)
                {
                    return Math.Round(tinSurface.GetTerrainProperties().SurfaceArea2D, Accuracy);
                }
                else if (DbObject is Acad.Hatch hatch)
                {
                    return Math.Round(hatch.GetArea(), Accuracy);
                }

                return 0;
            }
        }

        public double Area3d
        {
            get
            {
                if (DbObject is Civil.TinSurface tinSurface)
                {
                    return Math.Round(tinSurface.GetTerrainProperties().SurfaceArea3D, Accuracy);
                }

                return Area2d;
            }
        }

        public double Thickness
        {
            get
            {
                if (DbObject is Acad.Solid3d solid3d)
                {
                    if (solid3d == null)
                    {
                        return 0;
                    }

                    if (!solid3d.Bounds.HasValue)
                    {
                        return 0;
                    }

                    Acad.Extents3d value = solid3d.Bounds.Value;

                    return Math.Round(value.MaxPoint.Z - value.MinPoint.Z, Accuracy);
                }

                return 0;
            }
        }

        public int Count
        {
            get
            {
                return 1;
            }
        }
    }
}
