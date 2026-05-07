// Program: Entry point for SubawardReader. Handles input validation, file iteration, and output formatting.
using SubawardReader.Parsing;

// Validate command-line arguments and input path.
if (args.Length == 0)
{
    WriteError("No input provided. Please run the program with a full folder path or .xlsx file path.");
    WriteError("Example: SubawardReader \"C:\\path\\to\\folder\"");
    return 1;
}

string input = args[0];
if (!Path.IsPathRooted(input))
{
    WriteError($"Input path '{input}' is not a full path. Please provide the complete path starting with a drive letter, such as C:\\path\\to\\folder.");
    return 1;
}


// Iterate over each .xlsx file and process subaward data.
IEnumerable<string> filePaths;

if (Directory.Exists(input))
{
    filePaths = Directory.EnumerateFiles(input, "*.xlsx");
}
else if (File.Exists(input))
{
    if (!input.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
    {
        WriteError($"'{input}' is not a supported file type. Please provide an .xlsx file or a folder containing .xlsx files.");
        return 1;
    }
    filePaths = new[] { input };
}
else
{
    WriteError($"Could not find '{input}'. Please check the path and try again.");
    return 1;
}


var parser = new SubawardReader.Parsing.SubawardParser();



foreach (var filePath in filePaths)
{
    Console.WriteLine($"\n{Path.GetFileName(filePath)}");

    // Use new method to get col span
    (List<SubawardReader.Models.SubawardRow> rows, List<string> totalColumnOrder, int totalHeaderRowSpan, int totalHeaderColSpan) = parser.ParseWithColumnOrderWithRowSpanAndColSpan(filePath);

    // Output number and names of Total columns
    Console.WriteLine($"  'Total' header column span (merged only): {totalHeaderColSpan}");

    if (rows.Count == 0)
    {
        Console.WriteLine("  No subaward rows found.");
        continue;
    }

    // If a merged 'Total' header is detected, extract its range and span.
    bool useMergedTotal = totalHeaderColSpan > 1;
    int headerRowNumber = 0;
    int totalHeaderColStart = 0;
    int totalHeaderColEnd = 0;
    if (useMergedTotal)
    {
        (int hdrRow, int colStart, int colEnd, int colSpan, string debugInfo) = SubawardReader.Parsing.TotalHeaderHelper.GetMergedTotalHeaderInfo(filePath);
        headerRowNumber = hdrRow;
        totalHeaderColStart = colStart;
        totalHeaderColEnd = colEnd;
        Console.WriteLine($"    {debugInfo}");
        if (headerRowNumber == 0 || totalHeaderColStart == 0)
        {
            Console.WriteLine("    Error: Could not determine merged 'Total' header range. The header row or column start is invalid.");
        }
    }

    // Output subaward row data, using subheadings if merged header is present.
    foreach (var row in rows)
    {
        Console.WriteLine($"  {row.RecipientName}");
        if (useMergedTotal && headerRowNumber > 0 && totalHeaderColStart > 0)
        {
            var worksheet = new ClosedXML.Excel.XLWorkbook(filePath).Worksheets.First();
            var (amounts, subheadings) = SubawardReader.Parsing.SubawardParser.ExtractAmountsWithSubheadings(
                worksheet,
                worksheet.CellsUsed().First(c => c.GetString().Contains(row.RecipientName)).Address.RowNumber,
                headerRowNumber,
                totalHeaderColStart,
                totalHeaderColSpan);
            foreach (var subheading in subheadings)
            {
                decimal value = 0;
                if (!amounts.TryGetValue(subheading, out value))
                {
                    value = 0;
                }
                Console.WriteLine($"    {subheading}: {value}");
            }
        }
        else
        {
            foreach (var colName in totalColumnOrder)
            {
                string output;
                if (row.Amounts.TryGetValue(colName, out var value))
                {
                    output = value.ToString();
                }
                else
                {
                    output = "0";
                }
                Console.WriteLine($"    {colName}: {output}");
            }
        }
    }
}

return 0;

// Writes error messages in red to the console.
static void WriteError(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(message);
    Console.ResetColor();
}
