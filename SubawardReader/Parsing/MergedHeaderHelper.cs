// MergedHeaderHelper: Utilities for detecting and describing merged header cells in worksheets.
using System;
using ClosedXML.Excel;

namespace SubawardReader.Parsing
{
    // Represents details about a merged header cell, including its range, text, and span.
    public class MergedHeaderInfo
    {
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public int Row { get; set; }
        public int ColSpan { get; set; }
        public int RowSpan { get; set; }
        public string? HeaderText { get; set; }
        public IXLRange? MergedRange { get; set; }
    }

    public static class MergedHeaderHelper
    {
        // Finds the first merged header cell containing the specified text in the given rows above the anchor.
        // Returns a MergedHeaderInfo with range and span details, or null if not found.
        public static MergedHeaderInfo? FindMergedHeaderRange(IXLWorksheet worksheet, int headerRowNumber, string headerText = "Total", int rowsToCheck = 2)
        {
            for (int offset = 0; offset < rowsToCheck; offset++)
            {
                int rowNum = headerRowNumber - offset;
                if (rowNum < 1) continue;
                var row = worksheet.Row(rowNum);
                foreach (var cell in row.CellsUsed())
                {
                    if (cell.GetString().Contains(headerText, StringComparison.OrdinalIgnoreCase))
                    {
                        var mergedRange = cell.MergedRange();
                        if (mergedRange != null && mergedRange.RowCount() == 1)
                        {
                            return new MergedHeaderInfo
                            {
                                StartColumn = mergedRange.FirstColumn().ColumnNumber(),
                                EndColumn = mergedRange.LastColumn().ColumnNumber(),
                                Row = mergedRange.FirstRow().RowNumber(),
                                ColSpan = mergedRange.ColumnCount(),
                                RowSpan = mergedRange.RowCount(),
                                HeaderText = cell.GetString(),
                                MergedRange = mergedRange
                            };
                        }
                        else
                        {
                            // Not merged, treat as single cell
                            return new MergedHeaderInfo
                            {
                                StartColumn = cell.Address.ColumnNumber,
                                EndColumn = cell.Address.ColumnNumber,
                                Row = cell.Address.RowNumber,
                                ColSpan = 1,
                                RowSpan = 1,
                                HeaderText = cell.GetString(),
                                MergedRange = null
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}
