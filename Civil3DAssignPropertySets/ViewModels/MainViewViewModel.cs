using Acad = Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Civil = Autodesk.Civil.DatabaseServices;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices.Styles;
using System.Windows;
using System.IO;
using System.Collections.ObjectModel;
using Civil3DUtils.Utils;
using AutoCADUtils.Utils;
using AutoCADUtils;
using ExcelDataReader;
using System.Data;
using Civil3DAssignPropertySets.Models;
using Civil3DUtils;
using System.Reflection;

namespace Civil3DAssignPropertySets.ViewModels
{
    public class MainViewViewModel : BindableBase
    {
        public DelegateCommand LoadExcelCommand { get; }
        public DelegateCommand SelectElements { get; }
        public DelegateCommand SetPropertySetValuesCommand { get; }
        public string ExcelFilePath { get; private set; }
        public List<PropertySetItem> PropertySetItems { get; private set; } = new List<PropertySetItem>();

        private List<string> propertySetGroups = new List<string>();
        public List<string> PropertySetGroups
        {
            get
            {
                return propertySetGroups;
            }
            set
            {
                propertySetGroups = value;
                RaisePropertyChanged();
            }
        }

        private string selectedPropertySetGroup;
        public string SelectedPropertySetGroup
        {
            get { return selectedPropertySetGroup; }
            set
            {
                selectedPropertySetGroup = value;
                RaisePropertyChanged();
            }
        }

        public List<Entity> SelectedEntities { get; private set; } = new List<Entity>();

        public MainViewViewModel(IEventAggregator eventAggregator)
        {
            string assemblyLocation = System.Reflection.Assembly.GetAssembly(typeof(MainViewViewModel)).Location;
            string assemblyFolder = Path.GetDirectoryName(assemblyLocation);

            DocumentCollection documentCollection = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            documentCollection.DocumentActivated += DocumentCollectionDocumentActivated;

            LoadExcelCommand = new DelegateCommand(OnLoadExcelCommand);
            SelectElements = new DelegateCommand(OnSelectElements);
            SetPropertySetValuesCommand = new DelegateCommand(OnSetPropertySetValuesCommand);
        }

        private void OnSetPropertySetValuesCommand()
        {
            try
            {
                List<PropertySetItem> propertySetItems = PropertySetItems.Where(item => item.Group == SelectedPropertySetGroup).ToList();

                using (AutocadDocumentService.LockActiveDocument())
                {
                    using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                    {
                        foreach (Entity entity in SelectedEntities)
                        {
                            if (entity == null)
                            {
                                continue;
                            }

                            Entity entityOpen = transaction.GetObject(entity.Id, OpenMode.ForWrite, false, true) as Entity;

                            foreach (PropertySetItem propertySetItem in propertySetItems)
                            {
                                Autodesk.Aec.PropertyData.DatabaseServices.PropertySetDefinition propertySetDefinition = null;
                                Autodesk.Aec.PropertyData.DatabaseServices.PropertyDefinition propertyDefinition = null;

                                PropertySetUtils.GetPropertySetAndPropertyDefinitionByName(OpenMode.ForRead, propertySetItem.PropertySet, propertySetItem.Property, ref propertySetDefinition, ref propertyDefinition);

                                if(propertySetDefinition == null || propertyDefinition == null)
                                {
                                    continue;
                                }

                                if (propertyDefinition.DataType == Autodesk.Aec.PropertyData.DataType.Text)
                                {
                                    PropertySetUtils.SetValueToProperty(propertySetDefinition, propertyDefinition, entityOpen, propertySetItem.Value);
                                }
                                else if (propertyDefinition.DataType == Autodesk.Aec.PropertyData.DataType.Real)
                                {
                                    double value = NumbersUtils.ParseStringToDouble(propertySetItem.Value);
                                    PropertySetUtils.SetValueToProperty(propertySetDefinition, propertyDefinition, entityOpen, value);
                                }
                                else if (propertyDefinition.DataType == Autodesk.Aec.PropertyData.DataType.Integer)
                                {
                                    int value = NumbersUtils.ParseStringToInt(propertySetItem.Value);
                                    PropertySetUtils.SetValueToProperty(propertySetDefinition, propertyDefinition, entityOpen, value);
                                }
                            }
                        }

                        transaction.Commit();
                    }
                }

                RaiseCloseRequest();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error", exception.Message);
            }
        }

        private void OnSelectElements()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedPropertySetGroup))
                {
                    MessageBox.Show("Error", "No property set group was selected");
                    return;
                }

                SelectedEntities = SelectionUtils.GetEntities("Get elements to assign the property values");

            }
            catch (Exception exception)
            {
                MessageBox.Show("Error", exception.Message);
            }
        }

        private void OnLoadExcelCommand()
        {
            try
            {
                string assemblyLocation = Assembly.GetAssembly(typeof(MainViewViewModel)).Location;
                string assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

                string exampleFilePath = Path.Combine(assemblyDirectory, "Files", "Civil3DAssignPropertySets");

                ExcelFilePath = FileUtils.GetFilePath(exampleFilePath, "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*");

                if (string.IsNullOrEmpty(ExcelFilePath))
                {
                    return;
                }

                using (var stream = File.Open(ExcelFilePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        DataSet dataSet = reader.AsDataSet();
                        PropertySetItems = PropertySetItem.Convert(dataSet);
                        PropertySetGroups = PropertySetItems.Select(item => item.Group).Distinct().OrderBy(item => item).ToList();
                    }
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show("Error", $"{exception.Message}");
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
