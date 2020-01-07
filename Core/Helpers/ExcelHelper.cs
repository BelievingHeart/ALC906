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

            var workbook = new XSSFWorkbook();
            var worksheet = workbook.CreateSheet(worksheetName);

            foreach (var line in csvLines)
            {
                var row = worksheet.CreateRow(rowCount);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="maxRowIndex">Row index of max values</param>
        /// <param name="minRowIndex">Row index of min values</param>
        /// <param name="numFaiRows"></param>
        /// <param name="numFaiColumns"></param>
        /// <param name="numMiscRows">Count of rows other than fai value rows</param>
        public static void FormatFaiExcel(string path, int maxRowIndex, int minRowIndex, int numFaiRows,
            int numFaiColumns, int numMiscRows)
        {
            XSSFWorkbook wb;
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                wb = new XSSFWorkbook(file);
            }
            // Delete unformatted file
            File.Delete(path);

            var ws = wb.GetSheetAt(0);

          
                for (int j = 0; j < numFaiColumns; j++)
                {
                    var columnIndex = GetExcelColumnName(j+1);
                    var maxFormula = $"${columnIndex}${maxRowIndex + 1}";
                    var minFormula = $"${columnIndex}${minRowIndex + 1}";

                    ISheetConditionalFormatting sheetCF = ws.SheetConditionalFormatting;

                    // Max rule
                    IConditionalFormattingRule maxRule =
                        sheetCF.CreateConditionalFormattingRule(ComparisonOperator.GreaterThan, maxFormula);
                    IPatternFormatting fill1 = maxRule.CreatePatternFormatting();
                    fill1.FillBackgroundColor = (IndexedColors.Red.Index);
                    fill1.FillPattern = FillPattern.SolidForeground;

                    // min rule
                    IConditionalFormattingRule minRule =
                        sheetCF.CreateConditionalFormattingRule(ComparisonOperator.LessThan, minFormula);
                    IPatternFormatting fill2 = minRule.CreatePatternFormatting();
                    fill2.FillBackgroundColor = (IndexedColors.Blue.Index);
                    fill2.FillPattern = FillPattern.SolidForeground;

                    CellRangeAddress[] regions =
                    {
                        CellRangeAddress.ValueOf($"{columnIndex}{numMiscRows+1}:{columnIndex}{numFaiRows+numMiscRows}")
                    };

                    sheetCF.AddConditionalFormatting(regions, maxRule, minRule);
                }
            
            
                using (var fs = File.Create(path))
                {
                    wb.Write(fs);
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