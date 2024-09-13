using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.Aec.PropertyData.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows.Data;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using Civil3DIFCBatchImport.Properties;
using Civil3DUtils;
using Civil3DUtils.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using static Autodesk.AutoCAD.EditorInput.Editor;
using Acad = Autodesk.AutoCAD.DatabaseServices;
using Civil = Autodesk.Civil.DatabaseServices;

namespace Civil3DIFCBatchImport.ViewModels
{
    public class MainViewViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        public DelegateCommand SelectDirectoryCommand { get; }
        public DelegateCommand ExportCommand { get; }
        public DelegateCommand SelectTemplateCommand { get; }

        public MainViewViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            DocumentCollection documentCollection = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            documentCollection.DocumentActivated += DocumentCollectionDocumentActivated;

            SelectDirectoryCommand = new DelegateCommand(OnSelectDirectoryCommand);

            ExportCommand = new DelegateCommand(OnExportCommand);

            SelectTemplateCommand = new DelegateCommand(OnSelectTemplateCommand);

            TemplateFilePath = Settings.Default.TemplateFilePath;

            ExportValuesCommand = new DelegateCommand(OnExportValuesCommand);
            ImportValuesCommand = new DelegateCommand(OnImportValuesCommand);

            SelectedDirectoryInfo = "Select";

            SelectedTemplateInfo = string.IsNullOrEmpty(TemplateFilePath) ? "Select" : $"Selected: {Path.GetFileNameWithoutExtension(TemplateFilePath)}";
        }

        private string selectedTemplateInfo;
        public string SelectedTemplateInfo
        {
            get { return selectedTemplateInfo; }
            set
            {
                selectedTemplateInfo = value;
                RaisePropertyChanged();
            }
        }

        private void OnExportValuesCommand()
        {
            Acad.DBObject dbObject = SelectionUtils.GetDbObject("Select an element to export property set values", OpenMode.ForRead);

            List<PropertySetDefinition> propertySetDefinitions = PropertySetUtils.GetAllPropertySetDefinitions(OpenMode.ForRead);

            foreach (PropertySetDefinition propertySetDefinition in propertySetDefinitions)
            {
                List<PropertyDefinition> propertyDefinitions = PropertySetUtils.GetAllPropertyDefinitionsOfPropertySet(propertySetDefinition);

                foreach (PropertyDefinition propertyDefinition in propertyDefinitions)
                {
                    // Get values by different types and create json file with them

                    switch (propertyDefinition.DataType)
                    {
                        case Autodesk.Aec.PropertyData.DataType.Integer:
                            break;
                        case Autodesk.Aec.PropertyData.DataType.Real:
                            break;
                        case Autodesk.Aec.PropertyData.DataType.Text:
                            break;
                        case Autodesk.Aec.PropertyData.DataType.TrueFalse:
                            break;
                        case Autodesk.Aec.PropertyData.DataType.AutoIncrement:
                            break;
                        case Autodesk.Aec.PropertyData.DataType.AlphaIncrement:
                            break;
                        case Autodesk.Aec.PropertyData.DataType.List:
                            break;
                        default:
                            break;
                    }

                }
            }
        }

        private void OnImportValuesCommand()
        {
            // Import values from json to selected elements
        }

        private void OnSelectTemplateCommand()
        {
            string assemblyLocation = System.Reflection.Assembly.GetAssembly(typeof(MainViewViewModel)).Location;
            string assemblyFolder = Path.GetDirectoryName(assemblyLocation);

            string templatesFolder = Path.Combine(assemblyFolder, "Files", "Template");

            if (!Directory.Exists(templatesFolder))
            {
                templatesFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            TemplateFilePath = FileUtils.GetFilePath(templatesFolder, "DWT files (*.dwt)|*.dwt|All files (*.*)|*.*");

            SelectedTemplateInfo = $"Selected: {Path.GetFileNameWithoutExtension(TemplateFilePath)}";
        }

        private string templateFilePath;
        public string TemplateFilePath
        {
            get { return templateFilePath; }
            set
            {
                templateFilePath = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand ExportValuesCommand { get; }
        public DelegateCommand ImportValuesCommand { get; }
        public string SelectedDirectoryInfo
        {
            get
            {
                return selectedDirectoryInfo;
            }
            set
            {
                selectedDirectoryInfo = value;
                RaisePropertyChanged();
            }
        }

        private void OnExportCommand()
        {
            try
            {
                if (string.IsNullOrEmpty(FolderPath))
                {
                    MessageBox.Show("No folder was selected");
                    return;
                }

                if (string.IsNullOrEmpty(TemplateFilePath))
                {
                    MessageBox.Show("No template file");
                    return;
                }

                if (!File.Exists(TemplateFilePath))
                {
                    MessageBox.Show("Template file doesn't exist");
                    return;
                }

                List<string> dwgFiles = new List<string>();

                foreach (string ifcFile in IfcFiles)
                {
                    string directory = Path.GetDirectoryName(ifcFile);
                    string dwgFile = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(ifcFile)}.dwg");

                    dwgFiles.Add(dwgFile);
                    string command = $"-IFCIMPORT\n{ifcFile}\nNo\n{dwgFile}\nOptions\nTemplate\n{templateFilePath}\neXit\nImport\nNo\n";
                    AutocadDocumentService.ActiveDocument.SendStringToExecute(command, true, false, false);
                }

                AutocadDocumentService.ActiveDocument.SendStringToExecute(string.Concat(Enumerable.Repeat("\nNo\n", 2)), true, false, false);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void OnSelectDirectoryCommand()
        {
            FolderPath = !File.Exists(Settings.Default.FolderPath) ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop) : Settings.Default.FolderPath;
            FolderPath = FolderUtils.GetFolderPathExtendedWindow(FolderPath);

            if (string.IsNullOrEmpty(FolderPath))
            {
                return;
            }

            string[] ifcFilesArray = Directory.GetFiles(FolderPath, "*.ifc", SearchOption.AllDirectories);

            if (ifcFilesArray != null && ifcFilesArray.Length > 0)
            {
                IfcFiles = ifcFilesArray.ToList();
            }

            SelectedDirectoryInfo = $"Selected: {IfcFiles.Count}";
        }

        private bool isUnitsMillimeters;
        public bool IsUnitsMillimeters
        {
            get { return isUnitsMillimeters; }
            set
            {
                isUnitsMillimeters = value;
                RaisePropertyChanged();
            }
        }

        private bool isUnitsMeters;
        private string folderPath;
        private string selectedDirectoryInfo;

        public bool IsUnitsMeters
        {
            get { return isUnitsMeters; }
            set
            {
                isUnitsMeters = value;
                RaisePropertyChanged();
            }
        }

        public string FolderPath
        {
            get
            {
                return folderPath;
            }
            set
            {
                folderPath = value;

                Settings.Default.FolderPath = folderPath;
                Settings.Default.Save();

                RaisePropertyChanged();
            }
        }

        public List<string> IfcFiles { get; set; } = new List<string>();

        private void DocumentCollectionDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
        }

        protected void RaiseCloseRequest()
        {
            OnRequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OnRequestClose;
    }
}
