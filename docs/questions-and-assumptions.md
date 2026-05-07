# Questions and Assumptions

This document records questions that arose during development and the assumptions made in the absence of explicit requirements. These are questions to clarify with the stakeholder on a real engagement.

---

**Q: Who is the minimally knowledged user, and how should console messages be written?**
A (assumption): The minimally knowledged user is someone who can run console commands but is not a programmer. All console messages must be clear, avoid programming jargon, and help users report issues or check spreadsheets without needing to understand code.

---

**Q: How should the application know where to find the spreadsheet files?**
A (assumption): The folder path is required as a command-line argument. If not provided, the application exits with a usage error: `Usage: SubawardReader <path-to-folder>`. The folder can be located anywhere on the filesystem.

---

**Q: What if the 'Total' header spans multiple columns or is not directly above the anchor row?**
A (assumption): The application searches both the row directly above the anchor row and the row above that for a cell containing "Total" (case-insensitive). If "Total" spans multiple columns (e.g., with subheadings like "Sponsor Share" and "Cost Share"), all such columns are identified and extracted for each subaward row. The output includes both the subheading and the value for clarity.

**Q: Which column contains the total subaward amount?**
A (assumption): The Total column is not assumed to be at a fixed index. The application locates it dynamically by scanning the header row for a cell whose value contains "Total" (case-insensitive).

---

**Q: Which sheet in each workbook contains the budget data?**
A (assumption): The data is always on the first sheet. The sheet name may differ between files and is not used for identification.

---

**Q: How are subaward rows identified in the spreadsheet?**
A (assumption): First locate the `G.` section anchor in column A and confirm the adjacent cell in column B is `Other Direct Costs`. From that section onward, treat a row as a subaward entry if column B starts with `Subaward:` (case-insensitive). To extract the recipient name, first check if it appears in the same cell as `Subaward:` in column B (e.g., `Subaward: Example University`). If not present, read the recipient name from the adjacent column C cell on the same row. This approach handles both spreadsheet formats observed so far.

---

**Q: Should files with zero subaward rows still appear in the per-file output?**
A (assumption): Yes. Every file processed is listed in the output, even if it contains no subaward rows, so it is clear the file was read.

---

**Q: How should the final summary list be sorted?**
A (assumption): Two reasonable options exist: alphabetical by name, or in order of first appearance across the files. Use order of first appearance so non-technical staff can cross-reference the summary against the source files without needing to search for a name.

---

**Q: Should the application process non-.xlsx files found in the folder?**
A (assumption): Only `.xlsx` files are processed. Other file types are silently skipped.

---

**Q: Which row is the header row containing column labels like "Total"?**
A (assumption): The header row is not at a fixed row number and varies across files. Locate it by scanning column 1 for a cell with the value `"A."`, confirmed by `"Senior Personnel"` in column 2 of the same row. The header row is the row immediately above that anchor row. Use this two-column match to minimize false positives, since `"A."` alone could appear elsewhere in the spreadsheet.

---

**Q: Must input be a directory path, or can it be a single file?**
A (assumption): Accept both. If input is a directory, process all `.xlsx` files in it. If input is a single `.xlsx` file, process only that file. For non-.xlsx files, return a clear error and exit.

---

**Q: Should totals be summed for recipients with identical names, or should each subaward row be reported exactly as found?**
A (assumption): Do not sum totals for identical recipients. Each subaward row should be reported exactly as it appears in the Excel document, so the output matches what a user sees. This avoids accidental aggregation and preserves fidelity to the source data.
