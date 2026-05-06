# Gate 3: Parsing Implementation

Goal: Extract subaward rows from each workbook correctly.

## Checklist

- [x] Resolve input path as either directory or single file
- [x] Enumerate `.xlsx` files from input directory when directory mode is used
- [x] Select only the specified `.xlsx` file when file mode is used
- [ ] Open first worksheet per workbook
- [ ] Discover `Total` column dynamically by scanning headers
- [ ] Detect rows where column A starts with `Subaward:`
- [ ] Parse recipient name from the same cell text
- [ ] Parse amount from the same row in the discovered `Total` column

## Implementation Notes

### Resolve input path

Input path comes from `args[0]` in `Program.cs`. Three cases are handled:

- **Directory**: `Directory.Exists` — enumerate all `.xlsx` files with `Directory.EnumerateFiles(input, "*.xlsx")`
- **Single file**: `File.Exists` — validate `.xlsx` extension, then wrap in a single-element array
- **Invalid**: path doesn't exist or wrong extension — call `WriteError` and exit with code 1

Output is `IEnumerable<string> filePaths`, passed to the parser one path at a time. All error output goes through `WriteError` (red text to `Console.Error`).

### Enumerate directory files

Implemented as part of path resolution above — `Directory.EnumerateFiles(input, "*.xlsx")` handles this directly.

### Select single file

Implemented as part of path resolution above — single-file mode wraps the validated path in `new[] { input }`.

### Open first worksheet

### Discover Total column

### Detect Subaward rows

### Parse recipient name

### Parse amount

## Exit Criteria

- [ ] Parser returns expected extracted rows for all example files
