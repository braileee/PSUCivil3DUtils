using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.Aec.Modeler;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using Civil3DWeedLines.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Civil3DWeedLines.ViewModels
{
    public class MainViewViewModel : BindableBase
    {
        private string selectLinesInfo;
        public string SelectLinesInfo
        {
            get
            {
                return selectLinesInfo;
            }
            set
            {
                selectLinesInfo = value;
                RaisePropertyChanged();
            }
        }

        private double deltaAngleValue;
        public double DeltaAngleValue
        {
            get { return deltaAngleValue; }
            set
            {
                deltaAngleValue = value;
                RaisePropertyChanged();
            }
        }

        public int ToleranceValue
        {
            get
            {
                return toleranceValue;
            }
            set
            {
                toleranceValue = value;
                RaisePropertyChanged();
            }
        }

        private double distanceValue;
        private int toleranceValue;

        public double DistanceValue
        {
            get { return distanceValue; }
            set
            {
                distanceValue = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand SelectLinesCommand { get; }

        private bool byDistanceBetweenSegments;

        public bool ByDistanceBetweenSegments
        {
            get { return byDistanceBetweenSegments; }
            set
            {
                byDistanceBetweenSegments = value;
                RaisePropertyChanged();
            }
        }

        private bool byShortestSegment;

        public bool ByShortestSegment
        {
            get { return byShortestSegment; }
            set
            {
                byShortestSegment = value;
                RaisePropertyChanged();
            }
        }


        public MainViewViewModel(IEventAggregator eventAggregator)
        {
            SelectLinesInfo = "Weed lines";
            SelectLinesCommand = new DelegateCommand(OnSelectLinesCommand);
            ByShortestSegment = true;

            LoadDefaultValues();
        }

        private void LoadDefaultValues()
        {
            double defaultDistanceValue = NumbersUtils.ParseStringToDouble(Properties.Settings.Default.DistanceValue);
            double defaultAngleValue = NumbersUtils.ParseStringToDouble(Properties.Settings.Default.DeltaAngleValue);
            int defaultToleranceValue = NumbersUtils.ParseStringToInt(Properties.Settings.Default.ToleranceValue);

            DistanceValue = defaultDistanceValue > 0 ? defaultDistanceValue : 1;
            DeltaAngleValue = defaultAngleValue > 0 ? defaultAngleValue : 0.5;
            ToleranceValue = defaultToleranceValue > 0 ? defaultToleranceValue : 3;
        }

        private void OnSelectLinesCommand()
        {
            try
            {
                if (DistanceValue <= 0 || DeltaAngleValue <= 0 || ToleranceValue <= 0)
                {
                    MessageBox.Show("Weeding values can't be less or equal to zero", "Error");
                    return;
                }

                List<Polyline3d> polylines = SelectionUtils.GetElements<Polyline3d>("Select 3D Polylines");

                if (polylines.Count == 0)
                {
                    MessageBox.Show("3D polylines have not been selected, operation canceled");
                    return;
                }

                // Weed duplicate point within tolerance

                using (AutocadDocumentService.LockActiveDocument())
                {
                    using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                    {
                        foreach (Polyline3d polyline in polylines)
                        {
                            Polyline3d polylineOpened = polyline;

                            List<PolylineVertex3d> vertexes = polylineOpened.GetVertexes();

                            var duplicates = vertexes.GroupBy(t => new { t.Position.X, t.Position.Y, t.Position.Z })
                                 .Where(t => t.Count() > 1)
                                 .Select(g => g.Key).ToList();

                            List<PolylineVertex3d> duplicateVertexes = vertexes.Where(vertex => duplicates.Any(duplicate =>
                                                                                    Math.Round(duplicate.X, ToleranceValue) == Math.Round(vertex.Position.X, ToleranceValue) &&
                                                                                    Math.Round(duplicate.Y, ToleranceValue) == Math.Round(vertex.Position.Y, ToleranceValue) &&
                                                                                    Math.Round(duplicate.Z, ToleranceValue) == Math.Round(vertex.Position.Z, ToleranceValue))).Skip(1).ToList();

                            foreach (PolylineVertex3d vertex in duplicateVertexes)
                            {
                                PolylineVertex3d vertexOpened = vertex;

                                if (!vertex.IsWriteEnabled)
                                {
                                    vertexOpened = transaction.GetObject(vertexOpened.Id, OpenMode.ForWrite, false, true) as PolylineVertex3d;
                                }

                                if (!polylineOpened.IsWriteEnabled)
                                {
                                    polylineOpened = transaction.GetObject(polylineOpened.ObjectId, OpenMode.ForWrite, false, true) as Polyline3d;
                                }

                                if (!vertexOpened.IsErased)
                                {
                                    vertexOpened.Erase(true);
                                }
                            }
                        }

                        transaction.Commit();
                    }

                    int allDeletedPointsCount = WeedByShortestSegment(polylines);

                    MessageBox.Show($"Deleted points: {allDeletedPointsCount}");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Error :{exception.Message}");
            }
        }

        private int WeedByDistanceBetweenSegments(List<Polyline3d> polylines)
        {
            int allDeletedPointsCount = 0;

            using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
            {
                foreach (Polyline3d polyline in polylines)
                {
                    List<LineSegmentWrapper> lineSegmentWrappers = LineSegmentWrapper.Create(polyline);

                    List<LineSegmentWrapper> segmentsToWeed = new List<LineSegmentWrapper>();

                    for (int i = 0; i < lineSegmentWrappers.Count; i++)
                    {
                        LineSegmentWrapper currentSegment = lineSegmentWrappers[i];

                        LineSegmentWrapper lastSegmentToWeed = segmentsToWeed.LastOrDefault();

                        double deltaAngle = double.PositiveInfinity;

                        // Add first elements if its distance do not exceed the limit
                        if (i == 0 || segmentsToWeed.Count == 0)
                        {
                            if (currentSegment.LineSegment.Length <= DistanceValue)
                            {
                                segmentsToWeed.Add(currentSegment);
                            }

                            continue;
                        }
                        else if (segmentsToWeed.Count == 1)
                        {
                            // Add the second segment if total distance and angle between do not exceed the limit

                            deltaAngle = GetDeltaAngle(currentSegment, lastSegmentToWeed);

                            double twoLastSegmentsLength = lastSegmentToWeed.LineSegment.Length + currentSegment.LineSegment.Length;

                            if (twoLastSegmentsLength <= DistanceValue && deltaAngle <= DeltaAngleValue)
                            {
                                segmentsToWeed.Add(currentSegment);
                            }
                            else
                            {
                                segmentsToWeed.Clear();
                                segmentsToWeed.Add(currentSegment);
                            }

                            continue;
                        }

                        if (segmentsToWeed.Count < 2)
                        {
                            continue;
                        }

                        double deltaAngleNext = GetDeltaAngle(currentSegment, lastSegmentToWeed);

                        // Check total length of segments plus the current one
                        double totalLength = segmentsToWeed.Sum(item => item.LineSegment.Length) + currentSegment.LineSegment.Length;

                        if (deltaAngleNext > DeltaAngleValue)
                        {
                            int deletedPointsCount = RemoveCommonPoints(segmentsToWeed, transaction);
                            allDeletedPointsCount += deletedPointsCount;

                            segmentsToWeed.Clear();
                            segmentsToWeed.Add(currentSegment);
                        }
                        else if (deltaAngleNext <= DeltaAngleValue && totalLength <= DistanceValue)
                        {
                            segmentsToWeed.Add(currentSegment);
                        }
                        else
                        {
                            int deletedPointsCount = RemoveCommonPoints(segmentsToWeed, transaction);
                            allDeletedPointsCount += deletedPointsCount;

                            segmentsToWeed.Clear();
                            segmentsToWeed.Add(currentSegment);
                        }
                    }

                    if (segmentsToWeed.Count > 1)
                    {
                        int deletedPointsCount = RemoveCommonPoints(segmentsToWeed, transaction);
                        allDeletedPointsCount += deletedPointsCount;
                    }
                }
                transaction.Commit();
            }

            return allDeletedPointsCount;
        }

        private int WeedByShortestSegment(List<Polyline3d> polylines)
        {
            int allDeletedPointsCount = 0;

            using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
            {
                foreach (Polyline3d polyline in polylines)
                {
                    List<LineSegmentWrapper> lineSegmentWrappers = LineSegmentWrapper.Create(polyline);

                    if (lineSegmentWrappers.Count < 2)
                    {
                        continue;
                    }

                    List<LineSegmentWrapper> segmentsToWeed = new List<LineSegmentWrapper>();

                    for (int i = 0; i < lineSegmentWrappers.Count; i++)
                    {
                        LineSegmentWrapper currentSegment = lineSegmentWrappers[i];

                        if (currentSegment.LineSegment.Length >= DistanceValue)
                        {
                            continue;
                        }

                        bool weedByStartPoint = false;
                        bool weedByEndPoint = false;

                        if (i > 0)
                        {
                            LineSegmentWrapper previousSegment = lineSegmentWrappers.ElementAtOrDefault(i - 1);

                            if (previousSegment != null)
                            {
                                double angle = GetDeltaAngle(currentSegment, previousSegment);

                                if (angle < DeltaAngleValue)
                                {
                                    weedByStartPoint = true;
                                }
                            }

                            LineSegmentWrapper nextSegment = lineSegmentWrappers.ElementAtOrDefault(i + 1);

                            if (nextSegment != null)
                            {
                                double angle = GetDeltaAngle(currentSegment, previousSegment);

                                if (angle < DeltaAngleValue)
                                {
                                    weedByEndPoint = true;
                                }
                            }

                            int deletedPointsCount = 0;
                            if (weedByStartPoint && weedByEndPoint)
                            {
                                deletedPointsCount = RemoveCommonPoints(new List<LineSegmentWrapper> { currentSegment, previousSegment, nextSegment }, transaction);
                            }
                            else if (weedByStartPoint)
                            {
                                deletedPointsCount = RemoveCommonPoints(new List<LineSegmentWrapper> { currentSegment, previousSegment }, transaction);
                            }
                            else if (weedByEndPoint)
                            {
                                deletedPointsCount = RemoveCommonPoints(new List<LineSegmentWrapper> { currentSegment, nextSegment }, transaction);
                            }

                            allDeletedPointsCount += deletedPointsCount;
                        }
                    }
                }
                transaction.Commit();
            }

            return allDeletedPointsCount;
        }

        private int RemoveCommonPoints(List<LineSegmentWrapper> segmentsToWeed, Transaction transaction)
        {
            int deletedPointsCount = 0;

            for (int i = 0; i < segmentsToWeed.Count; i++)
            {
                if (i == 0)
                {
                    continue;
                }

                LineSegmentWrapper previousSegment = segmentsToWeed[i - 1];
                LineSegmentWrapper currentSegment = segmentsToWeed[i];

                PolylineVertex3d vertex = previousSegment.GetCommonVertex(currentSegment);

                if (vertex == null)
                {
                    continue;
                }

                if (vertex.IsErased)
                {
                    continue;
                }

                if (!vertex.IsWriteEnabled)
                {
                    vertex = transaction.GetObject(vertex.ObjectId, OpenMode.ForWrite, false, true) as PolylineVertex3d;
                }

                if (!vertex.IsErased)
                {
                    vertex.Erase(true);
                    deletedPointsCount++;
                }
            }

            return deletedPointsCount;
        }

        private static double GetDeltaAngle(LineSegmentWrapper currentSegment, LineSegmentWrapper lastSegment)
        {
            double angleRadians = lastSegment.LineSegment.Direction.GetAngleTo(currentSegment.LineSegment.Direction, currentSegment.LineSegment.Direction);
            double angleDegrees = angleRadians.RadiansToDegrees();

            if (Math.Abs(angleDegrees) > 180)
            {
                angleDegrees = Math.Abs(360 - angleDegrees);
            }

            return angleDegrees;
        }

        protected void RaiseCloseRequest()
        {
            OnRequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OnRequestClose;
    }
}
