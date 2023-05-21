using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Acad = Autodesk.AutoCAD.DatabaseServices;

namespace AutoCADUtils.Utils
{
    public static class TableUtils
    {
        public static Acad.Table CreateTable(
            string promptInsertionPointsMessage,
            double rowHeight, double columnWidth)
        {
            var adoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var db = adoc.Database;
            var ed = adoc.Editor;
            Acad.Table table = null;
            using (Acad.Transaction ts = db.TransactionManager.StartTransaction())
            {
                var pr = ed.GetPoint(promptInsertionPointsMessage);
                if (pr.Status == PromptStatus.OK)
                {
                    table = new Acad.Table();
                    table.TableStyle = db.Tablestyle;
                    table.SetRowHeight(rowHeight);
                    table.SetColumnWidth(columnWidth);
                    table.Position = pr.Value;
                    var bt = (Acad.BlockTable)ts.GetObject(adoc.Database.BlockTableId,
                        Acad.OpenMode.ForRead);
                    var btr = (Acad.BlockTableRecord)ts.GetObject(bt[Acad.BlockTableRecord.ModelSpace], Acad.OpenMode.ForWrite, true, true);
                    btr.AppendEntity(table);
                    ts.AddNewlyCreatedDBObject(table, true);
                }
                ts.Commit();
            }
            return table;
        }

        public static Acad.Table CreateTable(
            string promptInsertionPointsMessage,
            double rowHeight, double columnWidth,
            int rowCount, int columnCount)
        {
            var adoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var db = adoc.Database;
            var ed = adoc.Editor;
            Acad.Table tb = null;

            using (Acad.Transaction ts = db.TransactionManager.StartTransaction())
            {
                var pr = ed.GetPoint(promptInsertionPointsMessage);
                if (pr.Status == PromptStatus.OK)
                {
                    tb = new Acad.Table();
                    tb.TableStyle = db.Tablestyle;
                    //set common table width
                    tb.Width = columnWidth * columnCount;
                    tb.SetRowHeight(rowHeight);
                    tb.SetColumnWidth(columnWidth);
                    tb.Position = pr.Value;
                    var bt = (Acad.BlockTable)ts.GetObject(adoc.Database.BlockTableId,
                        Acad.OpenMode.ForRead);
                    var btr = (Acad.BlockTableRecord)ts.GetObject(
                        bt[Acad.BlockTableRecord.ModelSpace],
                        Acad.OpenMode.ForWrite, true, true);
                    //set size
                    tb.SetSize(rowCount, columnCount);
                    btr.AppendEntity(tb);
                    ts.AddNewlyCreatedDBObject(tb, true);
                }
                ts.Commit();
            }
            return tb;
        }

