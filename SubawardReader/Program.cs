// Program: Entry point for SubawardReader. Handles input validation, file iteration, and output formatting.
using SubawardReader.Parsing;
using SubawardReader;
using Spectre.Console;


// Validate command-line arguments and input path.
if (args.Length == 0)
{
    ConsoleFormatter.PrintError("No input provided. Please run the program with a full folder path or .xlsx file path.");
    ConsoleFormatter.PrintError("Example: SubawardReader \"C:\\path\\to\\folder\"");
    return 1;
}



string input = args[0];
// Accept both relative and absolute paths; resolve to absolute
string resolvedInput;
try
{
    resolvedInput = Path.GetFullPath(input);
}
catch (Exception ex)
{
    ConsoleFormatter.PrintError($"Input path '{input}' is invalid: {ex.Message}");
    return 1;
}


// Iterate over each .xlsx file and process subaward data.
IEnumerable<string> filePaths;

if (Directory.Exists(resolvedInput))
{
    filePaths = Directory.EnumerateFiles(resolvedInput, "*.xlsx");
}
else if (File.Exists(resolvedInput))
{
    if (!resolvedInput.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
    {
        ConsoleFormatter.PrintError($"'{resolvedInput}' is not a supported file type. Please provide an .xlsx file or a folder containing .xlsx files.");
        return 1;
    }
    filePaths = new[] { resolvedInput };
}
else
{
    ConsoleFormatter.PrintError($"Could not find '{resolvedInput}'. Please check the path and try again.");
    return 1;
}


foreach (var filePath in filePaths)
{
    // Print a blank line, then filename in yellow
    Console.WriteLine();
    ConsoleFormatter.PrintFileName(Path.GetFileName(filePath));

    var result = SubawardReader.Parsing.SubawardParser.ParseWithMetadata(filePath);

    if (result.Rows.Count == 0)
    {
        AnsiConsole.MarkupLine("  [dim]No subaward rows found.[/]");
        continue;
    }

    // Output logic based on merged or single Total header
    if (result.IsMergedTotalHeader && result.Subheadings != null)
    {
        foreach (var row in result.Rows)
        {
            ConsoleFormatter.PrintRecipientName(row.RecipientName);
            foreach (var subheading in result.Subheadings)
            {
                decimal value = row.Amounts.TryGetValueOrDefault(subheading, 0);
                ConsoleFormatter.PrintSubheading(subheading, value);
            }
        }
    }
    else if (result.TotalColumnOrder.Count == 1)
    {
        string colName = result.TotalColumnOrder[0];
        foreach (var row in result.Rows)
        {
            decimal value = row.Amounts.TryGetValueOrDefault(colName, 0);
            AnsiConsole.MarkupLine($"  {ConsoleFormatter.RecipientName(row.RecipientName)}: {ConsoleFormatter.Money(value)}");
        }
    }
    else
    {
        foreach (var row in result.Rows)
        {
            Console.WriteLine($"  {row.RecipientName}");
            foreach (var colName in result.TotalColumnOrder)
            {
                decimal value = row.Amounts.TryGetValueOrDefault(colName, 0);
                string formatted = value.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                Console.WriteLine($"    {colName}: {formatted}");
            }
        }
    }
}

return 0;


