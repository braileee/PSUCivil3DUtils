using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using Civil3DExtractCorridorLinksToSurfaces.Events;
using Civil3DExtractCorridorLinksToSurfaces.Models;
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
using Acad = Autodesk.AutoCAD.DatabaseServices;
using Civil = Autodesk.Civil.DatabaseServices;

namespace Civil3DExtractCorridorLinksToSurfaces.ViewModels
{
    public class MainViewViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private bool isStartStationCheckboxChecked;
        private double startStation;
        private Civil.Alignment selectedAlignment;
        private bool isEndStationCheckboxChecked;
        private double endStation;
        private bool isEndStationTextboxEnabled;
        private bool isStartStationTextboxEnabled;
        private double interval;
        private List<Acad.TableStyle> tableStyles;
        private List<Corridor> corridors = new List<Corridor>();

        public List<Corridor> Corridors
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

        public DelegateCommand SelectCorridorFromModel { get; }
        public DelegateCommand CreateSurfacesCommand { get; }
        private ObservableCollection<LinkCodeWrapper> linkCodeWrappers = new ObservableCollection<LinkCodeWrapper>();
        public ObservableCollection<LinkCodeWrapper> LinkCodeWrappers
        {
            get => linkCodeWrappers; set
            {
                linkCodeWrappers = value;
                RaisePropertyChanged();
            }
        }
        private Corridor selectedCorridor;
        private ObservableCollection<CorridorSurfaceWrapper> corridorSurfaceWrappers = new ObservableCollection<CorridorSurfaceWrapper>();

        public Corridor SelectedCorridor
        {
            get { return selectedCorridor; }
            set
            {
                selectedCorridor = value;

                LinkCodeWrappers.Clear();
                LinkCodeWrappers.Add(new LinkCodeWrapper(_eventAggregator) { Name = "<Select All>", IsSelected = true });
                LinkCodeWrappers.AddRange(SelectedCorridor.GetLinkCodes().Select(linkCode => new LinkCodeWrapper(_eventAggregator)
                {
                    IsSelected = true,
                    Name = linkCode
                }).ToList());

                UpdateCorridorSurfaceWrappers();

                RaisePropertyChanged();
            }
        }

        private void UpdateCorridorSurfaceWrappers()
        {
            CorridorSurfaceWrappers.Clear();
            CorridorSurfaceWrappers.Add(new CorridorSurfaceWrapper(_eventAggregator)
            {
                Corridor = SelectedCorridor,
                Name = "<Select All>",
                IsSelected = true,
            });
            CorridorSurfaceWrappers.AddRange(SelectedCorridor.CorridorSurfaces.Select(surface => new CorridorSurfaceWrapper(_eventAggregator)
            {
                Corridor = SelectedCorridor,
                IsSelected = true,
                Name = surface.Name,
                Surface = surface,
            }));
        }

