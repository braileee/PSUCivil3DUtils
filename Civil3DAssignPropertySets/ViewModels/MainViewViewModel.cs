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
using Autodesk.Aec.PropertyData.DatabaseServices;

namespace Civil3DAssignPropertySets.ViewModels
{
    public class MainViewViewModel : BindableBase
    {
        public DelegateCommand LoadExcelCommand { get; }
        public DelegateCommand SelectElements { get; }
        public DelegateCommand SetPropertySetValuesCommand { get; }
        public DelegateCommand SelectSourceElement { get; }
        public DelegateCommand SelectDestinationElements { get; }

        public string LoadExcelInfo
        {
            get
            {
                return loadExcelInfo;
            }
            set
            {
                loadExcelInfo = value;
                RaisePropertyChanged();
            }
        }

        public string SelectElementsInfo
        {
            get
            {
                return selectElementsInfo;
            }
            set
            {
                selectElementsInfo = value;
                RaisePropertyChanged();
            }
        }

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
        private string loadExcelInfo;
        private string selectElementsInfo;

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
        public DBObject SourceElement { get; private set; }
        public List<DBObject> DestinationElements { get; private set; } = new List<DBObject>();

        public MainViewViewModel(IEventAggregator eventAggregator)
        {
            string assemblyLocation = System.Reflection.Assembly.GetAssembly(typeof(MainViewViewModel)).Location;
            string assemblyFolder = Path.GetDirectoryName(assemblyLocation);

            DocumentCollection documentCollection = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            documentCollection.DocumentActivated += DocumentCollectionDocumentActivated;

            LoadExcelCommand = new DelegateCommand(OnLoadExcelCommand);
            SelectElements = new DelegateCommand(OnSelectElements);
            SetPropertySetValuesCommand = new DelegateCommand(OnSetPropertySetValuesCommand);

            SelectSourceElement = new DelegateCommand(OnSelectSourceElement);
            SelectDestinationElements = new DelegateCommand(OnSelectDestinationElements);

            LoadExcelInfo = "Load";

            SelectElementsInfo = "Select";
        }

