using Acad = Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Civil = Autodesk.Civil.DatabaseServices;
using Civil3DAlignmentStationCoordinatesTable.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using System.Windows;

namespace Civil3DAlignmentStationCoordinatesTable.ViewModels
{
    public class MainViewViewModel : BindableBase
    {
        private readonly IEventAggregator eventAggregator;
        private bool isStartStationCheckboxChecked;
        private double startStation;
        private Civil.Alignment selectedAlignment;
        private bool isEndStationCheckboxChecked;
        private double endStation;
        private bool isEndStationTextboxEnabled;
        private bool isStartStationTextboxEnabled;
        private double interval;
        private List<Acad.TableStyle> tableStyles;

        public List<Civil.Alignment> Alignments { get; } = new List<Civil.Alignment>();
        public bool IsStartStationCheckboxChecked
        {
            get
            {
                return isStartStationCheckboxChecked;
            }
            set
            {
                isStartStationCheckboxChecked = value;

                if (isStartStationCheckboxChecked && SelectedAlignment != null)
                {
                    StartStation = SelectedAlignment.StartingStation;
                }

                IsStartStationTextboxEnabled = !isStartStationCheckboxChecked;
                RaisePropertyChanged();
            }
        }

        public bool IsEndStationTextboxEnabled
        {
            get
            {
                return isEndStationTextboxEnabled;
            }
            set
            {
                isEndStationTextboxEnabled = value;
                RaisePropertyChanged();
            }
        }
        public bool IsStartStationTextboxEnabled
        {
            get
            {
                return isStartStationTextboxEnabled;
            }
            set
            {
                isStartStationTextboxEnabled = value;
                RaisePropertyChanged();
            }
        }

        public double StartStation
        {
            get { return startStation; }
            set
            {
                startStation = value;
                RaisePropertyChanged();
            }
        }

        public bool IsEndStationCheckboxChecked
        {
            get
            {
                return isEndStationCheckboxChecked;
            }
            set
            {
                isEndStationCheckboxChecked = value;

                if (isEndStationCheckboxChecked && SelectedAlignment != null)
                {
                    EndStation = SelectedAlignment.EndingStation;
                }

                IsEndStationTextboxEnabled = !isEndStationCheckboxChecked;
                RaisePropertyChanged();
            }
        }
        public double EndStation
        {
            get
            {
                return endStation;
            }
            set
            {
                endStation = value;
                RaisePropertyChanged();
            }
        }

        public double Interval
        {
            get
            {
                return interval;
            }
            set
            {
                interval = value;
                RaisePropertyChanged();
            }
        }

        public Civil.Alignment SelectedAlignment
        {
            get { return selectedAlignment; }
            set
            {
                selectedAlignment = value;

                IsStartStationCheckboxChecked = true;
                StartStation = SelectedAlignment == null ? 0 : SelectedAlignment.StartingStation;

                IsEndStationCheckboxChecked = true;
                EndStation = SelectedAlignment == null ? 0 : SelectedAlignment.EndingStation;

                Interval = 50;

                PointIndexStart = 1;
                PointIndexName = "ASS-";

                StationPrefix = "SS";

                RaisePropertyChanged();
            }
        }

        public Acad.TableStyle SelectedTableStyle { get; set; }
        public List<PointStyle> PointStyles { get; } = new List<PointStyle>();
        public PointStyle SelectedPointStyle
        {
            get
            {
                return selectedPointStyle;
            }
            set
            {
                selectedPointStyle = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand CreateTableCommand { get; }
        public List<Acad.TableStyle> TableStyles
        {
            get
            {
                return tableStyles;
            }
            set
            {
                tableStyles = value;
                RaisePropertyChanged();
            }
        }

        private string pointIndexName;

        public string PointIndexName
        {
            get { return pointIndexName; }
            set { pointIndexName = value; }
        }

        public string StationPrefix
        {
            get
            {
                return stationPrefix;
            }
            set
            {
                stationPrefix = value;
                RaisePropertyChanged();
            }
        }

        private int pointIndexStart;
        private string stationPrefix;

        public int PointIndexStart
        {
            get { return pointIndexStart; }
            set { pointIndexStart = value; }
        }

        public List<LabelStyle> PointLabelStyles { get; } = new List<LabelStyle>();

        private LabelStyle selectedPointLabelStyle;
        private PointStyle selectedPointStyle;
        private bool isPointLeftSide;
        private bool isPointRightSide;

        public LabelStyle SelectedPointLabelStyle
        {
            get
            {
                return selectedPointLabelStyle;
            }
            set
            {
                selectedPointLabelStyle = value;
                RaisePropertyChanged();
            }
        }

        public bool IsPointLeftSide
        {
            get
            {
                return isPointLeftSide;
            }
            set
            {
                isPointLeftSide = value;
                RaisePropertyChanged();
            }
        }

        public bool IsPointRightSide
        {
            get
            {
                return isPointRightSide;
            }
            set
            {
                isPointRightSide = value;
                RaisePropertyChanged();
            }
        }

        public MainViewViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            Alignments = AlignmentUtils.GetAlignments(Acad.OpenMode.ForRead);
            SelectedAlignment = Alignments.FirstOrDefault();
            CreateTableCommand = new DelegateCommand(OnCreateTableCommand);

            TableStyles = TableUtils.GetAllTableStyles(Acad.OpenMode.ForRead);
            SelectedTableStyle = TableStyles.FirstOrDefault(tableStyle => tableStyle.Name.Contains("Alignment Stations"));

            if (SelectedTableStyle == null)
            {
                SelectedTableStyle = TableStyles.FirstOrDefault();
            }

            PointStyles = PointUtils.GetPointStyles(Acad.OpenMode.ForRead);
            SelectedPointStyle = PointStyles.FirstOrDefault(pointStyle => pointStyle.Name == "Station Marked Point");

            if (SelectedPointStyle == null)
            {
                SelectedPointStyle = PointStyles.FirstOrDefault();
            }

            PointLabelStyles = PointUtils.GetPointLabelStyles(Acad.OpenMode.ForRead);
            SelectedPointLabelStyle = PointLabelStyles.FirstOrDefault(labelStyle => labelStyle.Name == "Point Name With Underline [Left]");

            if (SelectedPointLabelStyle == null)
            {
                SelectedPointLabelStyle = PointLabelStyles.FirstOrDefault();
            }

            IsPointLeftSide = true;

            DocumentCollection documentCollection = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            documentCollection.DocumentActivated += DocumentCollectionDocumentActivated;
        }

        private void DocumentCollectionDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            RaiseCloseRequest();
        }

