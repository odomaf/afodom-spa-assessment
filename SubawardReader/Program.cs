

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
    Console.WriteLine($"\nFile: {Path.GetFileName(filePath)}");
    var rows = parser.Parse(filePath).ToList();

    if (rows.Count == 0)
    {
        Console.WriteLine("  No subaward rows found.");
        continue;
    }

    foreach (var row in rows)
    {
        Console.WriteLine($"  Recipient: {row.RecipientName}");
        foreach (var amount in row.Amounts)
        {
            Console.WriteLine($"    {amount.Key}: {amount.Value}");
        }
    }
}

return 0;

static void WriteError(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(message);
    Console.ResetColor();
}
