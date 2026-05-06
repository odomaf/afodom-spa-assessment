# Gate 2: Core Design

Goal: Establish testable boundaries before implementation details.

## Design Decisions

### Models

Two record types live in the `SubawardReader` project.

Both models live in the `SubawardReader` console app project. A separate class library is unnecessary here — the test project already references the app project directly, so models are automatically visible to tests. The `Models/` subfolder is organizational convention only.

**SubawardRow** — one extracted row from a single workbook:

```csharp
public record SubawardRow(string FileName, string RecipientName, decimal Amount);
```

**SubawardSummary** — one aggregated entry in the final output:

```csharp
public record SubawardSummary(string RecipientName, decimal TotalAmount);
```

### Parser Interface

The parser is defined behind an interface rather than used as a concrete class directly. This keeps `Program.cs` loosely coupled from parsing logic, and allows tests to substitute a fake parser when testing aggregation or output behavior in isolation — without needing real files on disk.

```csharp
public interface ISubawardParser
{
    IEnumerable<SubawardRow> Parse(string filePath);
}
```

One file path in, a sequence of extracted rows out. The caller is responsible for looping over files and aggregating results. This keeps the parser focused on a single file.

### Aggregation

Aggregation is a separate static concern — not part of the parser. A simple LINQ group-by on `RecipientName` with first-appearance ordering is sufficient.

### Console Output

`Program.cs` handles all console writes. Parsing, aggregation, and output formatting are never mixed in the same method.

## Files to Create

| File                                        | Purpose                    |
| ------------------------------------------- | -------------------------- |
| `SubawardReader/Models/SubawardRow.cs`      | Extracted row model        |
| `SubawardReader/Models/SubawardSummary.cs`  | Summary row model          |
| `SubawardReader/Parsing/ISubawardParser.cs` | Parser interface           |
| `SubawardReader/Parsing/SubawardParser.cs`  | Parser implementation stub |

## Exit Criteria

- [x] `SubawardRow` record defined and compiles
- [x] `SubawardSummary` record defined and compiles
- [x] `ISubawardParser` interface defined and compiles
- [x] `SubawardParser` stub implementing `ISubawardParser` compiles
- [x] `dotnet build` passes with no errors
