using Autodesk.AutoCAD.DatabaseServices;

namespace AutoCADUtils
{
    public static class TableExtensions
    {
        public static void TrySetValue(this Table table, int rowIndex, int columnIndex, string value, double textHeight, CellAlignment cellAlignment)
        {
            if (rowIndex > table.Rows.Count - 1 || columnIndex > table.Columns.Count - 1)
            {
                return;
            }

            table.Cells[rowIndex, columnIndex].TextString = value;
            table.Cells[rowIndex, columnIndex].TextHeight = textHeight;
            table.Cells[rowIndex, columnIndex].Alignment = cellAlignment;
        }
    }
}
