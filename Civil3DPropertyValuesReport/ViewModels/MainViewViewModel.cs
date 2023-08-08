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

                    SelectedElements.Add(new ElementWrapper(dBObject, SelectedRoundingValue));
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
