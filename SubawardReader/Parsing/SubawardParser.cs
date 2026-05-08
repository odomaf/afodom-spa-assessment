// SubawardParser: Extracts subaward rows and totals from Excel workbooks.
// Handles both same-cell and adjacent-cell recipient formats, and supports merged/multi-column totals.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using SubawardReader.Models;
using SubawardReader.Parsing;

namespace SubawardReader.Parsing
{
    public class SubawardParser : ISubawardParser
    {
        // Extracts subaward amounts and subheadings for a given recipient row.
        // Always returns a value for each subheading; missing or malformed cells are treated as 0.
        public static (Dictionary<string, decimal> Amounts, List<string> Subheadings) ExtractAmountsWithSubheadings(
            IXLWorksheet worksheet,
            int recipientRow,
            int totalHeaderRow,
            int totalHeaderColStart,
            int totalHeaderColSpan)
        {
            var amounts = new Dictionary<string, decimal>();
            var subheadings = new List<string>();
            int subheadingRow = totalHeaderRow + 1;
            for (int i = 0; i < totalHeaderColSpan; i++)
            {
                int col = totalHeaderColStart + i;
                var subheadingCell = worksheet.Cell(subheadingRow, col);
                string subheading = subheadingCell.GetString().Trim();
                if (string.IsNullOrEmpty(subheading))
                {
                    subheading = $"{subheadingCell.Address.ColumnLetter}:{subheadingCell.Address.RowNumber}";
                }
                subheadings.Add(subheading);
                decimal value = 0;
                var amountCell = worksheet.Cell(recipientRow, col);
                string cellRaw = amountCell.GetString();
                if (amountCell.DataType == XLDataType.Number || decimal.TryParse(cellRaw, out value))
                {
                    value = amountCell.GetValue<decimal>();
                }
                amounts[subheading] = value;
            }
            return (amounts, subheadings);
        }

            // Main parser entry: returns all subaward rows, total column order, and header info for a file.
            // Throws if a merged 'Total' header is not found above the anchor row.
            public static (List<SubawardRow> Rows, List<string> TotalColumnOrder, int TotalHeaderRowSpan, int TotalHeaderColSpan) ParseWithColumnOrderWithRowSpanAndColSpan(string filePath)
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheets.First();
                int headerRowNumber = WorksheetHelper.FindHeaderRowNumber(worksheet, filePath);
                var mergedHeader = MergedHeaderHelper.FindMergedHeaderRange(worksheet, headerRowNumber, "Total", 2);
                if (mergedHeader == null)
                    throw new InvalidOperationException($"No merged 'Total' header found in the two header rows above the anchor in '{filePath}'.");

                // Build totalColumns dictionary for compatibility
                var totalColumns = new Dictionary<int, string>();
                for (int col = mergedHeader.StartColumn; col <= mergedHeader.EndColumn; col++)
                {
                    var cell = worksheet.Cell(mergedHeader.Row, col);
                    string text = cell.GetString();
                    if (string.IsNullOrWhiteSpace(text))
                        text = $"{cell.Address.ColumnLetter}:{cell.Address.RowNumber}";
                    totalColumns[col] = text;
                }
                var rows = ExtractSubawardRows(worksheet, headerRowNumber, totalColumns, filePath).ToList();
                var totalColumnOrder = totalColumns.Values.ToList();
                return (rows, totalColumnOrder, mergedHeader.RowSpan, mergedHeader.ColSpan);
            }

            // For compatibility with existing code: returns rows and total column order only.
            public static (List<SubawardRow> Rows, List<string> TotalColumnOrder) ParseWithColumnOrder(string filePath)
            {
                (List<SubawardRow> rows, List<string> totalColumnOrder, int _, int _) = ParseWithColumnOrderWithRowSpanAndColSpan(filePath);
                return (rows, totalColumnOrder);
            }

            // Legacy interface for compatibility: returns only the list of subaward rows.
            public IEnumerable<SubawardRow> Parse(string filePath)
            {
                (List<SubawardRow> rows, List<string> _, int _, int _) = ParseWithColumnOrderWithRowSpanAndColSpan(filePath);
                return rows;
            }