        private void OnSelectDestinationElements()
        {
            try
            {
                using (AutocadDocumentService.LockActiveDocument())
                {
                    if (SourceElement == null)
                    {
                        return;
                    }

                    DestinationElements = SelectionUtils.GetDbObjects("Select destination elements", OpenMode.ForWrite);


                    Dictionary<PropertySetDefinition, List<PropertyDefinition>> setPerProperties = PropertySetUtils.GetAllPropertySetAndPropertyDefinitions(OpenMode.ForRead);

                    List<ElementWrapper> elementsWrappers = new List<ElementWrapper>();

                    foreach (var setPerPropertiesItem in setPerProperties)
                    {
                        foreach (var property in setPerPropertiesItem.Value)
                        {
                            object value = PropertySetUtils.GetPropertyValue(setPerPropertiesItem.Key, property, SourceElement);
                            elementsWrappers.Add(new ElementWrapper
                            {
                                DbObject = SourceElement,
                                PropertyDefinition = property,
                                PropertySetDefinition = setPerPropertiesItem.Key,
                                Value = value,
                            });
                        }
                    }

                    foreach (DBObject destinationElement in DestinationElements)
                    {
                        foreach (var setPerPropertiesItem in setPerProperties)
                        {
                            foreach (var property in setPerPropertiesItem.Value)
                            {
                                ElementWrapper elementWrapper = elementsWrappers.FirstOrDefault(item => item.PropertySetDefinition.Name == setPerPropertiesItem.Key.Name && item.PropertyDefinition.Name == property.Name);

                                if (elementWrapper != null)
                                {
                                    switch (property.DataType)
                                    {
                                        case Autodesk.Aec.PropertyData.DataType.Integer:
                                            PropertySetUtils.SetValueToProperty(setPerPropertiesItem.Key, property, destinationElement, (int)elementWrapper.Value);
                                            break;
                                        case Autodesk.Aec.PropertyData.DataType.Real:
                                            PropertySetUtils.SetValueToProperty(setPerPropertiesItem.Key, property, destinationElement, (double)elementWrapper.Value);
                                            break;
                                        case Autodesk.Aec.PropertyData.DataType.Text:
                                            PropertySetUtils.SetValueToProperty(setPerPropertiesItem.Key, property, destinationElement, (string)elementWrapper.Value);
                                            break;
                                        case Autodesk.Aec.PropertyData.DataType.TrueFalse:
                                            PropertySetUtils.SetValueToProperty(setPerPropertiesItem.Key, property, destinationElement, (int)elementWrapper.Value);
                                            break;
                                        case Autodesk.Aec.PropertyData.DataType.AutoIncrement:
                                            break;
                                        case Autodesk.Aec.PropertyData.DataType.AlphaIncrement:
                                            break;
                                        case Autodesk.Aec.PropertyData.DataType.List:
                                            break;
                                        case Autodesk.Aec.PropertyData.DataType.Graphic:
                                            break;
                                        default:
                                            break;
                                    }

                                }
                            }
                        }
                    }

                    MessageBox.Show("Done");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Error: {exception.Message}");
            }
        }

        private void OnSelectSourceElement()
        {
            SourceElement = SelectionUtils.GetDbObject("Select source element", OpenMode.ForRead);
        }

        private void OnSetPropertySetValuesCommand()
        {
            try
            {
                List<PropertySetItem> propertySetItems = PropertySetItems.Where(item => item.Group == SelectedPropertySetGroup).ToList();
                List<string> classNames = SelectedEntities.Select(entity => entity.GetRXClass().Name).Distinct().ToList();

                List<PropertyAssigner> assigners = new List<PropertyAssigner>();

                using (AutocadDocumentService.LockActiveDocument())
                {
                    using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                    {
                        foreach (PropertySetItem propertySetItem in propertySetItems)
                        {
                            Autodesk.Aec.PropertyData.DatabaseServices.PropertySetDefinition propertySetDefinition = null;
                            Autodesk.Aec.PropertyData.DatabaseServices.PropertyDefinition propertyDefinition = null;

                            PropertySetUtils.GetPropertySetAndPropertyDefinitionByName(OpenMode.ForWrite, propertySetItem.PropertySet, propertySetItem.Property, ref propertySetDefinition, ref propertyDefinition);

                            if (propertySetDefinition == null)
                            {
                                propertySetDefinition = PropertySetUtils.CreatePropertySetDefinition(propertySetItem.PropertySet, classNames, transaction);
                            }

                            if (propertyDefinition == null)
                            {
                                propertySetDefinition = transaction.GetObject(propertySetDefinition.Id, OpenMode.ForWrite, false, true) as PropertySetDefinition;

                                if (double.TryParse(propertySetItem.Value, out double value) && !double.IsNaN(value) && !double.IsInfinity(value))
                                {
                                    propertyDefinition = PropertySetUtils.CreatePropertyDefinition(propertySetDefinition, propertySetItem.Property, Autodesk.Aec.PropertyData.DataType.Real);
                                }
                                else
                                {
                                    propertyDefinition = PropertySetUtils.CreatePropertyDefinition(propertySetDefinition, propertySetItem.Property, Autodesk.Aec.PropertyData.DataType.Text);
                                }
                            }

                            assigners.Add(new PropertyAssigner
                            {
                                PropertyDefinition = propertyDefinition,
                                PropertySetDefinition = propertySetDefinition,
                                PropertySetItem = propertySetItem,
                            });
                        }

                        transaction.Commit();
                    }
                }

                using (AutocadDocumentService.LockActiveDocument())
                {
                    using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                    {
                        foreach (Entity entity in SelectedEntities)
                        {
                            foreach (PropertyAssigner assigner in assigners)
                            {
                                if (entity == null)
                                {
                                    continue;
                                }

                                Entity entityOpen = transaction.GetObject(entity.Id, OpenMode.ForWrite, false, true) as Entity;

                                if (assigner.PropertyDefinition.DataType == Autodesk.Aec.PropertyData.DataType.Text)
                                {
                                    PropertySetUtils.SetValueToProperty(assigner.PropertySetDefinition, assigner.PropertyDefinition, entityOpen, assigner.PropertySetItem.Value);
                                }
                                else if (assigner.PropertyDefinition.DataType == Autodesk.Aec.PropertyData.DataType.Real)
                                {
                                    double value = NumbersUtils.ParseStringToDouble(assigner.PropertySetItem.Value);
                                    PropertySetUtils.SetValueToProperty(assigner.PropertySetDefinition, assigner.PropertyDefinition, entityOpen, value);
                                }
                                else if (assigner.PropertyDefinition.DataType == Autodesk.Aec.PropertyData.DataType.Integer)
                                {
                                    int value = NumbersUtils.ParseStringToInt(assigner.PropertySetItem.Value);
                                    PropertySetUtils.SetValueToProperty(assigner.PropertySetDefinition, assigner.PropertyDefinition, entityOpen, value);
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
                SelectElementsInfo = $"Selected: {SelectedEntities.Count}";
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
                        SelectedPropertySetGroup = PropertySetGroups.FirstOrDefault();
                    }
                }

                LoadExcelInfo = Path.GetFileName(ExcelFilePath);
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
