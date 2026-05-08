# SubawardReader: Excel Parsing Assessment for UIUC SPA

Console application that extracts and summarizes subaward recipient data from Excel spreadsheets.
Assessment project for UIUC Sponsored Programs Administration (SPA).

> **Note:** All commands in this README are shown in bash syntax. On Windows, use Git Bash, WSL, or adapt paths for PowerShell/cmd as needed.

---

## Prerequisites

- .NET 10 SDK (x64) installed ([Download .NET 10](https://dotnet.microsoft.com/download))
- Windows (recommended/tested), but should work cross-platform

## Getting Started

1. **Clone the repository**

   ```bash
   git clone https://github.com/odomaf/afodom-spa-assessment.git
   cd afodom-spa-assessment
   ```

2. **Restore and build**

   ```bash
   dotnet restore
   dotnet build
   ```

3. **Run the application**

   **PowerShell/Windows:**
   - To process all `.xlsx` files in the provided test data folder:
     ```powershell
     dotnet run --project SubawardReader .\data
     ```
   - To process all `.xlsx` files in a custom folder:
     ```powershell
     dotnet run --project SubawardReader C:\full\path\to\data
     ```
   - To process a single file in the provided test data folder:
     ```powershell
     dotnet run --project SubawardReader .\data\SubawardBudgetExample1.xlsx
     ```
   - To process a single file in a custom folder:
     ```powershell
     dotnet run --project SubawardReader C:\full\path\to\data\SubawardBudgetExample1.xlsx
     ```

   **bash/cross-platform:**
   - To process all `.xlsx` files in the provided test data folder:
     ```bash
     dotnet run --project SubawardReader ./data
     ```
   - To process all `.xlsx` files in a custom folder:
     ```bash
     dotnet run --project SubawardReader /full/path/to/data
     ```
   - To process a single file in the provided test data folder:
     ```bash
     dotnet run --project SubawardReader ./data/SubawardBudgetExample1.xlsx
     ```
   - To process a single file in a custom folder:
     ```bash
     dotnet run --project SubawardReader /full/path/to/data/SubawardBudgetExample1.xlsx
     ```

   > Input path must be a full path or a valid relative path from the repo root.

   After listing subrecipients per file, the app outputs a summary of all unique subrecipients and their total subaward amounts across all files, as required.

   After listing subrecipients per file, the app outputs a summary of all unique subrecipients and their total subaward amounts across all files, as required.

4. **Run tests**

   ```bash
   dotnet test SubawardReader.Tests
   ```

   - Includes a test that confirms SubawardBudgetExample1.xlsx contains exactly ‘Indiana’, ‘Mayo’, ‘Purdue’, and ‘Florida’ as subrecipients, as required.

   - To view coverage (requires bash and ReportGenerator):
     ```bash
     cd SubawardReader.Tests
     ./coverage-report.sh
     ```

   > Note: Some parsing methods have elevated CRAP scores due to necessary complexity and deadline-driven deferral of refactoring. See [Future Improvements](docs/implementation-plans/implementation-plan.md#future-improvements-recommended-but-not-required-for-submission) for details on planned code cleanup.

## Project Structure

- `SubawardReader/` — Main console app and parsing logic
- `SubawardReader.Tests/` — xUnit tests and coverage scripts
- `data/` — Example Excel files for testing
- `docs/` — Implementation plans, assumptions, and process documentation

## Key Implementation Notes

- All output and errors are formatted via `ConsoleFormatter` for clarity.
- Parser supports both single and merged/multi-column “Total” headers.
- Test suite uses real Excel files for robust validation.
- See [docs/questions-and-assumptions.md](docs/questions-and-assumptions.md) for all technical/user-facing assumptions.

## Known Limitations

- Only `.xlsx` files are supported.
- Input path must be absolute.
- See [docs/implementation-plans/implementation-plan.md](docs/implementation-plans/implementation-plan.md#future-improvements-recommended-but-not-required-for-submission) for deferred improvements.

## Assumptions & Questions

- All technical and user-facing assumptions are documented in [docs/questions-and-assumptions.md](docs/questions-and-assumptions.md).
- Any questions about ambiguous requirements are also listed there.
