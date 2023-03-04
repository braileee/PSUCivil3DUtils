using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace Civil3DAlignmentStationCoordinatesTable.Utils
{
    public class TableUtils
    {
        public static Table CreateTable(
            string promptInsertionPointsMessage,
            double rowHeight, double columnWidth)
        {
            var adoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var db = adoc.Database;
            var cdoc = CivilDocument.GetCivilDocument(adoc.Database);
            var ed = adoc.Editor;
            Table table = null;
            using (Transaction ts = db.TransactionManager.StartTransaction())
            {
                var pr = ed.GetPoint(promptInsertionPointsMessage);
                if (pr.Status == PromptStatus.OK)
                {
                    table = new Table();
                    table.TableStyle = db.Tablestyle;
                    table.SetRowHeight(rowHeight);
                    table.SetColumnWidth(columnWidth);
                    table.Position = pr.Value;
                    var bt = (BlockTable)ts.GetObject(adoc.Database.BlockTableId,
                        OpenMode.ForRead);
                    var btr = (BlockTableRecord)ts.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite, true, true);
                    btr.AppendEntity(table);
                    ts.AddNewlyCreatedDBObject(table, true);
                }
                ts.Commit();
            }
            return table;
        }

        public static Table CreateTable(
            string promptInsertionPointsMessage,
            double rowHeight, double columnWidth,
            int rowCount, int columnCount)
        {
            var adoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var db = adoc.Database;
            var cdoc = CivilDocument.GetCivilDocument(adoc.Database);
            var ed = adoc.Editor;
            Table tb = null;
            using (Transaction ts = db.TransactionManager.StartTransaction())
            {
                var pr = ed.GetPoint(promptInsertionPointsMessage);
                if (pr.Status == PromptStatus.OK)
                {
                    tb = new Table();
                    tb.TableStyle = db.Tablestyle;
                    //set common table width
                    tb.Width = columnWidth * columnCount;
                    tb.SetRowHeight(rowHeight);
                    tb.SetColumnWidth(columnWidth);
                    tb.Position = pr.Value;
                    var bt = (BlockTable)ts.GetObject(adoc.Database.BlockTableId,
                        OpenMode.ForRead);
                    var btr = (BlockTableRecord)ts.GetObject(
                        bt[BlockTableRecord.ModelSpace],
                        OpenMode.ForWrite, true, true);
                    //set size
                    tb.SetSize(rowCount, columnCount);
                    btr.AppendEntity(tb);
                    ts.AddNewlyCreatedDBObject(tb, true);
                }
                ts.Commit();
            }
            return tb;
        }

        public static Table CreateTable(
            TableStyle tableStyle,
           string promptInsertionPointsMessage,
           double rowHeight, double columnWidth,
           int rowCount, int columnCount)
        {
            var adoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var db = adoc.Database;
            var cdoc = CivilDocument.GetCivilDocument(adoc.Database);
            var ed = adoc.Editor;
            Table tb = null;

            using (adoc.LockDocument())
            {
                using (Transaction ts = db.TransactionManager.StartTransaction())
                {
                    var pr = ed.GetPoint(promptInsertionPointsMessage);
                    if (pr.Status == PromptStatus.OK)
                    {
                        tb = new Table();
                        tb.TableStyle = tableStyle.Id;
                        //set common table width
                        tb.Width = columnWidth * columnCount;
                        tb.SetRowHeight(rowHeight);
                        tb.SetColumnWidth(columnWidth);
                        tb.Position = pr.Value;
                        var bt = (BlockTable)ts.GetObject(adoc.Database.BlockTableId,
                            OpenMode.ForRead);
                        var btr = (BlockTableRecord)ts.GetObject(
                            bt[BlockTableRecord.ModelSpace],
                            OpenMode.ForWrite, true, true);
                        //set size
                        tb.SetSize(rowCount, columnCount);
                        btr.AppendEntity(tb);
                        ts.AddNewlyCreatedDBObject(tb, true);
                    }
                    ts.Commit();
                }
            }
            
            return tb;
        }

        public static void AddCellToTheTable(
            Table oTable, int rowHeight, int insertColumnNumber,
            string textValue)
        {
            int lastRow = oTable.Rows.Count - 1;
            oTable.InsertRows(lastRow, rowHeight, 1);
            oTable.Cells[lastRow, insertColumnNumber].TextString = textValue;
        }


        public static Table CreateTable(
            string promptInsertionPointsMessage,
            int rowsCount, int columnsCount)
        {
            var adoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var db = adoc.Database;
            var ed = adoc.Editor;
            Table tb = null;
            using (var ts = db.TransactionManager.StartTransaction())
            {
                var pr = ed.GetPoint(promptInsertionPointsMessage);
                if (pr.Status == PromptStatus.OK)
                {
                    tb = new Table
                    {
                        TableStyle = db.Tablestyle,
                        Position = pr.Value
                    };
                    var bt = (BlockTable)ts.GetObject(adoc.Database.BlockTableId,
                        OpenMode.ForRead);
                    var btr = (BlockTableRecord)ts.GetObject(
                        bt[BlockTableRecord.ModelSpace],
                        OpenMode.ForWrite, true, true);
                    //set size
                    btr.AppendEntity(tb);
                    ts.AddNewlyCreatedDBObject(tb, true);
                }
                if (tb != null)
                {
                    tb.SetSize(rowsCount, columnsCount);
                }
                ts.Commit();
            }
            return tb;
        }

        public static void SetWidthToTableColumns(
            Table oTable,
            int startColumnNumber,
            params int[] columnWidths)
        {
            if (oTable == null || columnWidths.Length == 0)
                return;
            int counter = 0;
            for (int i = startColumnNumber; i < columnWidths.Length + startColumnNumber; i++)
            {
                oTable.Columns[i].Width = columnWidths[counter++];
            }
        }
        public static void SetHeightToTableRows(
            Table oTable,
            int startRowNumber,
            params int[] rowsHeight)
        {
            if (oTable == null || rowsHeight.Length == 0)
                return;
            int counter = 0;
            for (int i = startRowNumber; i < rowsHeight.Length + startRowNumber; i++)
            {
                oTable.Rows[i].Height = rowsHeight[counter++];
            }
        }

        public static void FillTheTableByColumnNumber(
            Table oTable,
            int startRowNumber,
            int columnNumber,
            params string[] textForRowsToFill)
        {
            foreach (var text in textForRowsToFill)
            {
                oTable.Cells[startRowNumber++, columnNumber]
                      .TextString = text;
            }
        }

        public static void FillTheTableByRowNumber(
            Table oTable,
            int rowNumber,
            int startColumnNumber,
            params string[] textForRowsToFill)
        {
            foreach (var text in textForRowsToFill)
            {
                oTable.Cells[rowNumber, startColumnNumber++]
                      .TextString = text;
            }
        }

        public static void MergeCellsIfStringsAreEquals(Table oTable, int columnNumber)
        {
            for (int i = 0; i < oTable.Rows.Count; i++)
            {
                if (i == oTable.Rows.Count - 1)
                {
                    return;
                }
                if (oTable.Cells[i, columnNumber].TextString
                          .Equals(oTable.Cells[i + 1, columnNumber].TextString)
                          && oTable.Rows.Count > 1)
                {
                    if (oTable.Cells[i, columnNumber].TextString.Count() > 1)
                    {
                        oTable.MergeCells(
                       CellRange.Create(oTable, i, columnNumber, i + 1, columnNumber));
                    }
                }
            }
        }

        public static void DeleteRowsIfCellIsEmpty(Table oTable, int columnNumber, int excludeRowNumber)
        {
            List<int> rowsToDelete = new List<int>();
            for (int i = 0; i < oTable.Rows.Count; i++)
            {
                if (i == excludeRowNumber)
                {
                    continue;
                }
                if (oTable.Cells[i, columnNumber].TextString.Count() <= 0)
                {
                    rowsToDelete.Add(i);
                }
            }
            if (rowsToDelete.Count > 0)
            {
                oTable.DeleteRows(rowsToDelete.Min(), rowsToDelete.Count());
            }

        }

        public static double GetCommonRowHeight(Table oTable, int startRowNumber, int endRowNumber, int columnNumber)
        {
            double commonHeight = 0;
            for (int i = startRowNumber; i < endRowNumber + 1; i++)
            {
                commonHeight += oTable.Rows[i].Height;
            }
            return commonHeight;
        }

        public static double GetCommonColumnWidth(Table oTable, int startColumnNumber, int endColumnNumber, int rowNumber)
        {
            double commonWidth = 0;
            for (int i = startColumnNumber; i < endColumnNumber + 1; i++)
            {
                commonWidth += oTable.Columns[i].Width;
            }
            return commonWidth;
        }

        public static Point3d GetMiddlePointCellCoordinates(
            Table oTable,
            int rowNumber,
            int columnNumber)
        {
            double relativeY
                = GetCommonRowHeight(oTable, 0, rowNumber, columnNumber)
                - oTable.Rows[rowNumber].Height / 2;
            double relativeX
                = GetCommonColumnWidth(oTable, 0, columnNumber, rowNumber) -
                oTable.Columns[columnNumber].Width / 2;
            var startPoint = oTable.Position;
            var insertionPoint
                = new Point3d(startPoint.X + relativeX, startPoint.Y - relativeY, 0);
            return insertionPoint;
        }

        public static void MoveBlockFromPointToPoint(BlockReference oBlockRef, Point3d pointFrom, Point3d pointTo)
        {
            Vector3d v3d = pointFrom.GetVectorTo(pointTo);
            oBlockRef.TransformBy(Matrix3d.Displacement(v3d));
        }

        public static Point3d GetCenterCoordinatesOfBlocks(BlockReference oBlockRef)
        {
            var maxPoint = oBlockRef.Bounds.Value.MaxPoint;
            var minPoint = oBlockRef.Bounds.Value.MinPoint;
            return new Point3d(
                    (maxPoint.X + minPoint.X) / 2,
                    (maxPoint.Y + minPoint.Y) / 2,
                    (maxPoint.Z + minPoint.Z) / 2);
        }

        public static TableStyle GetTableStyleByName(string styleName, OpenMode openMode)
        {
            if (string.IsNullOrEmpty(styleName))
                return null;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            TableStyle oTableStyle = null;
            ObjectId tableStyleId = ObjectId.Null;

            using (var ts = db.TransactionManager.StartTransaction())
            {
                DBDictionary tableStyleDbDict = (DBDictionary)ts.GetObject(db.TableStyleDictionaryId, OpenMode.ForRead);
                tableStyleId = tableStyleDbDict.GetAt(styleName);
                oTableStyle = ts.GetObject(tableStyleId, openMode) as TableStyle;
                ts.Commit();
            }

            return oTableStyle;
        }

        public static List<TableStyle> GetAllTableStyles(OpenMode openMode)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            List<TableStyle> oTableStyles = new List<TableStyle>();

            using (var ts = db.TransactionManager.StartTransaction())
            {
                DBDictionary tableStyleDbDict = (DBDictionary)ts.GetObject(db.TableStyleDictionaryId, OpenMode.ForRead);
                foreach (DBDictionaryEntry tableStyleDbDictEntry in tableStyleDbDict)
                {
                    if (tableStyleDbDictEntry.Value.IsNull)
                        continue;
                    TableStyle oTableStyle = ts.GetObject(tableStyleDbDictEntry.Value, openMode) as TableStyle;
                    oTableStyles.Add(oTableStyle);
                }
                ts.Commit();
            }

            return oTableStyles;
        }
    }
}
