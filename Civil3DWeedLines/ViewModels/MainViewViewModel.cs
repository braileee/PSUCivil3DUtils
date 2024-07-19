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

        public MainViewViewModel(IEventAggregator eventAggregator)
        {
            SelectLinesInfo = "Weed lines";
            SelectLinesCommand = new DelegateCommand(OnSelectLinesCommand);

            LoadDefaultValues();
        }

        private void LoadDefaultValues()
        {
            DistanceValue = 1;
            DeltaAngleValue = 5;
            ToleranceValue = 3;
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
                                    RemoveCommonPoints(segmentsToWeed, transaction);
                                    segmentsToWeed.Clear();
                                    segmentsToWeed.Add(currentSegment);
                                }
                                else if(deltaAngleNext <= DeltaAngleValue && totalLength <= DistanceValue)
                                {
                                    segmentsToWeed.Add(currentSegment);
                                }
                                else
                                {
                                    RemoveCommonPoints(segmentsToWeed, transaction);
                                    segmentsToWeed.Clear();
                                    segmentsToWeed.Add(currentSegment);
                                }

                                // if their distance and angle less than limit value
                                // then add to list



                                /*LineSegmentWrapper currentSegment = lineSegmentWrappers[i];

                                // Add previous segment
                                LineSegmentWrapper previousSegment = lineSegmentWrappers[i - 1];
                                segmentsGroup.Add(previousSegment);

                                // Don't analyze group if there're less than 2 segments
                                if (segmentsGroup.Count < 2)
                                {
                                    continue;
                                }

                                double totalDistance = segmentsGroup.Sum(item => item.LineSegment.Length);

                                // no need to weed segments if the total distance more than limit value

                                if (totalDistance >= DistanceValue)
                                {
                                    segmentsGroup.Clear();
                                    continue;
                                }

                                double currentDistance = segmentsGroup[segmentsGroup.Count - 1].LineSegment.Length + segmentsGroup[segmentsGroup.Count - 2].LineSegment.Length;

                                double angleRadians = segmentsGroup[segmentsGroup.Count - 1].LineSegment.Direction.GetAngleTo(segmentsGroup[segmentsGroup.Count - 2].LineSegment.Direction);
                                double angleDegress = angleRadians.RadiansToDegrees();
                                double deltaAngle = Math.Abs(180 - angleDegress);*/

                                /*LineSegmentWrapper currentSegment = lineSegmentWrappers[i];
                                segmentsGroup.Add(currentSegment);

                                if (i == 0)
                                {
                                    continue;
                                }

                                LineSegmentWrapper previousSegment = lineSegmentWrappers[i - 1];

                                double totalDistance = segmentsGroup.Sum(item => item.LineSegment.Length);
                                double angleRadians = previousSegment.LineSegment.Direction.GetAngleTo(currentSegment.LineSegment.Direction);
                                double angleDegress = angleRadians.RadiansToDegrees();
                                double deltaAngle = Math.Abs(180 - angleDegress);
                                */

                            }

                            if (segmentsToWeed.Count > 1)
                            {
                                RemoveCommonPoints(segmentsToWeed, transaction);
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Error :{exception.Message}");
            }
        }

        private void RemoveCommonPoints(List<LineSegmentWrapper> segmentsToWeed, Transaction transaction)
        {
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
                }
            }
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
