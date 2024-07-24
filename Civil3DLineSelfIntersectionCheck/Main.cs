using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DatabaseServices;
using Civil3DLineSelfIntersectionCheck.Enums;
using Civil3DUtils;
using Civil3DUtils.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Civil3DLineSelfIntersectionCheck
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DLineSelfIntersectionCheck", CommandFlags.Modal)]
        public static void Civil3DLineSelfIntersectionCheck()
        {
            try
            {
                int tolerance = 8;

                List<Autodesk.AutoCAD.DatabaseServices.DBObject> dbObjects = SelectionUtils.GetDbObjects("Select polylines, 3d polylines, feature lines to detect self intersection:", OpenMode.ForRead);

                if (!dbObjects.Any())
                {
                    return;
                }

                string intersectionDetectionKeyword = PromptUtils.PromptKeyword("Select intersection detection method:", allowNone: false, allowArbitraryInput: false, defaultValue: Constants.TwoDimensionalKeyword, Constants.TwoDimensionalKeyword, Constants.ThreeDimensionalKeyword);

                if (string.IsNullOrEmpty(intersectionDetectionKeyword))
                {
                    MessageBox.Show("Incorrect input, operation has been canceled");
                    return;
                }

                string pointsGroupName = PromptUtils.PromptKeyword("Intersection points will be created as COGO points. Input point group name or press enter to use default name:", allowNone: false, allowArbitraryInput: true, defaultValue: Constants.LineIntersectionPointsGroupName, Constants.LineIntersectionPointsGroupName);

                IntersectionDetection intersectionDetection = intersectionDetectionKeyword == Constants.ThreeDimensionalKeyword ? IntersectionDetection.ThreeDimensional : IntersectionDetection.TwoDimensional;

                List<Point3d> intersectionPoints = new List<Point3d>();
                foreach (Autodesk.AutoCAD.DatabaseServices.DBObject dbObject in dbObjects)
                {
                    if (dbObject.IsBad())
                    {
                        continue;
                    }

                    if (dbObject is Polyline polyline)
                    {
                        List<Point3d> currentIntersectionPoints = polyline.GetSelfIntersectionPoints(tolerance);
                        intersectionPoints.AddRange(currentIntersectionPoints);
                    }
                    else if (dbObject is Polyline3d polyline3d)
                    {
                        List<Point3d> currentIntersectionPoints = new List<Point3d>();

                        if (intersectionDetection == IntersectionDetection.ThreeDimensional)
                        {
                            currentIntersectionPoints = polyline3d.GetSelfIntersectionPoints(tolerance);
                        }
                        else
                        {
                            currentIntersectionPoints = polyline3d.GetSelfIntersectionPointsBy2d(tolerance, elevation: 0);
                        }

                        intersectionPoints.AddRange(currentIntersectionPoints);
                    }
                    else if (dbObject is FeatureLine featureLine)
                    {
                        List<Point3d> currentIntersectionPoints = new List<Point3d>();


                        if (intersectionDetection == IntersectionDetection.ThreeDimensional)
                        {
                            currentIntersectionPoints = featureLine.GetSelfIntersectionPoints(tolerance);
                        }
                        else
                        {
                            currentIntersectionPoints = featureLine.GetSelfIntersectionPointsBy2d(tolerance, elevation: 0);
                        }

                        intersectionPoints.AddRange(currentIntersectionPoints);
                    }
                }

                PointGroup pointGroup = CogoPointUtils.CreateCogoPointGroup(pointsGroupName);
                CogoPointUtils.CreateCogoPoints(intersectionPoints, pointsGroupName);

                pointGroup.Update();

                MessageBox.Show($"Self ntersection points count: {intersectionPoints.Count}");
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message, "Error");
            }
        }
    }
}
