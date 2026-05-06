# Gate 3: Parsing Implementation

Goal: Extract subaward rows from each workbook correctly.

## Checklist

- [x] Resolve input path as either directory or single file
- [x] Enumerate `.xlsx` files from input directory when directory mode is used
- [x] Select only the specified `.xlsx` file when file mode is used
- [x] Open first worksheet per workbook
- [x] Discover `Total` column dynamically by scanning headers
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

Parser now opens each workbook with `XLWorkbook(filePath)` and selects the first worksheet with `workbook.Worksheets.First()`.

### Discover Total column

Header row is discovered structurally, not by fixed row number:

- Find an anchor row where column 1 is exactly `A.` and column 2 is exactly `Senior Personnel` (trimmed, case-insensitive)
- Treat the row immediately above that anchor as the header row
- Scan that header row for a cell containing `Total` (case-insensitive)

If either anchor row or `Total` column is missing, parser throws a clear `InvalidOperationException` with file-specific context.

Unit tests now cover:

- Expected structure reaches current `NotImplementedException` boundary
- Missing anchor row fails with anchor/header determination error
- False anchor (`A.` with wrong column 2) fails with anchor/header determination error
- Missing `Total` header fails with `Total` column error
- Trimmed and case-insensitive anchor values still resolve correctly
- Case-insensitive `Total` header (e.g. `PROJECT TOTAL`) still resolves correctly

Parser refactored into private helpers to reduce method complexity:

- `FindHeaderRowNumber` — locates the header row via anchor scanning
- `IsSeniorPersonnelAnchor` — predicate that checks both anchor columns
- `FindTotalColumnNumber` — scans header row for `Total` cell
- `Parse` is now a thin orchestrator; all branch-heavy logic is isolated in helpers

Coverage tooling added to `SubawardReader.Tests/`:

- `coverage-report.sh` — runs tests, generates HTML via ReportGenerator, opens in Firefox
- `coverage.runsettings` — excludes `[SubawardReader]Program` from metrics (top-level entry point, not unit-testable)

### Detect Subaward rows

### Parse recipient name

### Parse amount

## Exit Criteria

- [ ] Parser returns expected extracted rows for all example files
