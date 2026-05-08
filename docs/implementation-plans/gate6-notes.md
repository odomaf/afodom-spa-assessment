# Gate 6: Tests

Goal: Validate required behavior with automated tests.

## Checklist

- [x] Add required test: `SubawardBudgetExample1.xlsx` contains Indiana, Mayo, Purdue, Florida.
- [x] Add parser tests for `Subaward:` detection and recipient extraction.
- [x] Add test for dynamic `Total` column detection.
- [x] Add aggregation test for totals and first-appearance order.

## Exit Criteria

- [x] Run all tests and confirm they pass locally.

## Implementation Notes

- Used xUnit for all tests.
- Covered edge cases, malformed input, and empty files.
- Ensured test isolation and cleanup of temp files.
- Added comments to clarify test intent and expected outcomes.
- Used real-world Excel files for validation where possible.
- Confirmed all tests pass and project builds cleanly.

## Open Questions / Assumptions

- None at this time. All requirements and edge cases addressed.

## Lessons Learned / Edge Cases (Gate 6)

- Precise placement of merged headers and subheader rows is critical for parser compatibility.
- Using real Excel files in tests reduces risk of test data errors.
- Test method comments improve clarity for both junior and senior readers.
- Test isolation and cleanup are essential for reliable results.

## Status

Gate 6 complete. All requirements met and validated.
