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


foreach (var filePath in filePaths)
{
    Console.WriteLine($"\n{Path.GetFileName(filePath)}");

    var result = SubawardReader.Parsing.SubawardParser.ParseWithMetadata(filePath);

    if (result.Rows.Count == 0)
    {
        Console.WriteLine("  No subaward rows found.");
        continue;
    }

    // Output logic based on merged or single Total header
    if (result.IsMergedTotalHeader && result.Subheadings != null)
    {
        foreach (var row in result.Rows)
        {
            Console.WriteLine($"  {row.RecipientName}");
            foreach (var subheading in result.Subheadings)
            {
                decimal value = 0;
                if (!row.Amounts.TryGetValue(subheading, out value))
                {
                    value = 0;
                }
                Console.WriteLine($"    {subheading}: {value}");
            }
        }
    }
    else if (result.TotalColumnOrder.Count == 1)
    {
        string colName = result.TotalColumnOrder[0];
        foreach (var row in result.Rows)
        {
            decimal value = 0;
            if (!row.Amounts.TryGetValue(colName, out value))
            {
                value = 0;
            }
            Console.WriteLine($"  {row.RecipientName}: {value}");
        }
    }
    else
    {
        foreach (var row in result.Rows)
        {
            Console.WriteLine($"  {row.RecipientName}");
            foreach (var colName in result.TotalColumnOrder)
            {
                decimal value = 0;
                if (!row.Amounts.TryGetValue(colName, out value))
                {
                    value = 0;
                }
                Console.WriteLine($"    {colName}: {value}");
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