        public ObservableCollection<CorridorSurfaceWrapper> CorridorSurfaceWrappers
        {
            get
            {
                return corridorSurfaceWrappers;
            }
            set
            {
                corridorSurfaceWrappers = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand DeleteSelectedCommand { get; }
        public List<SurfaceStyle> SurfaceStyles { get; private set; } = new List<SurfaceStyle>();

        private SurfaceStyle selectedSurfaceStyle;
        private bool doCreateAutomaticBoundary;

        public SurfaceStyle SelectedSurfaceStyle
        {
            get { return selectedSurfaceStyle; }
            set
            {
                selectedSurfaceStyle = value;
                RaisePropertyChanged();
            }
        }

        public List<OverhangCorrectionWrapper> OverhangCorrectionList { get; private set; } = new List<OverhangCorrectionWrapper>();
        public OverhangCorrectionWrapper SelectedOverhangCorrection { get; private set; } = new OverhangCorrectionWrapper();
        public bool DoCreateAutomaticBoundary
        {
            get
            {
                return doCreateAutomaticBoundary;
            }
            set
            {
                doCreateAutomaticBoundary = value;
                RaisePropertyChanged();
            }
        }

        public MainViewViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            DocumentCollection documentCollection = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            documentCollection.DocumentActivated += DocumentCollectionDocumentActivated;

            SelectCorridorFromModel = new DelegateCommand(OnSelectCorridorFromModel);
            CreateSurfacesCommand = new DelegateCommand(OnCreateSurfacesCommand);

            LinkCodeWrappers = new ObservableCollection<LinkCodeWrapper>();

            _eventAggregator.GetEvent<SelectAllLinksEvent>().Subscribe(OnSelectAllLinksEvent);
            _eventAggregator.GetEvent<SelectAllSurfacesEvent>().Subscribe(OnSelectAllSurfacesEvent);

            DeleteSelectedCommand = new DelegateCommand(OnDeleteSelectedCommand);

            SurfaceStyles = SurfaceUtils.GetSurfaceStyles(OpenMode.ForRead);
            SelectedSurfaceStyle = SurfaceStyles.FirstOrDefault();

            OverhangCorrectionList = OverhangCorrectionWrapper.Get();
            SelectedOverhangCorrection = OverhangCorrectionList.FirstOrDefault();

            DoCreateAutomaticBoundary = true;

            Corridors = CorridorUtils.GetAllTheCorridors(OpenMode.ForRead);
            SelectedCorridor = Corridors.FirstOrDefault();
        }

        private void OnDeleteSelectedCommand()
        {
            using (AutocadDocumentService.LockActiveDocument())
            {

                using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    foreach (CorridorSurfaceWrapper corridorSurfaceWrapper in CorridorSurfaceWrappers.Where(corridorSurface => corridorSurface.IsSelected))
                    {
                        CorridorSurface surface = corridorSurfaceWrapper.Surface;
                        Corridor corridor = corridorSurfaceWrapper.Corridor;

                        if (!corridor.IsWriteEnabled)
                        {
                            corridor = transaction.GetObject(corridor.Id, OpenMode.ForWrite, false, true) as Corridor;
                        }

                        if (surface == null)
                        {
                            continue;
                        }

                        corridor.CorridorSurfaces.Remove(surface);
                    }

                    UpdateCorridorSurfaceWrappers();
                    transaction.Commit();
                }
            }
        }

        private void OnSelectAllSurfacesEvent(bool selectAll)
        {
            foreach (CorridorSurfaceWrapper corridorSurfaceWrapper in CorridorSurfaceWrappers)
            {
                if (corridorSurfaceWrapper.Name == "<Select All>")
                {
                    continue;
                }

                corridorSurfaceWrapper.IsSelected = selectAll;
            }
        }

        private void OnSelectAllLinksEvent(bool selectAll)
        {
            foreach (LinkCodeWrapper linkCodeWrapper in LinkCodeWrappers)
            {
                if (linkCodeWrapper.Name == "<Select All>")
                {
                    continue;
                }

                linkCodeWrapper.IsSelected = selectAll;
            }
        }

        private void OnCreateSurfacesCommand()
        {
            try
            {
                if (SelectedCorridor == null)
                {
                    MessageBox.Show("No selected corridor");
                    return;
                }

                if (SelectedSurfaceStyle == null)
                {
                    MessageBox.Show("No selected surface style");
                    return;
                }

                List<CorridorSurface> corridorSurfaces = new List<CorridorSurface>();
                Corridor corridor = SelectedCorridor;

                using (AutocadDocumentService.LockActiveDocument())
                {
                    using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                    {
                        if (!corridor.IsWriteEnabled)
                        {
                            corridor = transaction.GetObject(SelectedCorridor.Id, OpenMode.ForWrite, false, true) as Corridor;
                        }

                        foreach (LinkCodeWrapper linkCodeWrapper in LinkCodeWrappers.Where(item => item.IsSelected))
                        {
                            if (linkCodeWrapper.Name == "<Select All>")
                            {
                                continue;
                            }

                            string corridorSurfaceName = $"{corridor.Name} - {linkCodeWrapper.Name} - {Guid.NewGuid()}";

                            CorridorSurface corridorSurface = corridor.CorridorSurfaces.Add(corridorSurfaceName, SelectedSurfaceStyle.Id);
                            corridorSurface.AddLinkCode(linkCodeWrapper.Name, addAsBreakLine: false);
                            corridorSurface.OverhangCorrection = SelectedOverhangCorrection.OverhangCorrectionType;
                            corridorSurface.Boundaries.AddCorridorExtentsBoundary($"Boundary - {corridorSurfaceName}");
                            corridorSurfaces.Add(corridorSurface);
                        }

                        corridor.Rebuild();
                        transaction.Commit();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error");
            }

            RaiseCloseRequest();
        }

        private void OnSelectCorridorFromModel()
        {
            Corridor corridor = CorridorUtils.PromptCorridor(OpenMode.ForRead);
            SelectedCorridor = Corridors.FirstOrDefault(item => item.Id == corridor.Id);
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
