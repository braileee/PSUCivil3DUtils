using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DatabaseServices;
using Civil3DLineIntersectionCheck.Enums;
using Civil3DUtils;
using Civil3DUtils.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Civil3DLineIntersectionCheck
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DLineIntersectionCheck", CommandFlags.Modal)]
        public static void Civil3DLineIntersectionCheck()
        {
            try
            {
                int tolerance = 8;

                List<Autodesk.AutoCAD.DatabaseServices.DBObject> dbObjects = SelectionUtils.GetDbObjects("Select polylines, 3d polylines, feature lines:", OpenMode.ForRead);

                if (!dbObjects.Any())
                {
                    return;
                }

                string intersectionDimensionTypeValue = PromptUtils.PromptKeyword("Check intersection by 2D or 3D?", allowNone: false, allowArbitraryInput: false, defaultValue: Constants.TwoDimensionalKeyword, Constants.TwoDimensionalKeyword, Constants.ThreeDimensionalKeyword);

                if (string.IsNullOrEmpty(intersectionDimensionTypeValue))
                {
                    MessageBox.Show("Incorrect input, operation has been canceled");
                    return;
                }

                IntersectionDimensionType intersectionDimensionType = intersectionDimensionTypeValue == Constants.ThreeDimensionalKeyword ? IntersectionDimensionType.ThreeDimensional : IntersectionDimensionType.TwoDimensional;

                string intersectionCheckValue = PromptUtils.PromptKeyword("Check intersection between lines or perform a self-check?", allowNone: false, allowArbitraryInput: false, defaultValue: Constants.IntersectionBetweenLinesCheck, Constants.IntersectionBetweenLinesCheck, Constants.IntersectionSelfCheck);

                IntersectionCheckType intersectionCheckType = intersectionCheckValue == Constants.IntersectionBetweenLinesCheck ? IntersectionCheckType.BetweenLinesCheck : IntersectionCheckType.SelfCheck;

                string pointsGroupName = PromptUtils.PromptKeyword("Intersection points will be created as COGO points. Input point group name or press enter to use default name:", allowNone: false, allowArbitraryInput: true, defaultValue: Constants.LineIntersectionPointsGroupName, Constants.LineIntersectionPointsGroupName);

                List<Point3d> intersectionPoints = new List<Point3d>();

                foreach (Autodesk.AutoCAD.DatabaseServices.DBObject dbObject in dbObjects)
                {
                    if (dbObject.IsBad())
                    {
                        continue;
                    }

                    if (dbObject is Polyline polyline)
                    {
                        List<Point3d> currentIntersectionPoints = new List<Point3d>();

                        if (intersectionCheckType == IntersectionCheckType.SelfCheck)
                        {
                            if (intersectionDimensionType == IntersectionDimensionType.ThreeDimensional)
                            {
                                currentIntersectionPoints = polyline.GetSelfIntersectionPoints(tolerance);
                            }
                            else
                            {
                                currentIntersectionPoints = polyline.GetSelfIntersectionPointsBy2d(tolerance, elevation: 0);
                            }

                            intersectionPoints.AddRange(currentIntersectionPoints);
                        }
                        else
                        {
                            List<Polyline> polylinesToCheck = dbObjects.Where(item => item is Polyline).Cast<Polyline>().ToList();

                            foreach (Polyline polylineToCheck in polylinesToCheck)
                            {
                                if (polylineToCheck == polyline)
                                {
                                    continue;
                                }

                                if (intersectionDimensionType == IntersectionDimensionType.ThreeDimensional)
                                {
                                    currentIntersectionPoints = polyline.GetIntersectionPoints(polylineToCheck, tolerance);
                                }
                                else
                                {
                                    currentIntersectionPoints = polyline.GetIntersectionPointsBy2d(polylineToCheck, tolerance, elevation: 0);
                                }

                                intersectionPoints.AddRange(currentIntersectionPoints);
                                currentIntersectionPoints.Clear();
                            }
                        }
                    }
                    else if (dbObject is Polyline3d polyline3d)
                    {
                        List<Point3d> currentIntersectionPoints = new List<Point3d>();

                        if (intersectionCheckType == IntersectionCheckType.SelfCheck)
                        {
                            if (intersectionDimensionType == IntersectionDimensionType.ThreeDimensional)
                            {
                                currentIntersectionPoints = polyline3d.GetSelfIntersectionPoints(tolerance);
                            }
                            else
                            {
                                currentIntersectionPoints = polyline3d.GetSelfIntersectionPointsBy2d(tolerance, elevation: 0);
                            }

                            intersectionPoints.AddRange(currentIntersectionPoints);
                            currentIntersectionPoints.Clear();
                        }
                        else
                        {
                            List<Polyline3d> polylines3dToCheck = dbObjects.Where(item => item is Polyline3d).Cast<Polyline3d>().ToList();

                            foreach (Polyline3d polyline3dToCheck in polylines3dToCheck)
                            {
                                if (polyline3d == polyline3dToCheck)
                                {
                                    continue;
                                }

                                if (intersectionDimensionType == IntersectionDimensionType.ThreeDimensional)
                                {
                                    currentIntersectionPoints = polyline3d.GetIntersectionPoints(polyline3dToCheck, tolerance);
                                }
                                else
                                {
                                    currentIntersectionPoints = polyline3d.GetIntersectionPointsBy2d(polyline3dToCheck, tolerance, elevation: 0);
                                }

                                intersectionPoints.AddRange(currentIntersectionPoints);
                                currentIntersectionPoints.Clear();
                            }
                        }
                    }
                    else if (dbObject is FeatureLine featureLine)
                    {
                        List<Point3d> currentIntersectionPoints = new List<Point3d>();

                        if (intersectionCheckType == IntersectionCheckType.SelfCheck)
                        {
                            if (intersectionDimensionType == IntersectionDimensionType.ThreeDimensional)
                            {
                                currentIntersectionPoints = featureLine.GetSelfIntersectionPoints(tolerance);
                            }
                            else
                            {
                                currentIntersectionPoints = featureLine.GetSelfIntersectionPointsBy2d(tolerance, elevation: 0);
                            }

                            intersectionPoints.AddRange(currentIntersectionPoints);
                            currentIntersectionPoints.Clear();
                        }
                        else
                        {
                            List<FeatureLine> featureLinesToCheck = dbObjects.Where(item => item is FeatureLine).Cast<FeatureLine>().ToList();

                            foreach (FeatureLine featureLineToCheck in featureLinesToCheck)
                            {

                                if (featureLine == featureLineToCheck)
                                {
                                    continue;
                                }

                                if (intersectionDimensionType == IntersectionDimensionType.ThreeDimensional)
                                {
                                    currentIntersectionPoints = featureLine.GetIntersectionPoints(featureLineToCheck, tolerance);
                                }
                                else
                                {

                                    currentIntersectionPoints = featureLine.GetIntersectionPointsBy2d(featureLineToCheck, tolerance, elevation: 0);
                                }
                            }

                            intersectionPoints.AddRange(currentIntersectionPoints);
                            currentIntersectionPoints.Clear();
                        }
                    }
                    else if (dbObject is Pipe pipe)
                    {
                        if (intersectionCheckType == IntersectionCheckType.BetweenLinesCheck)
                        {
                            List<Pipe> pipesToCheck = dbObjects.Where(item => item is Pipe).Cast<Pipe>().ToList();

                            foreach (Pipe pipeToCheck in pipesToCheck)
                            {
                                if (pipe == pipeToCheck)
                                {
                                    continue;
                                }

                                if (intersectionDimensionType == IntersectionDimensionType.ThreeDimensional)
                                {
                                    Point3d? intersectionPoint = pipe.GetIntersectionPoint(pipeToCheck, tolerance);

                                    if (intersectionPoint.HasValue)
                                    {
                                        intersectionPoints.Add(intersectionPoint.Value);
                                    }
                                }
                                else
                                {
                                    Point3d? intersectionPoint = pipe.GetIntersectionPointsBy2d(pipeToCheck, tolerance, elevation: 0);

                                    if (intersectionPoint.HasValue)
                                    {
                                        intersectionPoints.Add(intersectionPoint.Value);
                                    }
                                }
                            }
                        }
                    }
                }

                intersectionPoints = intersectionPoints.GroupBy(point => $"{Math.Round(point.X, Constants.Accurracy)}, {Math.Round(point.Y, Constants.Accurracy)}, {Math.Round(point.Z, Constants.Accurracy)}").Select(item => item.First()).ToList();
                PointGroup pointGroup = CogoPointUtils.CreateCogoPointGroup(pointsGroupName);
                CogoPointUtils.CreateCogoPoints(intersectionPoints, pointsGroupName);

                pointGroup.Update();

                MessageBox.Show($"Self intersection points count: {intersectionPoints.Count}");
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message, "Error");
            }
        }
    }
}
