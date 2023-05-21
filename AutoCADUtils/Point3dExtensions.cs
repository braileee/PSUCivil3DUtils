using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADUtils
{
    public static class Point3dExts
    {

        public static bool IsPointBetweenLine(this Point3d checkPoint, Point3d startPoint, Point3d endPoint, int tolerance)
        {
            double startToCheckPointDistance = startPoint.DistanceTo(checkPoint);
            double endToCheckPointDistance = endPoint.DistanceTo(checkPoint);
            double startToEndPointDistance = startPoint.DistanceTo(endPoint);

            if (Math.Round(startToCheckPointDistance + endToCheckPointDistance, tolerance) ==
                Math.Round(startToEndPointDistance, tolerance))
                return true;
            return false;
        }
       
        public static bool IsPointBetweenLineButNotEqualByXY(this Point3d checkPoint, Point3d startPoint,
                                                                    Point3d endPoint, int tolerance)
        {
            if (startPoint.IsEqualByXY(checkPoint, tolerance) ||
                endPoint.IsEqualByXY(checkPoint, tolerance))
            {
                return false;
            }
            return IsPointBetweenLine(startPoint, endPoint, checkPoint, tolerance);
        }

        public static bool IsEqualByXY(this Point3d point1, Point3d point2, int accuracy)
        {
            if (Math.Round(point1.X, accuracy) == Math.Round(point2.X, accuracy) &&
                Math.Round(point1.Y, accuracy) == Math.Round(point2.Y, accuracy))
                return true;
            return false;
        }

        public static bool IsEqualByXYZ(this Point3d point1, Point3d point2, int accuracy)
        {
            if (Math.Round(point1.X, accuracy) == Math.Round(point2.X, accuracy) &&
                Math.Round(point1.Y, accuracy) == Math.Round(point2.Y, accuracy) &&
                Math.Round(point1.Z, accuracy) == Math.Round(point2.Z, accuracy))
                return true;
            return false;
        }

        public static bool isEqualBy2d(this Point3d thisPoint, Point3d somePoint, int accuracy)
        {
            return
                Math.Round(thisPoint.X, accuracy) == Math.Round(somePoint.X, accuracy) &&
                Math.Round(thisPoint.Y, accuracy) == Math.Round(somePoint.Y, accuracy);
        }

        public static bool isEqualBy2d(this Point3d thisPoint, Point2d somePoint, int accuracy)
        {
            return
                Math.Round(thisPoint.X, accuracy) == Math.Round(somePoint.X, accuracy) &&
                Math.Round(thisPoint.Y, accuracy) == Math.Round(somePoint.Y, accuracy);
        }

        public static Point2d ConvertToPoint2d(this Point3d thisPoint)
        {
            return new Point2d(thisPoint.X, thisPoint.Y);
        }

        public static Point3d GetMiddlePointWith(this Point3d point, Point3d otherPoint)
        {
            return new Point3d((point.X + otherPoint.X) / 2,
                                (point.Y + otherPoint.Y) / 2,
                                (point.Z + otherPoint.Z) / 2);
        }

        public static Point3d OffsetByY(this Point3d point, double offsetY)
        {
            return new Point3d(point.X,
                                point.Y + offsetY,
                                point.Z);
        }

        public static Point3d OffsetByZ(this Point3d point, double offsetZ)
        {
            return new Point3d(point.X,
                                point.Y,
                                point.Z + offsetZ);
        }

        public static Point3d OffsetByXY(this Point3d point, double offsetX, double offsetY)
        {
            return new Point3d(point.X + offsetX,
                                point.Y + offsetY,
                                point.Z);
        }

        public static Point3d OffsetByX(this Point3d point, double offsetX)
        {
            return new Point3d(point.X + offsetX,
                                point.Y,
                                point.Z);
        }
    }
}
