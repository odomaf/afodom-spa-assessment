// WorksheetHelper: Utility methods for analyzing worksheet structure and locating key anchors/headers.
using ClosedXML.Excel;

namespace SubawardReader.Parsing
{
    public static class WorksheetHelper
    {
        // Finds the header row by locating the anchor row with column 1 'A.' and column 2 'Senior Personnel'.
        // Returns the row above the anchor as the header row.
        // Throws if the anchor is not found.
        public static int FindHeaderRowNumber(IXLWorksheet worksheet, string filePath)
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

        // Returns true if the given cell is an anchor row: column 1 'A.' and column 2 'Senior Personnel' (case-insensitive).
        public static bool IsSeniorPersonnelAnchor(IXLWorksheet worksheet, IXLCell columnOneCell)
        {
            return columnOneCell.GetString().Trim() == "A." &&
                   string.Equals(
                       worksheet.Cell(columnOneCell.Address.RowNumber, 2).GetString().Trim(),
                       "Senior Personnel",
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}
