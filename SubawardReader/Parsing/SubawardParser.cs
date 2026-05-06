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

        _ = totalColumnNumber;

        throw new NotImplementedException();
    }

    private static int FindHeaderRowNumber(IXLWorksheet worksheet, string filePath)
    {
        foreach (var cell in worksheet.Column(1).CellsUsed())
        {
            if (IsSeniorPersonnelAnchor(worksheet, cell))
            {
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
