using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Civil3DBatchExtractSolids.Events;
using Civil3DBatchExtractSolids.Models;
using Civil3DBatchExtractSolids.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Civil3DBatchExtractSolids.ViewModels
{
    public class MainViewViewModel : BindableBase
    {
        public MainViewViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            List<Corridor> corridors = CorridorUtils.GetAllTheCorridors(OpenMode.ForRead);
            Corridors = corridors.Where(corridor => corridor != null).Select(corridor => CorridorWrapper.Create(corridor)).ToList();

            _eventAggregator.GetEvent<ShapeSelectEvent>().Subscribe(OnShapeSelectEvent);

            ExtractCommand = new DelegateCommand(OnExtractCommand);
        }

        private void OnShapeSelectEvent(ShapeWrapper wrapper)
        {
            try
            {
                ShapeWrapper selectAllShape = Shapes.FirstOrDefault(item => item.Name == Constants.SelectAllName);

                if (wrapper.IsSelectAllShape)
                {
                    foreach (ShapeWrapper shapeWrapper in Shapes)
                    {
                        if (shapeWrapper.IsSelectAllShape)
                        {
                            continue;
                        }

                        shapeWrapper.StopSelectEvent = true;
                        shapeWrapper.IsSelected = wrapper.IsSelected;
                        shapeWrapper.StopSelectEvent = false;
                    }
                }

                List<ShapeWrapper> shapesWithoutSelectAll = Shapes.Where(item => !item.IsSelectAllShape).ToList();
                List<ShapeWrapper> selectedShapes = Shapes.Where(shape => shape.IsSelected && !shape.IsSelectAllShape).ToList();

                if (!shapesWithoutSelectAll.All(shape => shape.IsSelected))
                {
                    selectAllShape.StopSelectEvent = true;
                    selectAllShape.IsSelected = false;
                    selectAllShape.StopSelectEvent = false;
                }

                List<string> names = selectedShapes.Select(shape => shape.Name).ToList();
                SelectedShapesInfo = string.Join(", ", names);
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Error on selecting shape. {exception.Message}", "Error");
            }
        }

        private void OnExtractCommand()
        {
#if (CIVIL3D2023DEBUG || CIVIL3D2024DEBUG)
            try
            {
                string solidsFilePath = DialogUtils.SaveFileToFolder(Environment.SpecialFolder.Desktop, ".dwg");

                if (string.IsNullOrEmpty(solidsFilePath))
                {
                    return;
                }

                List<ShapeWrapper> selectedShapes = Shapes.Where(shape => shape.IsSelected && !shape.IsSelectAllShape).ToList();

                using (Database targetDatabase = new Database())
                {
                    foreach (ShapeWrapper shape in selectedShapes)
                    {
                        ExportCorridorSolidsParams exportParams = new ExportCorridorSolidsParams
                        {
                            CreateSolidForShape = true,
                            IncludedCodes = new string[] { shape.Name },
                            ExportShapes = true,
                            ExportLinks = false,
                            SweepSolidForShape = false,
                        };

                        ObjectIdCollection solidIdCollection = SelectedCorridor.Model.ExportSolids(exportParams, targetDatabase);
                    }

                    targetDatabase.SaveAs(solidsFilePath, DwgVersion.Current);
                }

                RaiseCloseRequest();

                if (File.Exists(solidsFilePath))
                {
                    MessageBoxResult messageBoxResult = MessageBox.Show("Open the created solids file?", "Prompt", MessageBoxButton.YesNo);

                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        AutocadDocumentService.DocumentManager.Open(solidsFilePath);
                    }
                }
                else
                {
                    MessageBox.Show("File hasn't been created", "Error");
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show($"Error on extracting solids. {exception.Message}", "Error");
            }
#endif

        }

        private readonly IEventAggregator _eventAggregator;

        private List<CorridorWrapper> corridors = new List<CorridorWrapper>();
        public List<CorridorWrapper> Corridors
        {
            get
            {
                return corridors;
            }
            set
            {
                corridors = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand ExtractCommand { get; }

        private CorridorWrapper selectedCorridor;

        public CorridorWrapper SelectedCorridor
        {
            get
            {
                return selectedCorridor;
            }
            set
            {
                selectedCorridor = value;

                if (selectedCorridor != null)
                {
                    List<string> shapeCodes = selectedCorridor.Model.GetShapeCodes().ToList();

                    Shapes.Clear();
                    Shapes.Add(new ShapeWrapper(_eventAggregator) { Name = Constants.SelectAllName });
                    List<ShapeWrapper> corridorShapes = shapeCodes.Select(shape => new ShapeWrapper(_eventAggregator) { Corridor = selectedCorridor.Model, Name = shape }).ToList();
                    Shapes.AddRange(corridorShapes.ToList());
                }

                RaisePropertyChanged();
            }
        }

        private List<ShapeWrapper> shapes = new List<ShapeWrapper>();
        public List<ShapeWrapper> Shapes
        {
            get
            {
                return shapes;
            }
            set
            {
                shapes = value;
                RaisePropertyChanged();
            }
        }

        private ShapeWrapper selectedShape;

        public ShapeWrapper SelectedShape
        {
            get { return selectedShape; }
            set
            {
                selectedShape = value;

                if (selectedShape != null)
                {
                    selectedShape.IsSelected = !selectedShape.IsSelected;
                }

                RaisePropertyChanged();
            }
        }


        private string selectedShapesInfo;
        public string SelectedShapesInfo
        {
            get { return selectedShapesInfo; }
            set
            {
                selectedShapesInfo = value;
                RaisePropertyChanged();
            }
        }


        protected void RaiseCloseRequest()
        {
            OnRequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OnRequestClose;
    }
}
