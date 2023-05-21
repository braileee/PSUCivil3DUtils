using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
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
using Civil3DUtils;
using Civil3DSubassemblyAnnotations.Models;
using AutoCADUtils.Utils;
using Autodesk.Civil;
using Civil3DSubassemblyAnnotations.Events;

namespace Civil3DSubassemblyAnnotations.ViewModels
{
    public class MainViewViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        public DelegateCommand SelectCorridorCommand { get; }
        public DelegateCommand ApplyCommand { get; }
        public Corridor Corridor { get; private set; }

        public MainViewViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            DocumentCollection documentCollection = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            documentCollection.DocumentActivated += DocumentCollectionDocumentActivated;

            SelectCorridorCommand = new DelegateCommand(OnSelectCorridorCommand);
            ApplyCommand = new DelegateCommand(OnApplyCommand);

            eventAggregator.GetEvent<CorridorCodeUpdateEvent>().Subscribe(OnCorridorCodeUpdate);
        }

        private void OnCorridorCodeUpdate(CorridorCode corridorCode)
        {
            if (corridorCode == null || CorridorCodes == null)
            {
                return;
            }

            SelectedCorridorCodesInfo = string.Join(", ", CorridorCodes.Where(code => code.IsSelected).Select(code => code.Code).OrderBy(code => code).ToArray());
        }

        private void OnApplyCommand()
        {
            if (Corridor == null)
            {
                return;
            }

            List<Baseline> baselines = Corridor.GetBaselines();
            Baseline baseline = baselines.FirstOrDefault();

            List<AppliedAssembly> appliedAssemblies = baseline.GetAppliedAssemblies();

            List<LinkGroup> linkGroups = new List<LinkGroup>();
            int accuracy = 3;

            List<CorridorCode> selectedCorridorCodes = CorridorCodes.Where(c => c.IsSelected).ToList();

            foreach (AppliedAssembly appliedAssembly in appliedAssemblies)
            {
                List<CalculatedLink> currentLinks = appliedAssembly.Links.Where(link => selectedCorridorCodes.Any(selectedCode => link.CorridorCodes.Any(code => code == selectedCode.Code))).ToList();

                foreach (CalculatedLink link in currentLinks)
                {
                    List<CalculatedPoint> calculatedPoints = link.CalculatedPoints.ToList();

                    if (calculatedPoints.Count < 2)
                    {
                        continue;
                    }

                    CalculatedPoint point1 = calculatedPoints[0];
                    CalculatedPoint point2 = calculatedPoints[1];

                    double station = Math.Round(appliedAssembly.GetAppliedSubassemblies().First().OriginStationOffsetElevationToBaseline.X, accuracy);

                    linkGroups.Add(new LinkGroup
                    {
                        AppliedAssembly = appliedAssembly,
                        Station = station,
                        Link = link,
                        Point1 = point1,
                        Point2 = point2
                    });
                }
            }

            Alignment alignment = baseline.GetAlignment(OpenMode.ForRead);
            List<SampleLineGroup> groups = alignment.GetSampleLineGroups(OpenMode.ForRead);
            SampleLineGroup group = groups.FirstOrDefault();

            List<SampleLine> sampleLines = group.GetSampleLines(OpenMode.ForRead);


            foreach (SampleLine sampleLine in sampleLines)
            {
                List<SectionView> sectionViews = sampleLine.GetSectionViews(OpenMode.ForRead);
                List<LinkGroup> linkgGroupsOfStation = linkGroups.Where(item => item.Station == Math.Round(sampleLine.Station, accuracy)).ToList();

                foreach (LinkGroup linkGroup in linkgGroupsOfStation)
                {
                    CalculatedPoint calcPoint1 = linkGroup.Point1;
                    CalculatedPoint calcPoint2 = linkGroup.Point2;

                    foreach (SectionView sectionView in sectionViews)
                    {
                        Point3d point1 = GetPoint(calcPoint1, sectionView);
                        Point3d point2 = GetPoint(calcPoint2, sectionView);

                        DimensionUtils.CreateLinearDimension(point1, point2, dimLinkOffsetY: 1);
                    }
                }
            }

            RaiseCloseRequest();
        }

        private void OnSelectCorridorCommand()
        {
            Corridor = CorridorUtils.PromptCorridor(OpenMode.ForRead);

            if (Corridor == null)
            {
                return;
            }

            SelectedCorridorCodesInfo = "Select Links";

            CorridorCodes = new ObservableCollection<CorridorCode>(
                Corridor.GetLinkCodes().OrderBy(code => code).Select(linkCode => new CorridorCode(_eventAggregator)
                {
                    Code = linkCode,
                    IsSelected = false
                }).ToList());
        }

        private static Point3d GetPoint(CalculatedPoint calcPoint, SectionView sectionView)
        {
            Point3d stationOffsetElevationToBaseline1 = calcPoint.StationOffsetElevationToBaseline;

            double y1 = 0;
            double x1 = 0;

            sectionView.FindXYAtOffsetAndElevation(stationOffsetElevationToBaseline1.Y, stationOffsetElevationToBaseline1.Z, ref x1, ref y1);
            Point3d point1 = new Point3d(x1, y1, 0);
            return point1;
        }

        private ObservableCollection<CorridorCode> corridorCodes;

        public ObservableCollection<CorridorCode> CorridorCodes
        {
            get { return corridorCodes; }
            set
            {
                corridorCodes = value;
                RaisePropertyChanged();
            }
        }

        private string selectedCorridorCodesInfo;

        public string SelectedCorridorCodesInfo
        {
            get { return selectedCorridorCodesInfo; }
            set
            {
                selectedCorridorCodesInfo = value;
                RaisePropertyChanged();
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
