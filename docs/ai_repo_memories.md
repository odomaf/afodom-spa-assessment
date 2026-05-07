# 2026-05-07 (refactor plan)

- Create a TotalHeaderInfo struct/class to encapsulate all header-related values (row, col start/end, col span, debug info).
- Refactor SubawardParser.ParseWithColumnOrderWithRowSpanAndColSpan to return a result object containing:
	- List of SubawardRow
	- TotalHeaderInfo
	- List of subheadings (if needed)
- Centralize all logic for extracting subaward row amounts and subheadings into SubawardParser (or a helper).
- Update Program.cs to use the result object, removing manual header variable management.
- Add XML comments and use clear, consistent naming for all new types and methods.

# AI Repo Memories

This file stores backup copies of repo-specific AI memory instructions for afodom-spa-assessment.

## Active Rules

- Comment code clearly for maintainability without over-commenting. Skip comments on well-known conventions (exit codes, standard API calls); only comment where intent isn't obvious from structure alone.
- Prefer the most efficient implementations that follow .NET console app best practices.
- `WriteError(string message)` in `Program.cs` is the established helper for all red console error output; use it for all future error cases rather than writing directly to `Console.Error`.
- Keep repository-specific memories local to this repo; do not copy them to ai_memories unless explicitly requested.
- Project baseline: .NET 10 (C#), solution file is `SubawardReader.slnx`.
- Gate tracking convention: when a gate is done, mark checklist and exit criteria complete, then add `Gate status: Completed` and `Completed at` timestamp.
- Keep schedule data in `docs/implementation-plans/implementation-timeline.md` and execution details in `docs/implementation-plans/implementation-plan.md`.
- Gates 1 and 2 are complete. Models, interface, and parser stub are in place under `SubawardReader/Models/` and `SubawardReader/Parsing/`.
- Gate 3 is in progress: input path resolution, worksheet open, dynamic `Total` discovery, and parser helper refactor are complete.
- Header row discovery convention: locate anchor row with column 1 `A.` and column 2 exact `Senior Personnel` (trimmed, case-insensitive), then use the row above as header.
- Parser structure: `SubawardParser` uses private helpers `FindHeaderRowNumber`, `IsSeniorPersonnelAnchor`, and `FindTotalColumnNumber` to keep `Parse` as a thin orchestrator.
- Parser structure protections: 6 unit tests cover missing anchor, false anchor, missing `Total`, trimmed/case-insensitive anchor, and case-insensitive Total header.
- Coverage tooling: `SubawardReader.Tests/coverage-report.sh` runs tests with coverlet, generates HTML via ReportGenerator, and opens in Firefox.
- Coverage config: `SubawardReader.Tests/coverage.runsettings` excludes `[SubawardReader]Program` from coverage metrics (top-level statement entry point, tested manually).
- Parser design rule: as Gate 3 row-iteration logic is added, keep `Parse` as a thin orchestrator — extract helpers rather than accumulating inline logic.

## Project Context

### Assessment Requirements

- .NET 9/10 console app
- Read all Excel files from a folder; 3 example files are in data/
- Per file: output filename + each subrecipient name under "G. Other Direct Costs"
- Final output: distinct subrecipient list with total subaward amounts across all files, human-readable
- Handle 0 to unknown subaward rows per file
- At least one unit test confirming SubawardBudgetExample1.xlsx has: Indiana, Mayo, Purdue, Florida
- Must run as-is from public GitHub with no modifications
- README must document assumptions and questions

### Spreadsheet Structure (confirmed)

- Subaward rows: column A starts with `Subaward:  {Name}`, amount is on the same row in the Total column
- `Exempt Subaward Costs (>$25k)` rows below each subaward are separate cost lines — ignore them
- No need to locate the "G. Other Direct Costs" header; matching on `Subaward:` prefix is sufficient

## Memory Log

### 2026-05-05

Reason: Initial repo-specific memory setup and audit-friendly structure.

Changes:

- Add `Active Rules` section for current source of truth.
- Add `Memory Log` section for dated change tracking.
- Preserve existing repo-specific instruction entries.

### 2026-05-05 (end of session)

Reason: Record Gate 1 completion and planning document conventions established tonight.

Changes:

- Add rule: project baseline is .NET 10 (C#), solution file is `SubawardReader.slnx`.
- Add rule: gate completion convention — mark checklist, add `Gate status: Completed` and `Completed at` timestamp.
- Add rule: keep schedule in implementation-timeline.md and execution details in implementation-plan.md.
- Add note: Gate 1 is completed and verified (`dotnet restore` and `dotnet build` succeeded).

### 2026-05-06

Reason: Capture Gate 3 parsing-structure decisions and protections after completing worksheet/header discovery steps.

Changes:

- Add note: Gate 3 is in progress with input path resolution, worksheet open, and dynamic `Total` discovery complete.
- Add rule: determine header row from anchor row (`A.` + exact `Senior Personnel`) and use the row above as header.
- Add note: parser structure behavior is guarded with temporary-workbook unit tests for missing anchor, false anchor, and missing `Total` paths.

### 2026-05-06 (session 2)

Reason: Capture parser refactor, expanded test coverage, and coverage tooling added this session.

Changes:

- Update Gate 3 progress: parser helper refactor complete.
- Add rule: `SubawardParser` uses private helpers to keep `Parse` as thin orchestrator.
- Update test count: 6 tests now cover all known structural edge cases including trim/case variants.
- Add note: `coverage-report.sh` added to `SubawardReader.Tests/` for one-command HTML coverage in Firefox.
- Add note: `coverage.runsettings` added to exclude `[SubawardReader]Program` from coverage (top-level statements generate a synthetic Main$ method).

### 2026-05-06 (session 3)

Reason: Capture parser design rule surfaced during Interview Lens review.

Changes:

- Add rule: keep `Parse` as a thin orchestrator as Gate 3 row-iteration logic is added; extract helpers rather than accumulating inline logic.

### 2026-05-07 (backup)

Backed up planned unit test updates for Gate 4 coverage:

- Add new test: recipient extraction from same-cell format ("Subaward: Example University" in column B).
- Add new test: recipient extraction from adjacent-cell format ("Subaward:" in column B, recipient in column C).
- Add new test: extraction of all subheading/value pairs when "Total" spans multiple columns.
- Update or add test: error output is user-friendly and non-technical.

No existing test covers the adjacent-cell recipient extraction scenario; must be a new test.