        public static Acad.Table CreateTable(
            Acad.TableStyle tableStyle,
           string promptInsertionPointsMessage,
           double rowHeight, double columnWidth,
           int rowCount, int columnCount)
        {
            var adoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var db = adoc.Database;
            var ed = adoc.Editor;
            Acad.Table tb = null;

            using (adoc.LockDocument())
            {
                using (Acad.Transaction ts = db.TransactionManager.StartTransaction())
                {
                    var pr = ed.GetPoint(promptInsertionPointsMessage);
                    if (pr.Status == PromptStatus.OK)
                    {
                        tb = new Acad.Table();
                        tb.TableStyle = tableStyle.Id;

                        //set common table width
                        tb.Width = columnWidth * columnCount;
                        tb.SetRowHeight(rowHeight);
                        tb.SetColumnWidth(columnWidth);
                        tb.Position = pr.Value;
                        Acad.BlockTable bt = (Acad.BlockTable)ts.GetObject(adoc.Database.BlockTableId, Acad.OpenMode.ForRead);
                        Acad.BlockTableRecord btr = (Acad.BlockTableRecord)ts.GetObject(bt[Acad.BlockTableRecord.ModelSpace], Acad.OpenMode.ForWrite, true, true);

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
            Acad.Table oTable, int rowHeight, int insertColumnNumber,
            string textValue)
        {
            int lastRow = oTable.Rows.Count - 1;
            oTable.InsertRows(lastRow, rowHeight, 1);
            oTable.Cells[lastRow, insertColumnNumber].TextString = textValue;
        }


        public static Acad.Table CreateTable(
            string promptInsertionPointsMessage,
            int rowsCount, int columnsCount)
        {
            var adoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var db = adoc.Database;
            var ed = adoc.Editor;
            Acad.Table tb = null;
            using (var ts = db.TransactionManager.StartTransaction())
            {
                var pr = ed.GetPoint(promptInsertionPointsMessage);
                if (pr.Status == PromptStatus.OK)
                {
                    tb = new Acad.Table
                    {
                        TableStyle = db.Tablestyle,
                        Position = pr.Value
                    };
                    var bt = (Acad.BlockTable)ts.GetObject(adoc.Database.BlockTableId,
                        Acad.OpenMode.ForRead);
                    var btr = (Acad.BlockTableRecord)ts.GetObject(
                        bt[Acad.BlockTableRecord.ModelSpace],
                        Acad.OpenMode.ForWrite, true, true);
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
            Acad.Table oTable,
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
            Acad.Table oTable,
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
            Acad.Table oTable,
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
            Acad.Table oTable,
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

        public static void MergeCellsIfStringsAreEquals(Acad.Table oTable, int columnNumber)
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
                       Acad.CellRange.Create(oTable, i, columnNumber, i + 1, columnNumber));
                    }
                }
            }
        }

        public static void DeleteRowsIfCellIsEmpty(Acad.Table oTable, int columnNumber, int excludeRowNumber)
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

        public static double GetCommonRowHeight(Acad.Table oTable, int startRowNumber, int endRowNumber, int columnNumber)
        {
            double commonHeight = 0;
            for (int i = startRowNumber; i < endRowNumber + 1; i++)
            {
                commonHeight += oTable.Rows[i].Height;
            }
            return commonHeight;
        }

        public static double GetCommonColumnWidth(Acad.Table oTable, int startColumnNumber, int endColumnNumber, int rowNumber)
        {
            double commonWidth = 0;
            for (int i = startColumnNumber; i < endColumnNumber + 1; i++)
            {
                commonWidth += oTable.Columns[i].Width;
            }
            return commonWidth;
        }

        public static Point3d GetMiddlePointCellCoordinates(
            Acad.Table oTable,
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

        public static void MoveBlockFromPointToPoint(Acad.BlockReference oBlockRef, Point3d pointFrom, Point3d pointTo)
        {
            Vector3d v3d = pointFrom.GetVectorTo(pointTo);
            oBlockRef.TransformBy(Matrix3d.Displacement(v3d));
        }

        public static Point3d GetCenterCoordinatesOfBlocks(Acad.BlockReference oBlockRef)
        {
            var maxPoint = oBlockRef.Bounds.Value.MaxPoint;
            var minPoint = oBlockRef.Bounds.Value.MinPoint;
            return new Point3d(
                    (maxPoint.X + minPoint.X) / 2,
                    (maxPoint.Y + minPoint.Y) / 2,
                    (maxPoint.Z + minPoint.Z) / 2);
        }

        public static Acad.TableStyle GetTableStyleByName(string styleName, Acad.OpenMode openMode)
        {
            if (string.IsNullOrEmpty(styleName))
                return null;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Acad.Database db = doc.Database;

            Acad.TableStyle oTableStyle = null;
            Acad.ObjectId tableStyleId = Acad.ObjectId.Null;

            using (var ts = db.TransactionManager.StartTransaction())
            {
                Acad.DBDictionary tableStyleDbDict = (Acad.DBDictionary)ts.GetObject(db.TableStyleDictionaryId, Acad.OpenMode.ForRead);
                tableStyleId = tableStyleDbDict.GetAt(styleName);
                oTableStyle = ts.GetObject(tableStyleId, openMode) as Acad.TableStyle;
                ts.Commit();
            }

            return oTableStyle;
        }

        public static List<Acad.TableStyle> GetAllTableStyles(Acad.OpenMode openMode)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Acad.Database db = doc.Database;

            List<Acad.TableStyle> oTableStyles = new List<Acad.TableStyle>();

            using (var ts = db.TransactionManager.StartTransaction())
            {
                Acad.DBDictionary tableStyleDbDict = (Acad.DBDictionary)ts.GetObject(db.TableStyleDictionaryId, Acad.OpenMode.ForRead);
                foreach (Acad.DBDictionaryEntry tableStyleDbDictEntry in tableStyleDbDict)
                {
                    if (tableStyleDbDictEntry.Value.IsNull)
                        continue;
                    Acad.TableStyle oTableStyle = ts.GetObject(tableStyleDbDictEntry.Value, openMode) as Acad.TableStyle;
                    oTableStyles.Add(oTableStyle);
                }
                ts.Commit();
            }

            return oTableStyles;
        }

        public static List<Acad.TableStyle> GetAllTableStyles(Acad.OpenMode openMode, Database database)
        {
            List<Acad.TableStyle> oTableStyles = new List<Acad.TableStyle>();

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                Acad.DBDictionary tableStyleDbDict = (Acad.DBDictionary)transaction.GetObject(database.TableStyleDictionaryId, Acad.OpenMode.ForRead);

                foreach (Acad.DBDictionaryEntry tableStyleDbDictEntry in tableStyleDbDict)
                {
                    if (tableStyleDbDictEntry.Value.IsNull)
                    {
                        continue;
                    }

                    Acad.TableStyle oTableStyle = transaction.GetObject(tableStyleDbDictEntry.Value, openMode) as Acad.TableStyle;
                    oTableStyles.Add(oTableStyle);
                }
                transaction.Commit();
            }

            return oTableStyles;
        }

