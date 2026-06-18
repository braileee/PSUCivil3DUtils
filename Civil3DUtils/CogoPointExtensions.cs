using AutoCADUtils;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Civil3DUtils
{
    public static class CogoPointExtensions
    {
        public static PointStyle GetPointStyle(this CogoPoint point, Transaction tr)
        {
            var styleId = point.StyleId;

            PointStyle style = null;

            if (styleId.IsNull)
            {
                PointGroup pointGroup = tr.GetObject(point.PrimaryPointGroupId, OpenMode.ForRead) as PointGroup;
                styleId = pointGroup.PointStyleId;
            }

            // Get point style
            style = tr.GetObject(styleId, OpenMode.ForRead) as PointStyle;

            return style;
        }

        public static BlockTableRecord GetMarkerBlockTableReceord(this CogoPoint point, Transaction tr)
        {
            PointStyle style = point.GetPointStyle(tr);

            string blockName = style.MarkerSymbolName;

            if (string.IsNullOrEmpty(style.MarkerSymbolName))
            {
                AutocadDocumentService.Editor.WriteMessage("\nMarker does not use a block.");
                return null;
            }

            // Open block table
            BlockTable bt = tr.GetObject(AutocadDocumentService.Database.BlockTableId, OpenMode.ForRead) as BlockTable;

            if (!bt.Has(blockName))
            {
                AutocadDocumentService.Editor.WriteMessage("\nBlock not found.");
                return null;
            }

            BlockTableRecord btr = tr.GetObject(bt[blockName], OpenMode.ForRead) as BlockTableRecord;
            return btr;
        }

        public static Autodesk.AutoCAD.DatabaseServices.Entity TransformEntityToCogoPointLocation(this CogoPoint point, Autodesk.AutoCAD.DatabaseServices.Entity entity)
        {
            Polyline newPl = entity.Clone() as Polyline;

            Point3d location = point.Location;

            double rotation = point.MarkerRotation;

            Matrix3d transform =
                Matrix3d.Scaling(1, Point3d.Origin) *
                Matrix3d.Rotation(rotation, Vector3d.ZAxis, point.Location) *
                Matrix3d.Displacement(location - Point3d.Origin);

            newPl.TransformBy(transform);

            return newPl;
        }
    }
}
