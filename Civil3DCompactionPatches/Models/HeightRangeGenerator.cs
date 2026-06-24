using AutoCADUtils;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Civil3DCompactionPatches.Models
{
    public static class HeightRangeGenerator
    {

        public static List<HeightRange> Generate()
        {
            return new List<HeightRange>
    {
        new HeightRange { From = double.MinValue, To = 0.4, Color = Color.FromRgb(0, 0, 255) },

        new HeightRange { From = 0.4, To = 0.5, Color = Color.FromRgb(0, 180, 255) },
        new HeightRange { From = 0.5, To = 0.6, Color = Color.FromRgb(150, 220, 220) },
        new HeightRange { From = 0.6, To = 0.7, Color = Color.FromRgb(0, 255, 0) },
        new HeightRange { From = 0.7, To = 0.8, Color = Color.FromRgb(140, 220, 140) },
        new HeightRange { From = 0.8, To = 0.9, Color = Color.FromRgb(0, 170, 0) },

        new HeightRange { From = 0.9, To = 1.0, Color = Color.FromRgb(230, 230, 120) },
        new HeightRange { From = 1.0, To = 1.1, Color = Color.FromRgb(255, 255, 0) },
        new HeightRange { From = 1.1, To = 1.2, Color = Color.FromRgb(255, 200, 0) },
        new HeightRange { From = 1.2, To = 1.3, Color = Color.FromRgb(255, 150, 0) },
        new HeightRange { From = 1.3, To = 1.4, Color = Color.FromRgb(255, 100, 0) },

        new HeightRange { From = 1.4, To = 1.5, Color = Color.FromRgb(255, 0, 0) },
        new HeightRange { From = 1.5, To = 1.6, Color = Color.FromRgb(200, 0, 0) },
        new HeightRange { From = 1.6, To = 1.85, Color = Color.FromRgb(255, 0, 200) },

        new HeightRange { From = 1.85, To = double.MaxValue, Color = Color.FromRgb(150, 0, 200) }
    };
        }

        public static HeightRange FindRange(double value, List<HeightRange> ranges)
        {
            foreach (var r in ranges)
            {
                if (value >= r.From && value < r.To)
                    return r;
            }
            return null;
        }


        public static void DrawLegend(
    List<HeightRange> ranges,
    Point3d basePoint,
    double squareSize = 2.0,
    double rowSpacing = 3.0,
    double textOffsetX = 3.0)
        {
            var doc = AutocadDocumentService.ActiveDocument;
            var db = doc.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                var groupDict = (DBDictionary)tr.GetObject(db.GroupDictionaryId, OpenMode.ForWrite);

                var allIds = new ObjectIdCollection();

                double y = basePoint.Y;

                foreach (var range in ranges)
                {
                    // ----------------------------------
                    // ✅ Polyline square
                    // ----------------------------------
                    var pl = new Polyline(4);
                    pl.AddVertexAt(0, new Point2d(basePoint.X, y), 0, 0, 0);
                    pl.AddVertexAt(1, new Point2d(basePoint.X + squareSize, y), 0, 0, 0);
                    pl.AddVertexAt(2, new Point2d(basePoint.X + squareSize, y + squareSize), 0, 0, 0);
                    pl.AddVertexAt(3, new Point2d(basePoint.X, y + squareSize), 0, 0, 0);
                    pl.Closed = true;

                    btr.AppendEntity(pl);
                    tr.AddNewlyCreatedDBObject(pl, true);

                    // ----------------------------------
                    // ✅ Hatch
                    // ----------------------------------
                    var hatch = new Hatch();
                    hatch.SetDatabaseDefaults();
                    hatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
                    hatch.Color = range.Color;

                    btr.AppendEntity(hatch);
                    tr.AddNewlyCreatedDBObject(hatch, true);

                    hatch.Associative = true;
                    hatch.AppendLoop(HatchLoopTypes.Default, new ObjectIdCollection { pl.ObjectId });
                    hatch.EvaluateHatch(true);

                    // ----------------------------------
                    // ✅ Text
                    // ----------------------------------
                    var pos = new Point3d(basePoint.X + textOffsetX, y + squareSize / 2, 0);

                    var text = new DBText
                    {
                        Position = pos,
                        Height = 1.5,
                        TextString = FormatRange(range),
                        VerticalMode = TextVerticalMode.TextVerticalMid,
                        HorizontalMode = TextHorizontalMode.TextLeft,
                        AlignmentPoint = pos
                    };

                    btr.AppendEntity(text);
                    tr.AddNewlyCreatedDBObject(text, true);

                    // ✅ Collect all entities
                    allIds.Add(pl.ObjectId);
                    allIds.Add(hatch.ObjectId);
                    allIds.Add(text.ObjectId);

                    y -= rowSpacing;
                }

                // ----------------------------------
                // ✅ Single group for entire legend
                // ----------------------------------
                if (allIds.Count > 0)
                {
                    string groupName = $"HR_LEGEND_{Guid.NewGuid()}";

                    var group = new Group("Height Range Legend", true);
                    groupDict.SetAt(groupName, group);
                    tr.AddNewlyCreatedDBObject(group, true);

                    foreach (ObjectId id in allIds)
                    {
                        group.Append(id);
                    }
                }

                tr.Commit();
            }
        }

        private static string FormatRange(HeightRange range)
        {
            if (range.From == double.MinValue)
                return $"<{range.To:0.##}";

            if (range.To == double.MaxValue)
                return $">{range.From:0.##}";

            return $"{range.From:0.##}-{range.To:0.##}";
        }
    }
}
