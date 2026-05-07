# afodom-spa-assessment

Console application assessment for job app with UIUC SPA

## Project Setup Baseline

- Runtime/SDK: .NET 10 x64
- Solution file: `SubawardReader.slnx`
- Projects: `SubawardReader`, `SubawardReader.Tests`
- Added packages:
  - App: `ClosedXML`
  - Tests: `xunit.runner.visualstudio`
- Validation commands run:

```bash
dotnet restore
dotnet build
```

- Result: restore and build succeeded

## Gate 4: Aggregation and Ordering (2026-05-07)

> **Note:** For all technical and user-facing assumptions, see `docs/questions-and-assumptions.md` (canonical source of truth).

- Parser and data model updated to support multi-column "Total" headers and extraction of all subheading/value pairs.
- Recipient extraction logic now handles both same-cell and adjacent-cell formats for "Subaward:" rows.
- Output lists all subaward rows as found, with no aggregation, and preserves row order.
- All documentation and assumptions updated to reflect new logic and user experience requirements.

See:

- docs/implementation-plans/gate4-notes.md (Gate 4 details)
- docs/questions-and-assumptions.md (updated assumptions)
