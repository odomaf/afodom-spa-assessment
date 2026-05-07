# Gate 5: Console UX and Error Handling

> **Note:** For all technical and user-facing assumptions, see `questions-and-assumptions.md` (canonical source of truth).

Goal: Make output readable and resilient for non-technical reviewers.

## Checklist

- [x] Print clear per-file section (file name + subrecipient names).
- [ ] Format currency clearly (for example, `$#,##0.00`).
- [x] If the total is empty for a row, display $0 so the user knows the row was processed.
- [ ] Consider using color in output to facilitate easy comprehension (e.g., highlight errors, totals, or important values).
- [x] Show file with zero subawards as processed.
- [x] Handle invalid input path with clear message.
- [x] Handle missing `Total` column with clear file-level error.
- [x] Handle malformed amount cells defensively.

## Implementation Notes

- Output is visually clear, with per-file sections and subaward rows easy to distinguish.
- Amounts display as $0 for empty or missing totals; currency formatting is planned but not yet implemented.
- All error messages are actionable and free of jargon, with clear file-level errors for missing or malformed data.
- Files with no subawards are listed as processed, with a clear message.
- Merged header detection and subaward extraction are now fully centralized and robust to both same-cell and adjacent-cell recipient formats.

## Open Questions / Assumptions

- See `questions-and-assumptions.md` for the latest on error handling and output formatting edge cases.
- Defensive handling of malformed or missing amount cells is required; $0 should be shown if no value is present.

## Lessons Learned / Edge Cases (Gate 5)

- Code now handles both merged and non-merged “Total” headers, and both recipient extraction formats, with clear output for all cases.
- Defensive output (never blank for amounts) ensures users know every row was processed.
- Clear, actionable error messages reduce confusion and support easier troubleshooting.