        private void OnCreateTableCommand()
        {
            if (SelectedAlignment == null)
            {
                MessageBox.Show("No such an alignment", "Error");
                return;
            }

            List<double> stations = GetStationList();

            // add header
            int rowsCount = stations.Count + 1;

            Document document = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            Database database = document.Database;

            using (document.LockDocument())
            {
                using (Transaction transaction = database.TransactionManager.StartTransaction())
                {
                    Acad.Table table = TableUtils.CreateTable(SelectedTableStyle, "Select table insertion point", rowHeight: 8, columnWidth: 20, rowsCount, columnCount: 4);
                    if (table == null)
                    {
                        transaction.Abort();
                        return;
                    }

                    table = CreateStationsTable(stations, rowsCount, table, transaction);
                    transaction.Commit();
                }
            }

        }

        private Acad.Table CreateStationsTable(List<double> stations, int rowsCount, Acad.Table table, Transaction transaction)
        {
            table = transaction.GetObject(table.Id, OpenMode.ForWrite, false, true) as Acad.Table;

            TableUtils.SetWidthToTableColumns(table, 0, new int[] { 30, 30, 30, 30 });
            TableUtils.SetHeightToTableRows(table, 0, Enumerable.Repeat(6, rowsCount).ToArray());

            table.TrySetValue(rowIndex: 0, columnIndex: 0, "PUNKT", textHeight: 2.5, CellAlignment.MiddleCenter);
            table.TrySetValue(rowIndex: 0, columnIndex: 1, "STATION", textHeight: 2.5, CellAlignment.MiddleCenter);
            table.TrySetValue(rowIndex: 0, columnIndex: 2, "EASTING", textHeight: 2.5, CellAlignment.MiddleCenter);
            table.TrySetValue(rowIndex: 0, columnIndex: 3, "NORTHING", textHeight: 2.5, CellAlignment.MiddleCenter);

            int rowIndex = 1;

            int pointIndexStart = PointIndexStart;

            foreach (double station in stations)
            {
                double northing = 0;
                double easting = 0;
                GetNorthingEastingCoordinates(station, ref easting, ref northing);

                string pointName = $"{PointIndexName}{pointIndexStart++}";

                pointIndexStart = AddStationDataToTableRow(table, pointName, rowIndex, pointIndexStart, station, northing, easting);

                Civil.CogoPointCollection cogoPoints = CivilApplication.ActiveDocument.CogoPoints;
                ObjectId cogoPointId = cogoPoints.Add(new Point3d(easting, northing, 0), useNextPointNumSetting: true);

                CogoPoint cogoPoint = transaction.GetObject(cogoPointId, OpenMode.ForWrite, false, true) as CogoPoint;
                cogoPoint.RawDescription = SelectedAlignment?.Name;
                cogoPoint.PointName = pointName;
                cogoPoint.LabelStyleId = SelectedPointLabelStyle.Id;
                cogoPoint.StyleId = SelectedPointStyle.Id;

                RotateCogoPoint(station, cogoPoint);

                rowIndex++;
            }

            List<PointGroup> pointGroups = CivilApplication.ActiveDocument.PointGroups.Select(pointGroupId => transaction.GetObject(pointGroupId, OpenMode.ForRead) as PointGroup).Where(group => group != null).ToList();
            PointGroup pointGroup = pointGroups.FirstOrDefault(item => item.Name.Equals(SelectedAlignment?.Name, StringComparison.InvariantCultureIgnoreCase));

            if (pointGroup == null)
            {
                ObjectId pointGroupId = CivilApplication.ActiveDocument.PointGroups.Add(SelectedAlignment?.Name);
                pointGroup = transaction.GetObject(pointGroupId, OpenMode.ForWrite, false, true) as PointGroup;
                StandardPointGroupQuery query = new StandardPointGroupQuery();
                query.IncludeRawDescriptions = SelectedAlignment?.Name;
                pointGroup.SetQuery(query);
            }

            if (SelectedPointStyle != null)
            {
                pointGroup.PointStyleId = SelectedPointStyle.Id;
            }

            if (SelectedPointLabelStyle.Id != null)
            {
                pointGroup.PointLabelStyleId = SelectedPointLabelStyle.Id;
            }

            pointGroup.Update();

            return table;
        }

