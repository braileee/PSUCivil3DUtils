using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows.Data;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using Civil3DIFCBatchImport.Properties;
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

                string[] ifcFiles = Directory.GetFiles(FolderPath, "*.ifc", SearchOption.AllDirectories);

                List<string> dwgFiles = new List<string>();

                foreach (string ifcFile in ifcFiles)
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
