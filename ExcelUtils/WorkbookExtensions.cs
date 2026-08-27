using Microsoft.VisualBasic;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace ExcelUtils
{
    public static class WorkbookExtensions
    {
        public static ICellStyle CreateHeaderStyle(this IWorkbook workbook)
        {
            ICellStyle style = workbook.CreateCellStyle();

            style.FillForegroundColor = IndexedColors.DarkBlue.Index;
            style.FillPattern = FillPattern.SolidForeground;

            IFont font = workbook.CreateFont();
            font.IsBold = true;
            font.Color = IndexedColors.White.Index;

            style.SetFont(font);

            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;

            return style;
        }

        public static ICellStyle CreateIntegerStyle(this IWorkbook workbook)
        {
            ICellStyle style = workbook.CreateCellStyle();

            IDataFormat format = workbook.CreateDataFormat();
            style.DataFormat = format.GetFormat("0");

            return style;
        }

        public static ICellStyle CreateDecimalStyle(
            this IWorkbook workbook,
            string formatString)
        {
            ICellStyle style = workbook.CreateCellStyle();

            IDataFormat format = workbook.CreateDataFormat();
            style.DataFormat = format.GetFormat(formatString);

            return style;
        }
    }
}
