using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace Core.Helpers
{
    public static class ExcelHelper
    {
        public static void CsvToExcel(string excelFileName, string worksheetName, List<List<string>> csvLines)
        {
            if (csvLines == null || !csvLines.Any())
            {
                return;
            }

            int rowCount = 0;

            IWorkbook workbook = new XSSFWorkbook();
            ISheet worksheet = workbook.CreateSheet(worksheetName);

            foreach (var line in csvLines)
            {
                IRow row = worksheet.CreateRow(rowCount);

                var colCount = 0;
                foreach (var col in line)
                {
                    row.CreateCell(colCount).SetCellValueGeneric(col);
                    colCount++;
                }

                rowCount++;
            }

            using (FileStream fileWriter = File.Create(excelFileName))
            {
                workbook.Write(fileWriter);
                fileWriter.Close();
            }

            worksheet = null;
            workbook = null;
        }

        public static void SetCellValueGeneric(this ICell cell, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                cell.SetCellValue(string.Empty);
            }

            double doubleValue = 0;
            if (double.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out doubleValue))
            {
                cell.SetCellValue(doubleValue);
                return;
            }


            bool boolValue = false;
            if (bool.TryParse(value, out boolValue))
            {
                cell.SetCellValue(boolValue);
                return;
            }

            DateTime dateTimeValue = DateTime.MinValue;
            if (DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dateTimeValue))
            {
                cell.SetCellValue(dateTimeValue);
                return;
            }

            cell.SetCellValue(value);
        }

        public static void FormatFaiExcel(string path, int maxRowIndex, int minRowIndex, int numFaiRows,
            int numFaiColumns)
        {
            XSSFWorkbook wb;
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                wb = new XSSFWorkbook(file);
            }

            var ws = wb.GetSheetAt(0);

            for (int i = 3; i < numFaiRows + 3; i++)
            {
                var row = ws.GetRow(i);
                for (int j = 0; j < numFaiColumns; j++)
                {
                    var cell = row.Cells[j];
                    var columnIndex = GetExcelColumnName(cell.ColumnIndex+1);
                    var maxFormula = $"=${columnIndex}${maxRowIndex + 1}";
                    var minFormula = $"=${columnIndex}${minRowIndex + 1}";

                    XSSFSheetConditionalFormatting sCF = (XSSFSheetConditionalFormatting) ws.SheetConditionalFormatting;

//Fill red if too big
                    XSSFConditionalFormattingRule formatTooBig =
                        (XSSFConditionalFormattingRule) sCF.CreateConditionalFormattingRule(
                            ComparisonOperator.GreaterThan, maxFormula);
                    XSSFPatternFormatting fillRed = (XSSFPatternFormatting) formatTooBig.CreatePatternFormatting();
                    fillRed.FillBackgroundColor = IndexedColors.Red.Index;
                    fillRed.FillPattern = (short) FillPattern.SolidForeground;
                    
                    //Fill red if too big
                    XSSFConditionalFormattingRule formatTooSmall =
                        (XSSFConditionalFormattingRule) sCF.CreateConditionalFormattingRule(
                            ComparisonOperator.LessThan, minFormula);
                    XSSFPatternFormatting fillBlue = (XSSFPatternFormatting) formatTooSmall.CreatePatternFormatting();
                    fillBlue.FillBackgroundColor = IndexedColors.Blue.Index;
                    fillBlue.FillPattern = (short) FillPattern.SolidForeground;

                    CellRangeAddress[] cfRange = { CellRangeAddress.ValueOf($"{columnIndex}{i+1}") };
                    sCF.AddConditionalFormatting(cfRange, formatTooBig, formatTooSmall);
                }
            }
            
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                wb.Write(stream);
            }  
        }

        private static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int) ((dividend - modulo) / 26);
            }

            return columnName;
        }
    }
}