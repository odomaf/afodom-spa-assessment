# Gate 4: Subaward Row Extraction and Ordering

> **Note:** For all technical and user-facing assumptions, see `questions-and-assumptions.md` (canonical source of truth).

Goal: Extract and list each subaward row exactly as found in the source files, with deterministic ordering (no aggregation by recipient).

## Checklist

- [x] Update data model to support multiple amount columns (subheading/value pairs).
- [x] Update parser logic to detect multi-row and multi-column "Total" headers.
- [x] Extract all relevant amount columns for each subaward row.
- [x] Output all subheading/value pairs for each subaward row.
- [x] Validate output against sample spreadsheets with merged or multi-column totals.
- [x] Update parser to extract recipient from same or adjacent cell as "Subaward:" in column B.

## Implementation Notes

- The data model (`SubawardRow`) now uses a dictionary to store multiple amount columns, allowing for cases where the "Total" header spans multiple columns (e.g., "Sponsor Share", "Cost Share").
- The parser searches both the row directly above the anchor row and the row above that for a cell containing "Total".
- If "Total" spans multiple columns, all subheadings and their values are extracted and included in the output.
- Output clearly shows each subaward row with all relevant subheading/value pairs, matching the structure seen in the Excel document.
- The parser now extracts the recipient name from the same cell as "Subaward:" if present; if not, it falls back to the adjacent cell. This handles both spreadsheet formats observed so far.

## Open Questions / Assumptions

- See `questions-and-assumptions.md` for the latest on handling multi-row and multi-column headers and recipient extraction.
- No aggregation is performed; each subaward row is reported as found.

## Lessons Learned / Edge Cases (Gate 4)

- Real-world spreadsheets may place the recipient name in the same cell as "Subaward:" or in the adjacent cell; parser must handle both.
- Multi-column "Total" headers require dynamic extraction of all subheading/value pairs, not just a single total.
- User experience must be tailored for non-programmers: clear output, no jargon, and explicit error/help messages.
