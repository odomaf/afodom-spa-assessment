# Implementation Plan (Phase Gates)

This plan uses go/no-go phase gates so I can review and approve progress before the next phase starts.

## Gate 0: Scope Lock

Goal: Verify scope decisions are locked and team alignment confirmed.

Checklist:

- [ ] questions-and-assumptions.md is complete and signed off.
- [ ] All implementation constraints confirmed.

Exit criteria:

- [ ] Decisions documented, scope locked, ready to scaffold.

## Gate 1: Solution Scaffolding

Goal: Create the project structure and dependencies.

Checklist:

- [ ] Create solution file (`.sln` or `.slnx`).
- [ ] Create console app project.
- [ ] Create test project.
- [ ] Add both projects to the solution.
- [ ] Add Excel parsing package (recommended: `ClosedXML`).
- [ ] Add test dependencies (`xUnit`, runner, assertions as needed).

Exit criteria:

- [ ] Restore the solution successfully.
- [ ] Compile the empty app and test project.

## Gate 2: Core Design

Goal: Establish testable boundaries before implementation details.

Checklist:

- [ ] Define extracted row model (`FileName`, `RecipientName`, `Amount`).
- [ ] Define summary row model (`RecipientName`, `TotalAmount`).
- [ ] Define parser service interface and implementation boundary.
- [ ] Keep console output formatting separate from parsing/aggregation logic.

Exit criteria:

- [ ] Ensure core types and interfaces are in place and compile.

## Gate 3: Parsing Implementation

Goal: Extract subaward rows from each workbook correctly.

Checklist:

- [ ] Resolve input path as either directory or single file.
- [ ] Enumerate `.xlsx` files from input directory when directory mode is used.
- [ ] Select only the specified `.xlsx` file when file mode is used.
- [ ] Open first worksheet per workbook.
- [ ] Discover `Total` column dynamically by scanning headers.
- [ ] Detect rows where column A starts with `Subaward:`.
- [ ] Parse recipient name from the same cell text.
- [ ] Parse amount from the same row in the discovered `Total` column.

Exit criteria:

- [ ] Verify the parser returns expected extracted rows for all example files.

## Gate 4: Aggregation and Ordering

Goal: Produce final summary totals and deterministic ordering.

Checklist:

- [ ] Aggregate totals by recipient across all files.
- [ ] Preserve first-appearance ordering for summary output.
- [ ] Preserve row order in per-file sections.

Exit criteria:

- [ ] Verify summary totals match manual checks for sample data.

## Gate 5: Console UX and Error Handling

Goal: Make output readable and resilient for non-technical reviewers.

Checklist:

- [ ] Print clear per-file section (file name + subrecipient names).
- [ ] Print final summary section (recipient + total amount).
- [ ] Format currency clearly (for example, `$#,##0.00`).
- [ ] Show file with zero subawards as processed.
- [ ] Handle invalid input path with clear message.
- [ ] Handle missing `Total` column with clear file-level error.
- [ ] Handle malformed amount cells defensively.

Exit criteria:

- [ ] Ensure the app completes with readable output and actionable errors.

## Gate 6: Tests

Goal: Validate required behavior with automated tests.

Checklist:

- [ ] Add required test: `SubawardBudgetExample1.xlsx` contains Indiana, Mayo, Purdue, Florida.
- [ ] Add parser tests for `Subaward:` detection and recipient extraction.
- [ ] Add test for dynamic `Total` column detection.
- [ ] Add aggregation test for totals and first-appearance order.

Exit criteria:

- [ ] Run all tests and confirm they pass locally.

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
