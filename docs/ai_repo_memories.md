# AI Repo Memories

This file stores backup copies of repo-specific AI memory instructions for afodom-spa-assessment.

## Active Rules

- Comment code clearly for maintainability without over-commenting.
- Prefer the most efficient implementations that follow .NET console app best practices.
- Keep repository-specific memories local to this repo; do not copy them to ai_memories unless explicitly requested.
- Project baseline: .NET 10 (C#), solution file is `SubawardReader.slnx`.
- Gate tracking convention: when a gate is done, mark checklist and exit criteria complete, then add `Gate status: Completed` and `Completed at` timestamp.
- Keep schedule data in `docs/implementation-plans/implementation-timeline.md` and execution details in `docs/implementation-plans/implementation-plan.md`.
- Gate 1 is completed and verified (`dotnet restore` and `dotnet build` succeeded).

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
