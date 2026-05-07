// TotalHeaderHelper: Centralizes logic for extracting merged 'Total' header information from worksheets.
using ClosedXML.Excel;

namespace SubawardReader.Parsing
{
    public static class TotalHeaderHelper
    {
        // Returns merged 'Total' header info for a given file.
        // Uses WorksheetHelper to find the header row and MergedHeaderHelper to locate the merged 'Total' header.
        // Returns (headerRowNumber, totalHeaderColStart, totalHeaderColEnd, colSpan, debugInfo).
        // Returns zeros and an error message if the header is not found.
        public static (int headerRowNumber, int totalHeaderColStart, int totalHeaderColEnd, int colSpan, string debugInfo) GetMergedTotalHeaderInfo(string filePath)
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheets.First();
            int headerRowNumber = WorksheetHelper.FindHeaderRowNumber(worksheet, filePath);
            var mergedHeader = MergedHeaderHelper.FindMergedHeaderRange(worksheet, headerRowNumber, "Total", 2);
            if (mergedHeader != null)
            {
                int start = mergedHeader.StartColumn;
                int end = mergedHeader.EndColumn;
                int span = mergedHeader.ColSpan;
                string debug = $"[DEBUG] Merged 'Total' header at {ClosedXML.Excel.XLHelper.GetColumnLetterFromNumber(start)}{mergedHeader.Row} spanning {span} columns";
                return (mergedHeader.Row, start, end, span, debug);
            }
            else
            {
                return (0, 0, 0, 0, "Error: Could not determine merged 'Total' header range. The header row or column start is invalid.");
            }
        }
    }
}
