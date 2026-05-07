using ClosedXML.Excel;
using SubawardReader.Models;

namespace SubawardReader.Parsing;

public class SubawardParser : ISubawardParser
{

    public IEnumerable<SubawardRow> Parse(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.First();
        int headerRowNumber = FindHeaderRowNumber(worksheet, filePath);
        var totalColumns = FindTotalColumnNumbers(worksheet, headerRowNumber, filePath);

        var rows = ExtractSubawardRows(worksheet, headerRowNumber, totalColumns, filePath).ToList();
        return rows;
    }

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


            // Try to extract recipient from the same cell as 'Subaward:'
            string recipientName = cell.GetString().Substring("Subaward:".Length).Trim();
            // If not present, fall back to adjacent cell (column C)
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
                    amounts[heading] = value;
                }
            }

            if (amounts.Count == 0)
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

    private static int FindHeaderRowNumber(IXLWorksheet worksheet, string filePath)
    {
        foreach (var cell in worksheet.Column(1).CellsUsed())
        {
            if (IsSeniorPersonnelAnchor(worksheet, cell))
            {
                // Header row is the row immediately above the anchor
                return cell.Address.RowNumber - 1;
            }
        }

        throw new InvalidOperationException($"Could not determine header row in '{filePath}' because the anchor row was not found. Expected an anchor row with column 1 'A.' and column 2 'Senior Personnel'.");
    }

    private static bool IsSeniorPersonnelAnchor(IXLWorksheet worksheet, IXLCell columnOneCell)
    {
        return columnOneCell.GetString().Trim() == "A." &&
               string.Equals(
                   worksheet.Cell(columnOneCell.Address.RowNumber, 2).GetString().Trim(),
                   "Senior Personnel",
                   StringComparison.OrdinalIgnoreCase);
    }


    // Returns a dictionary: column index -> header text (e.g., 'Total', 'Sponsor Share', etc.)
    private static Dictionary<int, string> FindTotalColumnNumbers(IXLWorksheet worksheet, int headerRowNumber, string filePath)
    {
        var totalColumns = new Dictionary<int, string>();

        // Check both the row directly above the anchor and the row above that
        for (int offset = 0; offset <= 1; offset++)
        {
            int rowNum = headerRowNumber - offset;
            if (rowNum < 1) continue;
            var row = worksheet.Row(rowNum);
            foreach (var cell in row.CellsUsed())
            {
                if (cell.GetString().Contains("Total", StringComparison.OrdinalIgnoreCase))
                {
                    totalColumns[cell.Address.ColumnNumber] = cell.GetString();
                }
            }
        }

        if (totalColumns.Count == 0)
        {
            throw new InvalidOperationException($"No 'Total' column found in the two header rows above the anchor in '{filePath}'.");
        }

        return totalColumns;
    }
}
