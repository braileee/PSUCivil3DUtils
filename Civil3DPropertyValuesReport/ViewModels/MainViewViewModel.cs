using AutoCADUtils;
using Autodesk.AutoCAD.ApplicationServices;
using Acad = Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using Civil3DPropertyValuesReport.Models;
using System.Linq;
using System.Windows.Documents;
using Autodesk.AutoCAD.ApplicationServices.Core;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.Windows.Data;
using Prism.Commands;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Civil3DPropertyValuesReport.ViewModels
{
    public class MainViewViewModel : BindableBase
    {
        private IEventAggregator _eventAggregator;
        private string selectedRoundingSign;

        public MainViewViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            DocumentCollection documentCollection = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            documentCollection.DocumentActivated += DocumentCollectionDocumentActivated;

            AutocadDocumentService.ActiveDocument.ImpliedSelectionChanged += ActiveDocumentImpliedSelectionChanged;

            RoundingSigns = new List<string> { "0", "0.0", "0.00", "0.000", "0.0000" };
            SelectedRoundingSign = RoundingSigns.FirstOrDefault(sign => sign == "0.00");

            ExportCommand = new DelegateCommand(OnExportCommand);
        }

        private void OnExportCommand()
        {
            MessageBox.Show("Not implemented", "Error");
        }

        private void ActiveDocumentImpliedSelectionChanged(object sender, EventArgs e)
        {
            SelectedElements.Clear();
            TotalElements.Clear();
            SelectedLineElements.Clear();

            SelectionSet oSelectionSet = AutocadDocumentService.Editor?.SelectImplied()?.Value;

            if (oSelectionSet == null)
            {
                return;
            }

            using (Acad.Transaction ts = AutocadDocumentService.TransactionManager.StartOpenCloseTransaction())
            {
                foreach (SelectedObject addedObject in oSelectionSet)
                {
                    if (addedObject == null)
                    {
                        continue;
                    }

                    if (addedObject.ObjectId.IsNull)
                    {
                        continue;
                    }

                    Acad.DBObject dBObject = ts.GetObject(addedObject.ObjectId, Acad.OpenMode.ForRead, false, true);

                    if (dBObject == null)
                    {
                        continue;
                    }

                    ElementWrapper elementWrapper = new ElementWrapper(dBObject, SelectedRoundingValue);
                    SelectedElements.Add(elementWrapper);

                    if (dBObject is Polyline3d || dBObject is FeatureLine || dBObject is Polyline)
                    {
                        List<LineSegmentWrapper> lineSegmentWrappers = LineSegmentWrapper.Get(dBObject, SelectedRoundingValue);
                        SelectedLineElements.AddRange(lineSegmentWrappers);
                    }
                }

                TotalWrapper totalWrapper = TotalWrapper.CreateOneTotal(SelectedElements, SelectedRoundingValue);

                TotalElements.Add(totalWrapper);

                SelectedElements.CollectionChanged += SelectedElementsCollectionChanged;

                ts.Commit();
            }
        }

        private void SelectedElementsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (INotifyPropertyChanged item in e.OldItems)
                    item.PropertyChanged -= ItemPropertyChanged;
            }
            if (e.NewItems != null)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                    item.PropertyChanged += ItemPropertyChanged;
            }
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

        public ObservableCollection<ElementWrapper> SelectedElements { get; set; } = new ObservableCollection<ElementWrapper>();

        private LineSegmentWrapper selectedLineElement;
        public LineSegmentWrapper SelectedLineElement
        {
            get { return selectedLineElement; }
            set
            {
                selectedLineElement = value;

                if (selectedLineElement != null)
                {
                    double minX = Math.Min(SelectedLineElement.StartPoint.X, SelectedLineElement.EndPoint.X);
                    double minY = Math.Min(SelectedLineElement.StartPoint.Y, SelectedLineElement.EndPoint.Y);
                    double minZ = Math.Min(SelectedLineElement.StartPoint.Z, SelectedLineElement.EndPoint.Z);

                    double maxX = Math.Max(SelectedLineElement.StartPoint.X, SelectedLineElement.EndPoint.X);
                    double maxY = Math.Max(SelectedLineElement.StartPoint.Y, SelectedLineElement.EndPoint.Y);
                    double maxZ = Math.Max(SelectedLineElement.StartPoint.Z, SelectedLineElement.EndPoint.Z);

                    double offset = 0.1;

                    Point3d minPoint = new Point3d(minX - offset, minY - offset, minZ);
                    Point3d maxPoint = new Point3d(maxX + offset, maxY + offset, maxZ);

                    AutocadDocumentService.ZoomTo(minPoint, maxPoint);
                }

                RaisePropertyChanged();
            }
        }

        public ObservableCollection<LineSegmentWrapper> SelectedLineElements { get; set; } = new ObservableCollection<LineSegmentWrapper>();
        public ObservableCollection<TotalWrapper> TotalElements { get; set; } = new ObservableCollection<TotalWrapper>();
        public List<string> RoundingSigns { get; set; } = new List<string>();
        public string SelectedRoundingSign
        {
            get
            {
                return selectedRoundingSign;
            }
            set
            {
                selectedRoundingSign = value;

                foreach (ElementWrapper elementWrapper in SelectedElements)
                {
                    elementWrapper.Accuracy = SelectedRoundingValue;
                }

                foreach (TotalWrapper totalElement in TotalElements)
                {
                    totalElement.Accurracy = SelectedRoundingValue;
                }

                RaisePropertyChanged();
            }
        }

        public DelegateCommand ExportCommand { get; }

        public int SelectedRoundingValue
        {
            get
            {
                switch (SelectedRoundingSign)
                {
                    case "0":
                        return 0;
                    case "0.0":
                        return 1;
                    case "0.00":
                        return 2;
                    case "0.000":
                        return 3;
                    case "0.0000":
                        return 4;
                }

                return 2;
            }
        }

        private void DocumentCollectionDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            RaiseCloseRequest();
        }

        protected void RaiseCloseRequest()
        {
            OnRequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OnRequestClose;
    }
}
