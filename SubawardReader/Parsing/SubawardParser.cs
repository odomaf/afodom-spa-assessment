// SubawardParser: Extracts subaward rows and totals from Excel workbooks.
// Handles both same-cell and adjacent-cell recipient formats, and supports merged/multi-column totals.
using ClosedXML.Excel;
using SubawardReader.Parsing;
using SubawardReader.Models;

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
            if (amountCell.DataType == XLDataType.Number || decimal.TryParse(amountCell.GetString(), out value))
            {
                value = amountCell.GetValue<decimal>();
            }
            amounts[subheading] = value;
        }
        return (amounts, subheadings);
    }
    // Main parser entry: returns all subaward rows, total column order, and header info for a file.
    // Throws if a merged 'Total' header is not found above the anchor row.
    public (List<SubawardRow> Rows, List<string> TotalColumnOrder, int TotalHeaderRowSpan, int TotalHeaderColSpan) ParseWithColumnOrderWithRowSpanAndColSpan(string filePath)
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
    public (List<SubawardRow> Rows, List<string> TotalColumnOrder) ParseWithColumnOrder(string filePath)
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

        // Column B: subaward marker labels
        foreach (var cell in worksheet.Column(2).CellsUsed())
        {
            if (cell.Address.RowNumber <= headerRowNumber)
                continue;

            if (cell.Address.RowNumber < gSectionRowNumber)
                continue;

            string cellText = cell.GetString();
            if (!cellText.StartsWith("Subaward:", StringComparison.OrdinalIgnoreCase))
                continue;


            // Prefer extracting recipient from the same cell as 'Subaward:' for newer formats.
            // Fallback to adjacent cell (column C) for legacy or alternate formats.
            string recipientName = cell.GetString().Substring("Subaward:".Length).Trim();
            if (string.IsNullOrWhiteSpace(recipientName))
            {
                recipientName = worksheet.Cell(cell.Address.RowNumber, 3).GetString().Trim();
            }
            if (string.IsNullOrWhiteSpace(recipientName))
                continue;

            var amounts = new Dictionary<string, decimal>();
            foreach (var kvp in totalColumns)
            {
                int col = kvp.Key;
                string heading = kvp.Value;
                decimal value = 0;
                var amountCell = worksheet.Cell(cell.Address.RowNumber, col);
                if (amountCell.DataType == XLDataType.Number || decimal.TryParse(amountCell.GetString(), out value))
                {
                    value = amountCell.GetValue<decimal>();
                }
                // Always add the column, using 0 for missing/invalid values
                amounts[heading] = value;
            }

            // Only skip if all amounts are zero (no data in any total column)
            if (amounts.Values.All(v => v == 0))
                continue;

            yield return new SubawardRow(fileName, recipientName, amounts);
        }
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
    }
}
