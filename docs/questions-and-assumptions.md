# Questions and Assumptions

This document records questions that arose during development and the assumptions made in the absence of explicit requirements. These are questions to clarify with the stakeholder on a real engagement.

---

**Q: How should the application know where to find the spreadsheet files?**
A (assumption): The folder path is required as a command-line argument. If not provided, the application exits with a usage error: `Usage: SubawardReader <path-to-folder>`. The folder can be located anywhere on the filesystem.

---

**Q: Which column contains the total subaward amount?**
A (assumption): The Total column is not assumed to be at a fixed index. The application locates it dynamically by scanning the header row for a cell whose value contains "Total" (case-insensitive).

---

**Q: Which sheet in each workbook contains the budget data?**
A (assumption): The data is always on the first sheet. The sheet name may differ between files and is not used for identification.

---

**Q: How are subaward rows identified in the spreadsheet?**
A (assumption): First locate the `G.` section anchor in column A and confirm the adjacent cell in column B is `Other Direct Costs`. From that section onward, treat a row as a subaward entry if column B starts with `Subaward:` (case-insensitive). Read the recipient name from the adjacent column C cell on the same row.

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
