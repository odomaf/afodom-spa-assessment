if (args.Length == 0)
{
    WriteError("No input provided. Please run the program with a folder path or .xlsx file path.");
    WriteError("Example: SubawardReader \"C:\\path\\to\\folder\"");
    return 1;
}

string input = args[0];
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

return 0;

static void WriteError(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine(message);
    Console.ResetColor();
}
