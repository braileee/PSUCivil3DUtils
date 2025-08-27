using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Civil3DDataExport.Models;
using Civil3DDataExport.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Civil3DDataExport.ViewModels
{
    public class MainViewViewModel : ViewModelBase
    {
        public MainViewViewModel(Log log)
        {
            LoadData();

            Step = Properties.Settings.Default.Step;
            StartStation = Properties.Settings.Default.StartStation;
            EndStation = Properties.Settings.Default.EndStation;
            PlanOffset = Properties.Settings.Default.PlanOffset;
            VerticalOffset = Properties.Settings.Default.VerticalOffset;

            ExportCommand = new DelegateCommand(OnExportCommand);
            Log = log;
        }

        private void OnExportCommand(object obj)
        {
            try
            {
                if (SelectedAlignment == null)
                {
                    System.Windows.MessageBox.Show("Please select an alignment.");
                    return;
                }
                if (Profiles == null || Profiles.Count == 0)
                {
                    System.Windows.MessageBox.Show("No profiles found for the selected alignment.");
                    return;
                }
                if (StartStation >= EndStation)
                {
                    System.Windows.MessageBox.Show("Start station must be less than end station.");
                    return;
                }
                if (SelectedProfile == null)
                {
                    System.Windows.MessageBox.Show("Please select a profile.");
                    return;
                }
                if (Step <= 0)
                {
                    System.Windows.MessageBox.Show("Step must be greater than zero.");
                    return;
                }
                if (StartStation < SelectedAlignment.StartingStation || EndStation > SelectedAlignment.EndingStation)
                {
                    System.Windows.MessageBox.Show("Start and end stations must be within the alignment range.");
                    return;
                }

                Log.Information("Exporting data started");
                Log.Information($"Alignment: {SelectedAlignment.Name}");
                Log.Information($"Profile: {SelectedProfile.Name}");
                Log.Information($"Start Station: {StartStation}");
                Log.Information($"End Station: {EndStation}");
                Log.Information($"Step: {Step}");
                Log.Information($"Plan Offset: {PlanOffset}");
                Log.Information($"Vertical Offset: {VerticalOffset}");

                List<AlignmentPoint> alignmentPoints = AlignmentPoint.Create(SelectedAlignment, SelectedProfile, StartStation, EndStation, Step, PlanOffset, VerticalOffset, Log);

                if (alignmentPoints.Count == 0)
                {
                    System.Windows.MessageBox.Show("No points generated. Please check your parameters.");
                    Log.Error("No points generated. Export aborted.");
                    return;
                }

                string filePath = DialogUtils.SaveFileToFolder(Environment.SpecialFolder.Desktop, Constants.JsonExtension);

                if (string.IsNullOrEmpty(filePath))
                {
                    System.Windows.MessageBox.Show("File path is not valid.");
                    Log.Error("File path is not valid. Export aborted.");
                    return;
                }

                StationData stationData = new StationData
                {
                    FilePath = App.ActiveDocumentAutocad.Database.Filename,
                    AlignmentName = SelectedAlignment?.Name,
                    ProfileName = SelectedProfile?.Name,
                    Step = Step,
                    HorizontalOffset = PlanOffset,
                    VerticalOffset = VerticalOffset,
                    StartStation = StartStation,
                    EndStation = EndStation,
                    StationItems = alignmentPoints.Select(p => new StationItem
                    {
                        X = p.Point.X,
                        Y = p.Point.Y,
                        Z = p.Point.Z,
                        Station = p.Station
                    }).ToList()
                };

                Log.Information($"Export data to json file {filePath}");
                string stationDataJson = JsonConvert.SerializeObject(stationData, Formatting.Indented);

                File.WriteAllText(filePath, stationDataJson);

                MessageBox.Show($"Data exported successfully to {filePath}", "Export Successful", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                Log.Information("Exporting data completed successfully");
            }
            catch (Exception exception)
            {
                MessageBox.Show($"An error occurred during export: {exception.Message}", "Export Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private double planOffset;

        public double PlanOffset
        {
            get { return planOffset; }
            set
            {
                planOffset = value;
                Properties.Settings.Default.PlanOffset = value;
                Properties.Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        private double verticalOffset;

        public double VerticalOffset
        {
            get { return verticalOffset; }
            set
            {
                verticalOffset = value;
                Properties.Settings.Default.VerticalOffset = value;
                Properties.Settings.Default.Save();
                RaisePropertyChanged();
            }
        }


        private Profile selectedProfile;

        public Profile SelectedProfile
        {
            get { return selectedProfile; }
            set
            {
                selectedProfile = value;
                RaisePropertyChanged();
            }
        }


        private double step;
        public double Step
        {
            get { return step; }
            set
            {
                step = value;

                Properties.Settings.Default.Step = value;
                Properties.Settings.Default.Save();

                RaisePropertyChanged();
            }
        }

        public DelegateCommand ExportCommand { get; }

        private double startStation;
        public double StartStation
        {
            get { return startStation; }
            set
            {
                startStation = value;
                Properties.Settings.Default.StartStation = value;
                Properties.Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        private double endStation;
        public double EndStation
        {
            get { return endStation; }
            set
            {
                endStation = value;
                Properties.Settings.Default.EndStation = value;
                Properties.Settings.Default.Save();
                RaisePropertyChanged();
            }
        }



        public List<Alignment> Alignments { get; private set; } = new List<Alignment>();

        private List<Profile> profiles;
        public List<Profile> Profiles
        {
            get { return profiles; }
            set
            {
                profiles = value;
                RaisePropertyChanged();
            }
        }

        private Alignment selectedAlignment;
        public Alignment SelectedAlignment
        {
            get { return selectedAlignment; }
            set
            {
                selectedAlignment = value;

                if (selectedAlignment != null)
                {
                    using (Transaction transaction = App.TransactionManager.StartTransaction())
                    {
                        Profiles = AlignmentUtils.GetProfilesOfAlignment(selectedAlignment, transaction, OpenMode.ForRead);
                        transaction.Commit();
                    }
                }

                RaisePropertyChanged();
            }
        }

        public Log Log { get; }

        private void LoadData()
        {
            using (Transaction transaction = App.TransactionManager.StartTransaction())
            {
                Alignments = AlignmentUtils.GetAlignments(OpenMode.ForRead, transaction);

                transaction.Commit();
            }
        }
    }
}