        private void RotateCogoPoint(double station, CogoPoint cogoPoint)
        {
            if (station >= StartStation && station <= EndStation)
            {
                double northingStart = 0;
                double eastingStart = 0;
                SelectedAlignment.PointLocation(station, 0, ref eastingStart, ref northingStart);
                Point3d pointStart = new Point3d(eastingStart, northingStart, 0);

                double northingEnd = 0;
                double eastingEnd = 0;

                Point3d pointEnd = new Point3d();
                Point3d centerPoint = new Point3d();

                if (station == StartStation)
                {
                    SelectedAlignment.PointLocation(station + 1, 0, ref eastingEnd, ref northingEnd);
                    pointEnd = new Point3d(eastingEnd, northingEnd, 0);
                    centerPoint = new Point3d(pointStart.X + 1, pointStart.Y, 0);
                }
                else
                {
                    SelectedAlignment.PointLocation(station - 1, 0, ref eastingEnd, ref northingEnd);

                    pointEnd = new Point3d(eastingEnd, northingEnd, 0);
                    centerPoint = new Point3d(pointStart.X - 1, pointStart.Y, 0);
                }

                double angle = AngleFrom3PointsInDegrees(centerPoint.X, centerPoint.Y, pointStart.X, pointStart.Y, pointEnd.X, pointEnd.Y);

                if (IsPointLeftSide)
                {
                    angle += Math.PI / 2;
                }
                else
                {
                    angle -= Math.PI / 2;
                }

                cogoPoint.LabelRotation = angle;
            }
        }

        private double AngleFrom3PointsInDegrees(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double a = x2 - x1;
            double b = y2 - y1;
            double c = x3 - x2;
            double d = y3 - y2;

            double atanA = Math.Atan2(a, b);
            double atanB = Math.Atan2(c, d);

            return (atanA - atanB);
        }

        private int AddStationDataToTableRow(Acad.Table table, string pointName, int rowIndex, int pointIndexStart, double station, double northing, double easting)
        {
            table.TrySetValue(rowIndex, columnIndex: 0, pointName, textHeight: 2.5, CellAlignment.MiddleCenter);

            int stationIndex = (int)(station / 1000);
            double stationPart = station - stationIndex * 1000;
            string stationFormatted = $"{StationPrefix} {stationIndex}+{stationPart.ToString("000")}";
            table.TrySetValue(rowIndex, columnIndex: 1, stationFormatted, textHeight: 2.5, CellAlignment.MiddleCenter);

            table.TrySetValue(rowIndex, columnIndex: 2, Math.Round(easting, 3).ToString(), textHeight: 2.5, CellAlignment.MiddleCenter);
            table.TrySetValue(rowIndex, columnIndex: 3, Math.Round(northing, 3).ToString(), textHeight: 2.5, CellAlignment.MiddleCenter);
            return pointIndexStart;
        }

        private void GetNorthingEastingCoordinates(double station, ref double easting, ref double northing)
        {
            try
            {
                SelectedAlignment.PointLocation(station, 0, ref easting, ref northing);
            }
            catch (Exception)
            {
                return;
            }
        }

        private List<double> GetStationList()
        {
            List<double> stations = new List<double>();

            // Get stations by interval

            double currentStation = StartStation;
            stations.Add(currentStation);

            while (currentStation < EndStation)
            {
                currentStation += Interval;

                if (currentStation > EndStation)
                {
                    stations.Add(EndStation);
                }
                else
                {
                    stations.Add(currentStation);
                }
            }

            // Get stations at PI points
            List<Station> piStationSet = SelectedAlignment.GetStationSet(StationTypes.PIPoint).ToList();
            List<double> piStations = piStationSet.Select(station => station.RawStation).ToList();

            // Excude already added stations
            piStations = piStations.Where(piStation => !stations.Contains(piStation)).ToList();

            List<double> allStations = new List<double>();
            allStations.AddRange(stations);
            allStations.AddRange(piStations);

            // Order stations
            allStations = allStations.OrderBy(station => station).ToList();
            return allStations;
        }

        protected void RaiseCloseRequest()
        {
            OnRequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OnRequestClose;
    }
}