        public static Acad.TableStyle CreateOrGetTableStyle(Acad.OpenMode openMode, string styleName, double horizontalCellMargin, double verticalCellMargin, double textHeight)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Acad.Database db = doc.Database;

            Acad.TableStyle tableStyle = null;

            using (Acad.Transaction transaction = db.TransactionManager.StartTransaction())
            {
                Acad.DBDictionary tableStyleDictionary = (Acad.DBDictionary)transaction.GetObject(db.TableStyleDictionaryId, openMode);

                if (!tableStyleDictionary.Contains(styleName))
                {
                    tableStyle = new Acad.TableStyle();

                    tableStyleDictionary.UpgradeOpen();
                    tableStyleDictionary.SetAt(styleName, tableStyle);
                    transaction.AddNewlyCreatedDBObject(tableStyle, true);

                    tableStyle.Name = styleName;
                    tableStyle.HorizontalCellMargin = horizontalCellMargin;
                    tableStyle.VerticalCellMargin = verticalCellMargin;

                    tableStyle.IsHeaderSuppressed = true;
                    tableStyle.IsTitleSuppressed = true;

                    tableStyleDictionary.DowngradeOpen();
                }
                else
                {
                    Acad.ObjectId? tableStyleId = tableStyleDictionary[styleName] as Acad.ObjectId?;

                    if (tableStyleId.HasValue)
                    {
                        tableStyle = transaction.GetObject(tableStyleId.Value, openMode, false, true) as Acad.TableStyle;
                    }
                }

                transaction.Commit();
            }

            return tableStyle;
        }

        public static void ImportTableStyle(string filePath, string tableStyleName)
        {
            DocumentCollection documentCollection = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            Document destinationDocument = documentCollection.MdiActiveDocument;

            Editor ed = destinationDocument.Editor;
            Acad.Database destinationDatabase = documentCollection.MdiActiveDocument.Database;
            Acad.Database sourceDatabase = new Acad.Database(false, true);

            try
            {
                // Read the DWG into a side database
                sourceDatabase.ReadDwgFile(filePath, System.IO.FileShare.Read, true, "");

                // Create a variable to store the list of block identifiers
                Acad.ObjectIdCollection elementIds = new Acad.ObjectIdCollection();

                Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = sourceDatabase.TransactionManager;

                using (Acad.Transaction transaction = transactionManager.StartTransaction())
                {
                    List<Acad.TableStyle> tableStyles = GetAllTableStyles(OpenMode.ForRead, sourceDatabase);

                    foreach (Acad.TableStyle tableStyle in tableStyles)
                    {
                        if (tableStyle.Name.Equals(tableStyleName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            elementIds.Add(tableStyle.Id);
                        }
                    }
                }

                using (DocumentLock documentLock = destinationDocument.LockDocument())
                {
                    Acad.IdMapping mapping = new Acad.IdMapping();
                    sourceDatabase.WblockCloneObjects(elementIds, destinationDatabase.TableStyleDictionaryId, mapping, Acad.DuplicateRecordCloning.Replace, false);
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                ed.WriteMessage("\nError during copy: " + ex.Message);
            }
            sourceDatabase.Dispose();
        }
    }
}
