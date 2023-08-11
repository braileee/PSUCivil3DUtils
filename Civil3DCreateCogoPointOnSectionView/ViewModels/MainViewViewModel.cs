using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using Civil3DCreateCogoPointOnSectionView.Properties;
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

namespace Civil3DCreateCogoPointOnSectionView.ViewModels
{
    public class MainViewViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        public DelegateCommand SelectSectionView { get; }
        public DelegateCommand SelectPointOnSectionView { get; }
        public SectionView SectionView { get; private set; }

        private string pointDescription;
        public string PointDescription
        {
            get { return pointDescription; }
            set
            {
                pointDescription = value;
                Settings.Default.PointDescription = pointDescription;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        private string selectedSectionViewInfo;

        public string SelectedSectionViewInfo
        {
            get { return selectedSectionViewInfo; }
            set
            {
                selectedSectionViewInfo = value;
                RaisePropertyChanged();
            }
        }


        public Point3d? SectionViewPoint { get; private set; }

        public MainViewViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            DocumentCollection documentCollection = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            documentCollection.DocumentActivated += DocumentCollectionDocumentActivated;

            SelectSectionView = new DelegateCommand(OnSelectSectionView);
            SelectPointOnSectionView = new DelegateCommand(OnSelectPointOnSectionView);

            PointDescription = Settings.Default.PointDescription == null ? string.Empty : Settings.Default.PointDescription;
        }

        private void OnSelectPointOnSectionView()
        {
            if (SectionView == null)
            {
                MessageBox.Show("Section view hasn't been selected");
                return;
            }

            SectionViewPoint = PointUtils.PromptPoint("Pick a point");

            if (!SectionViewPoint.HasValue)
            {
                MessageBox.Show("No picked point");
                return;
            }

            double offset = 0;
            double elevation = 0;
            SectionView.FindOffsetAndElevationAtXY(SectionViewPoint.Value.X, SectionViewPoint.Value.Y, ref offset, ref elevation);

            List<Alignment> alignments = AlignmentUtils.GetAlignments(OpenMode.ForRead);

            Alignment alignment = null;

            bool alignmentDetected = false;
            using (AutocadDocumentService.LockActiveDocument())
            {
                using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    SampleLine sampleLine = transaction.GetObject(SectionView.ParentEntityId, OpenMode.ForRead, false, true) as SampleLine;

                    if (sampleLine == null)
                    {
                        MessageBox.Show("No sample line found for the selected section view");
                        return;
                    }

                    foreach (Alignment currentAlignment in alignments)
                    {
                        foreach (ObjectId groupId in currentAlignment.GetSampleLineGroupIds())
                        {
                            if (sampleLine.GroupId == groupId)
                            {
                                SampleLineGroup sampleLineGroup = transaction.GetObject(groupId, OpenMode.ForRead, false, true) as SampleLineGroup;
                                alignment = currentAlignment;

                                alignmentDetected = true;

                                break;
                            }
                        }

                        if (alignmentDetected)
                        {
                            break;
                        }
                    }

                    if (alignment == null)
                    {
                        MessageBox.Show("Alignment can't be detected to create a cogo point");
                        return;
                    }

                    double easting = 0;
                    double northing = 0;

                    double station = sampleLine.Station;

                    try
                    {
                        alignment.PointLocation(station, offset, ref easting, ref northing);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Point can't be created");
                        return;
                    }

                    CogoPoint cogoPoint = CogoPointUtils.CreateCogoPoint(new Point3d(easting, northing, elevation));

                    if (!cogoPoint.IsWriteEnabled)
                    {
                        cogoPoint.UpgradeOpen();
                    }

                    if (!string.IsNullOrEmpty(PointDescription))
                    {
                        cogoPoint.RawDescription = PointDescription;
                    }

                    transaction.Commit();
                }
            }
        }

        private void OnSelectSectionView()
        {
            SectionView = SectionViewUtils.PromptASectionView("Select a section view", OpenMode.ForRead);

            if (SectionView != null)
            {
                SelectedSectionViewInfo = $"Selected: {SectionView.Name}";
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
