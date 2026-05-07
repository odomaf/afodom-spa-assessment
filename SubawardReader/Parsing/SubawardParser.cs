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
        int totalColumnNumber = FindTotalColumnNumber(worksheet, headerRowNumber, filePath);

        var rows = ExtractSubawardRows(worksheet, headerRowNumber, totalColumnNumber, filePath).ToList();
        return rows;
    }

    private static IEnumerable<SubawardRow> ExtractSubawardRows(IXLWorksheet worksheet, int headerRowNumber, int totalColumnNumber, string filePath)
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

            // Column C: recipient name
            string recipientName = worksheet.Cell(cell.Address.RowNumber, 3).GetString().Trim();
            if (string.IsNullOrWhiteSpace(recipientName))
                continue;

            decimal amount = worksheet.Cell(cell.Address.RowNumber, totalColumnNumber).GetValue<decimal>();

            yield return new SubawardRow(fileName, recipientName, amount);
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

    private static int FindTotalColumnNumber(IXLWorksheet worksheet, int headerRowNumber, string filePath)
    {
        var headerRow = worksheet.Row(headerRowNumber);

        foreach (var cell in headerRow.CellsUsed())
        {
            if (cell.GetString().Contains("Total", StringComparison.OrdinalIgnoreCase))
            {
                return cell.Address.ColumnNumber;
            }
        }

        throw new InvalidOperationException($"No 'Total' column found in header row of '{filePath}'.");
    }
}
