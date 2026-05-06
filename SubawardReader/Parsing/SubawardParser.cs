using ClosedXML.Excel;
using SubawardReader.Models;

namespace SubawardReader.Parsing;

public class SubawardParser : ISubawardParser
{
    public IEnumerable<SubawardRow> Parse(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.First();

        // Locate the header row by finding the "A." section marker in column 1,
        // confirmed by "Senior Personnel" in column 2. The header row is one row above.
        int? headerRowNumber = null;
        foreach (var cell in worksheet.Column(1).CellsUsed())
        {
            if (cell.GetString().Trim() == "A." &&
                string.Equals(
                    worksheet.Cell(cell.Address.RowNumber, 2).GetString().Trim(),
                    "Senior Personnel",
                    StringComparison.OrdinalIgnoreCase))
            {
                headerRowNumber = cell.Address.RowNumber - 1;
                break;
            }
        }

        if (headerRowNumber is null)
            throw new InvalidOperationException($"Could not determine header row in '{filePath}' because the anchor row was not found. Expected an anchor row with column 1 'A.' and column 2 'Senior Personnel'.");

        var headerRow = worksheet.Row(headerRowNumber.Value);
        int? totalColumn = null;

        foreach (var cell in headerRow.CellsUsed())
        {
            if (cell.GetString().Contains("Total", StringComparison.OrdinalIgnoreCase))
            {
                totalColumn = cell.Address.ColumnNumber;
                break;
            }
        }

        if (totalColumn is null)
            throw new InvalidOperationException($"No 'Total' column found in header row of '{filePath}'.");

        throw new NotImplementedException();
    }
}