            // Iterates all candidate subaward rows in the worksheet, starting after the header and G. section anchor.
            // Extracts recipient from same cell as 'Subaward:' if present, otherwise from adjacent cell (column C).
            // Skips rows where all amounts are zero.
            private static IEnumerable<SubawardRow> ExtractSubawardRows(
                IXLWorksheet worksheet,
                int headerRowNumber,
                Dictionary<int, string> totalColumns,
                string filePath)
            {
                string fileName = Path.GetFileName(filePath);
                int gSectionRowNumber = FindGSectionRowNumber(worksheet);

                foreach (var cell in worksheet.Column(2).CellsUsed())
                {
                    if (cell.Address.RowNumber <= headerRowNumber)
                        continue;
                    if (cell.Address.RowNumber < gSectionRowNumber)
                        continue;

                    if (!IsRecipientRow(cell, worksheet))
                        continue;

                    string recipientName = ExtractRecipientName(cell, worksheet);
                    if (string.IsNullOrWhiteSpace(recipientName))
                        continue;

                    var amounts = new Dictionary<string, decimal>();
                    foreach (var kvp in totalColumns)
                    {
                        int col = kvp.Key;
                        string heading = kvp.Value;
                        decimal value = 0;
                        var amountCell = worksheet.Cell(cell.Address.RowNumber, col);
                        string cellRaw = amountCell.GetString();
                        if (amountCell.DataType == XLDataType.Number || decimal.TryParse(cellRaw, out value))
                        {
                            value = amountCell.GetValue<decimal>();
                        }
                        amounts[heading] = value;
                    }

                    if (amounts.Values.All(v => v == 0))
                        continue;

                    yield return new SubawardRow(fileName, recipientName, amounts);
                }
            }

            // Determines if a worksheet cell marks the start of a recipient row ("Subaward:" in column B)
            private static bool IsRecipientRow(IXLCell cell, IXLWorksheet worksheet)
            {
                string cellText = cell.GetString();
                return cellText.StartsWith("Subaward:", StringComparison.OrdinalIgnoreCase);
            }

            // Extracts the recipient name from a recipient row, handling both same-cell and adjacent-cell formats
            private static string ExtractRecipientName(IXLCell cell, IXLWorksheet worksheet)
            {
                string recipientName = cell.GetString().Substring("Subaward:".Length).Trim();
                if (string.IsNullOrWhiteSpace(recipientName))
                {
                    recipientName = worksheet.Cell(cell.Address.RowNumber, 3).GetString().Trim();
                }
                return recipientName;
            }

