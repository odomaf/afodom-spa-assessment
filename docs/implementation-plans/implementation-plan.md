# Implementation Plan (Phase Gates)

This plan uses go/no-go phase gates so I can review and approve progress before the next phase starts.

Timeline details are maintained in `docs/implementation-plans/implementation-timeline.md`.

## Gate 0: Scope Lock

Goal: Verify scope decisions are locked and team alignment confirmed.

Checklist:

- [x] questions-and-assumptions.md is complete and signed off.
- [x] All implementation constraints confirmed.

Exit criteria:

- [x] Decisions documented, scope locked, ready to scaffold.

Gate status: Completed
Completed at: 2026-05-05 15:53 (UTC-05:00)

## Gate 1: Solution Scaffolding

Goal: Create the project structure and dependencies.

Checklist:

- [x] Create solution file (`.sln` or `.slnx`).
- [x] Create console app project.
- [x] Create test project.
- [x] Add both projects to the solution.
- [x] Add Excel parsing package (recommended: `ClosedXML`).
- [x] Add test dependencies (`xUnit`, runner, assertions as needed).

Exit criteria:

- [x] Restore the solution successfully.
- [x] Compile the empty app and test project.

Gate status: Completed
Completed at: 2026-05-05 21:40 (UTC-05:00)

## Gate 2: Core Design

Goal: Establish testable boundaries before implementation details.

Checklist:

- [x] Define extracted row model (`FileName`, `RecipientName`, `Amount`).
- [x] Define summary row model (`RecipientName`, `TotalAmount`).
- [x] Define parser service interface and implementation boundary.
- [x] Keep console output formatting separate from parsing/aggregation logic.

Exit criteria:

- [x] Ensure core types and interfaces are in place and compile.

Gate status: Completed
Completed at: 2026-05-06 10:46 (UTC-05:00)

## Gate 3: Parsing Implementation

See details in [gate3-notes.md](gate3-notes.md)

Goal: Extract subaward rows from each workbook correctly.

Checklist:

- [x] Resolve input path as either directory or single file.
- [x] Enumerate `.xlsx` files from input directory when directory mode is used.
- [x] Select only the specified `.xlsx` file when file mode is used.
- [x] Open first worksheet per workbook.
- [x] Discover `Total` column dynamically by scanning headers.
- [x] Locate `G.` section start by matching column A `G.` with adjacent column B `Other Direct Costs`.
- [x] Detect rows in column B where text starts with `Subaward:`.
- [x] Parse recipient name from adjacent column C on the same row.
- [x] Parse amount from the same row in the discovered `Total` column.

Exit criteria:

- [x] Verify the parser returns expected extracted rows for all example files.

Gate status: Completed
Completed at: 2026-05-06 20:15 (UTC-05:00)

## Gate 4: Subaward Row Extraction and Ordering

See details in [gate4-notes.md](gate4-notes.md)

Goal: Extract and list each subaward row exactly as found in the source files, with deterministic ordering (no aggregation by recipient).

Checklist:

- [x] List each subaward row exactly as found in the source files.
- [x] Preserve first-appearance ordering for summary output.
- [x] Preserve row order in per-file sections.
- [x] Update parser to extract recipient from same or adjacent cell as "Subaward:" in column B.
- [x] Output all subheading/value pairs for each subaward row.

Exit criteria:

- [x] Verify output matches the subaward rows and totals as shown in the Excel documents, with no aggregation.
- [x] Validate parser against sample spreadsheets with merged or multi-column totals and recipient in same/adjacent cell.

Gate status: Completed
Completed at: 2026-05-07 15:30 (UTC-05:00)

## Gate 5: Console UX and Error Handling

See details in [gate5-notes.md](gate5-notes.md)

Goal: Make output readable and resilient for non-technical reviewers.

Checklist:

- [x] Print clear per-file section (file name + subrecipient names).
- [x] Format currency clearly (for example, `$#,##0.00`).
- [x] If the total is empty for a row, display $0 so the user knows the row was processed.
- [x] Consider using color in output to facilitate easy comprehension (e.g., highlight errors, totals, or important values).
- [x] Show file with zero subawards as processed.
- [x] Handle invalid input path with clear message.
- [x] Handle missing `Total` column with clear file-level error.
- [x] Handle malformed amount cells defensively.

Implementation Notes: - All output formatting and error messages are now routed through ConsoleFormatter, which uses Spectre.Console for consistent color and style conventions: - Filenames: yellow - Recipient names: bold - Money values: green - Errors: red - "No subaward rows found": gray

Exit criteria:

- [x] Ensure the app completes with readable output and actionable errors.

Summary: Console output and error handling are complete and meet requirements for non-technical users. All formatting, error handling, and color conventions are implemented and documented. Gate 5 is fully complete.

Gate status: Completed
Completed at: 2026-05-07

## Gate 6: Tests

See details in [gate6-notes.md](gate6-notes.md)

Goal: Validate required behavior with automated tests.

Checklist:

- [ ] Add required test: `SubawardBudgetExample1.xlsx` contains Indiana, Mayo, Purdue, Florida.
- [ ] Add parser tests for `Subaward:` detection and recipient extraction.
- [ ] Add test for dynamic `Total` column detection.
- [ ] Add aggregation test for totals and first-appearance order.

Exit criteria:

- [ ] Run all tests and confirm they pass locally.

## Gate 6.5: Final Interview Lens Review and Updates

Goal: Ensure the codebase is interview-ready and meets clarity, maintainability, and best-practice standards.

Checklist:

- [ ] Perform a full Interview Lens review of the codebase.
- [ ] Add or improve comments for junior clarity where needed.
- [ ] Refactor any large or complex methods for readability.
- [ ] Replace magic strings with constants or enums where appropriate.
- [ ] Add or update test comments to clarify intent.
- [ ] Address any weak signals or recommendations from the review.

Exit criteria:

- [ ] All Interview Lens recommendations are addressed or documented as deferred.
- [ ] Codebase is ready for final documentation and reviewer handoff.

## Gate 7: Documentation

Goal: Make repo pull-and-run ready for reviewers.

Checklist:

- [ ] Update `README.md` with prerequisites, run command, and test command.
- [ ] Reference `docs/questions-and-assumptions.md` from README.
- [ ] Document key implementation choices and known limitations.

Exit criteria:

- [ ] Verify a reviewer can clone, run, and test without modification.

## Gate 8: Final Verification and Commit

Goal: Verify end-to-end before handoff.

Checklist:

- [ ] Run app against provided `data/` files.
- [ ] Verify per-file names and final totals.
- [ ] Run full test suite.
- [ ] Perform final code readability pass (comments clear, not excessive).

Exit criteria:

- [ ] Run all commands successfully and verify output matches requirements.

## Optional Commit Batching

- [ ] Commit scaffolding and package setup.
- [ ] Commit parser and aggregation logic.
- [ ] Commit tests.
- [ ] Commit documentation.