            private static int FindGSectionRowNumber(IXLWorksheet worksheet)
            {
                foreach (var cell in worksheet.Column(1).CellsUsed())
                {
                    if (cell.GetString().Trim() == "G." &&
                        string.Equals(
                            worksheet.Cell(cell.Address.RowNumber, 2).GetString().Trim(),
                            "Other Direct Costs",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return cell.Address.RowNumber;
                    }
                }

                // No G. anchor found; skip section filtering
                return 0;
            }

            // Finds all columns in the header row(s) containing 'Total', supporting merged/multi-column headers.
            // Returns a dictionary: column index -> header text (e.g., 'Total', 'Sponsor Share', etc.), row span, and total header col span.
            private static (Dictionary<int, string> totalColumns, int rowSpan, int totalHeaderColSpan) FindTotalColumnNumbersWithRowSpanAndColSpan(IXLWorksheet worksheet, int headerRowNumber, string filePath)
            {
                var totalColumns = new Dictionary<int, string>();
                int firstRowWithTotal = -1;
                int lastRowWithTotal = -1;
                int totalHeaderColSpan = 1;

                // Check both the row directly above the anchor and the row above that
                for (int offset = 0; offset <= 1; offset++)
                {
                    int rowNum = headerRowNumber - offset;
                    if (rowNum < 1) continue;
                    var row = worksheet.Row(rowNum);
                    bool foundInThisRow = false;
                    foreach (var cell in row.CellsUsed())
                    {
                        if (cell.GetString().Contains("Total", StringComparison.OrdinalIgnoreCase))
                        {
                            totalColumns[cell.Address.ColumnNumber] = cell.GetString();
                            foundInThisRow = true;
                            // Only treat as merged header if the cell is part of a single-row merged range.
                            // This avoids false positives from multi-row merges or accidental merges.
                            var mergedRange = cell.MergedRange();
                            if (mergedRange != null && mergedRange.RowCount() == 1)
                            {
                                int span = mergedRange.ColumnCount();
                                if (span > totalHeaderColSpan) totalHeaderColSpan = span;
                            }
                        }
                    }
                    if (foundInThisRow)
                    {
                        if (firstRowWithTotal == -1) firstRowWithTotal = rowNum;
                        lastRowWithTotal = rowNum;
                    }
                }

                if (totalColumns.Count == 0)
                {
                    throw new InvalidOperationException($"No 'Total' column found in the two header rows above the anchor in '{filePath}'.");
                }

                int rowSpan = (firstRowWithTotal != -1 && lastRowWithTotal != -1) ? (firstRowWithTotal - lastRowWithTotal + 1) : 1;
                return (totalColumns, rowSpan, totalHeaderColSpan);
            }
        // Parses a subaward worksheet and returns output metadata for formatting.
        public static SubawardParseResult ParseWithMetadata(string filePath)
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheets.First();
            int headerRowNumber = WorksheetHelper.FindHeaderRowNumber(worksheet, filePath);

            // Try merged header first
            var mergedHeader = MergedHeaderHelper.FindMergedHeaderRange(worksheet, headerRowNumber, "Total", 2);
            if (mergedHeader != null && mergedHeader.ColSpan > 1)
            {
                // Merged/multi-column header: use ExtractAmountsWithSubheadings for each recipient row
                var rows = new List<Models.SubawardRow>();
                List<string>? subheadings = null;
                int subheadingRow = mergedHeader.Row + 1;

                // Find all candidate recipient rows (column B, after header and G. section anchor)
                int gSectionRowNumber = FindGSectionRowNumber(worksheet);
                foreach (var cell in worksheet.Column(2).CellsUsed())
                {
                    if (cell.Address.RowNumber <= headerRowNumber)
                        continue;
                    if (cell.Address.RowNumber < gSectionRowNumber)
                        continue;
                    string cellText = cell.GetString();
                    if (!cellText.StartsWith("Subaward:", StringComparison.OrdinalIgnoreCase))
                        continue;
                    string recipientName = cell.GetString().Substring("Subaward:".Length).Trim();
                    if (string.IsNullOrWhiteSpace(recipientName))
                    {
                        recipientName = worksheet.Cell(cell.Address.RowNumber, 3).GetString().Trim();
                    }
                    if (string.IsNullOrWhiteSpace(recipientName))
                        continue;

                    // Use ExtractAmountsWithSubheadings for this recipient row
                    var (amounts, extractedSubheadings) = ExtractAmountsWithSubheadings(
                        worksheet,
                        cell.Address.RowNumber,
                        mergedHeader.Row,
                        mergedHeader.StartColumn,
                        mergedHeader.ColSpan);
                    if (subheadings == null)
                        subheadings = extractedSubheadings;
                    // Only skip if all amounts are zero
                    if (amounts.Values.All(v => v == 0))
                        continue;
                    rows.Add(new Models.SubawardRow(System.IO.Path.GetFileName(filePath), recipientName, amounts));
                }
                return new SubawardParseResult
                {
                    Rows = rows,
                    TotalColumnOrder = subheadings ?? new List<string>(),
                    IsMergedTotalHeader = true,
                    Subheadings = subheadings,
                    DebugInfo = $"Merged 'Total' header at {ClosedXML.Excel.XLHelper.GetColumnLetterFromNumber(mergedHeader.StartColumn)}{mergedHeader.Row} spanning {mergedHeader.ColSpan} columns"
                };
            }
            else
            {
                // Fallback: single or non-merged 'Total' column(s)
                var result = FindTotalColumnNumbersWithRowSpanAndColSpan(worksheet, headerRowNumber, filePath);
                var totalColumns = result.totalColumns;
                int totalHeaderColSpan = result.totalHeaderColSpan;
                var rows = ExtractSubawardRows(worksheet, headerRowNumber, totalColumns, filePath).ToList();
                return new SubawardParseResult
                {
                    Rows = rows,
                    TotalColumnOrder = totalColumns.Values.ToList(),
                    IsMergedTotalHeader = false,
                    Subheadings = null,
                    DebugInfo = totalHeaderColSpan == 1 ? "Single 'Total' column detected" : "Multiple non-merged 'Total' columns detected"
                };
            }
        }
    }
}

