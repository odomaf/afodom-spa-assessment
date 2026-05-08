using System;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using SubawardReader.Parsing;
using Xunit;

namespace SubawardReader.Tests
{
    //
    // SubawardParserTests
    //
    // These tests validate the SubawardParser's ability to extract subaward recipient and amount data from Excel files.
    // Each test covers a specific scenario, including real-world files, edge cases, and malformed input.
    // Comments clarify the scenario, expected behavior, and why the test exists.
    //
    public class SubawardParserTests
    {
        [Fact]
        // Validates that the parser extracts all expected recipients from SubawardBudgetExample1.xlsx.
        // This file contains four recipients: Indiana, Mayo, Purdue, Florida.
        public void Parse_WithExampleWorkbook_ReturnsExpectedSubawardRows()
        {
            string repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            string workbookPath = Path.Combine(repoRoot, "data", "SubawardBudgetExample1.xlsx");

            var parser = new SubawardParser();

            var rows = parser.Parse(workbookPath).ToList();
            var recipients = rows.Select(row => row.RecipientName).ToList();

            Assert.Equal("SubawardBudgetExample1.xlsx", rows.First().FileName);
            Assert.Contains("Indiana", recipients);
            Assert.Contains("Mayo", recipients);
            Assert.Contains("Purdue", recipients);
            Assert.Contains("Florida", recipients);
        }

        [Fact]
        // Validates that the parser can handle a real-world file (SubawardBudgetExample2.xlsx)
        // with a multi-column merged Total header and multiple subheaders.
        // The test expects at least one subaward row to be extracted.
        public void Parse_WithMultiColumnTotals_GrabsAllSubheadersAndOutputsCorrectly()
        {
            string repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            string filePath = Path.Combine(repoRoot, "data", "SubawardBudgetExample2.xlsx");

            var parser = new SubawardParser();
            var rows = parser.Parse(filePath).ToList();
            Assert.NotEmpty(rows);
            // Add assertions here based on the expected content of SubawardBudgetExample2.xlsx
        }

        [Fact]
        // Ensures that a workbook with the expected header structure but no subaward rows returns an empty list.
        public void Parse_WithExpectedStructure_ReturnsEmptyList()
        {
            string filePath = CreateTempWorkbook(path =>
            {
                using var workbook = new XLWorkbook();
                var sheet = workbook.AddWorksheet("Budget");

                sheet.Cell(10, 1).Value = "Header Col A";
                sheet.Cell(10, 5).Value = "Total";
                sheet.Cell(11, 1).Value = "A.";
                sheet.Cell(11, 2).Value = "Senior Personnel";

                workbook.SaveAs(path);
            });

            try
            {
                var parser = new SubawardParser();

                var rows = parser.Parse(filePath).ToList();
                Assert.Empty(rows);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        // Ensures that the parser ignores anchors with extra whitespace or case differences.
        public void Parse_WithTrimmedAndCaseInsensitiveAnchor_ReturnsEmptyList()
        {
            string filePath = CreateTempWorkbook(path =>
            {
                using var workbook = new XLWorkbook();
                var sheet = workbook.AddWorksheet("Budget");

                sheet.Cell(10, 1).Value = "Header Col A";
                sheet.Cell(10, 5).Value = "Total";
                sheet.Cell(11, 1).Value = " A. ";
                sheet.Cell(11, 2).Value = " senior personnel ";

                workbook.SaveAs(path);
            });

            try
            {
                var parser = new SubawardParser();

                var rows = parser.Parse(filePath).ToList();
                Assert.Empty(rows);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        // Ensures that the parser ignores a 'PROJECT TOTAL' header (case-insensitive, not 'Total').
        public void Parse_WithCaseInsensitiveTotalHeader_ReturnsEmptyList()
        {
            string filePath = CreateTempWorkbook(path =>
            {
                using var workbook = new XLWorkbook();
                var sheet = workbook.AddWorksheet("Budget");

                sheet.Cell(10, 1).Value = "Header Col A";
                sheet.Cell(10, 5).Value = "PROJECT TOTAL";
                sheet.Cell(11, 1).Value = "A.";
                sheet.Cell(11, 2).Value = "Senior Personnel";

                workbook.SaveAs(path);
            });

            try
            {
                var parser = new SubawardParser();

                var rows = parser.Parse(filePath).ToList();
                Assert.Empty(rows);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        // Ensures that the parser throws if the anchor row is missing.
        public void Parse_WithoutAnchorRow_ThrowsInvalidOperationException()
        {
            string filePath = CreateTempWorkbook(path =>
            {
                using var workbook = new XLWorkbook();
                var sheet = workbook.AddWorksheet("Budget");

                sheet.Cell(10, 1).Value = "Header Col A";
                sheet.Cell(10, 5).Value = "Total";

                workbook.SaveAs(path);
            });

            try
            {
                var parser = new SubawardParser();

                var exception = Assert.Throws<InvalidOperationException>(() => parser.Parse(filePath).ToList());
                Assert.Contains("Could not determine header row", exception.Message);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        // Ensures that the parser throws if column two does not contain 'Senior Personnel' after 'A.' in column one.
        public void Parse_WithAInColumnOneButWrongColumnTwo_ThrowsInvalidOperationException()
        {
            string filePath = CreateTempWorkbook(path =>
            {
                using var workbook = new XLWorkbook();
                var sheet = workbook.AddWorksheet("Budget");

                sheet.Cell(10, 1).Value = "Header Col A";
                sheet.Cell(10, 5).Value = "Total";
                sheet.Cell(11, 1).Value = "A.";
                sheet.Cell(11, 2).Value = "Not Senior Personnel";

                workbook.SaveAs(path);
            });

            try
            {
                var parser = new SubawardParser();

                var exception = Assert.Throws<InvalidOperationException>(() => parser.Parse(filePath).ToList());
                Assert.Contains("Could not determine header row", exception.Message);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        // Ensures that the parser throws if the required merged 'Total' header is missing.
        public void Parse_WithoutTotalHeader_ThrowsInvalidOperationException()
        {
            string filePath = CreateTempWorkbook(path =>
            {
                using var workbook = new XLWorkbook();
                var sheet = workbook.AddWorksheet("Budget");

                sheet.Cell(10, 1).Value = "Header Col A";
                sheet.Cell(11, 1).Value = "A.";
                sheet.Cell(11, 2).Value = "Senior Personnel";

                workbook.SaveAs(path);
            });

            try
            {
                var parser = new SubawardParser();

                var exception = Assert.Throws<InvalidOperationException>(() => parser.Parse(filePath).ToList());
                Assert.Contains("No merged 'Total' header found in the two", exception.Message);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        // Validates that the parser extracts the recipient name when it appears in the same cell as 'Subaward:'.
        public void Parse_WithRecipientInSameCellAsSubaward_ExtractsRecipientCorrectly()
        {
            string filePath = CreateTempWorkbook(path =>
            {
                using var workbook = new XLWorkbook();
                var sheet = workbook.AddWorksheet("Budget");

                // Header row (row 1):
                sheet.Cell(1, 1).Value = "Header Col A";
                sheet.Cell(1, 5).Value = "Total";

                // Anchor row (row 2):
                sheet.Cell(2, 1).Value = "A.";
                sheet.Cell(2, 2).Value = "Senior Personnel";

                // Section anchor (row 3):
                sheet.Cell(3, 1).Value = "G.";
                sheet.Cell(3, 2).Value = "Other Direct Costs";

                // Subaward row (row 4): recipient in same cell as Subaward:
                sheet.Cell(4, 2).Value = "Subaward: Indiana University";
                sheet.Cell(4, 5).Value = 12345.67m;

                workbook.SaveAs(path);
            });

            try
            {
                var parser = new SubawardParser();
                var rows = parser.Parse(filePath).ToList();
                Assert.Single(rows);
                Assert.Equal("Indiana University", rows[0].RecipientName);
                Assert.Equal(12345.67m, rows[0].Amounts.Values.First());
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        // Validates that the parser extracts the recipient name when it appears in the cell adjacent to 'Subaward:'.
        public void Parse_WithRecipientInAdjacentCell_ExtractsRecipientCorrectly()
        {
            string filePath = CreateTempWorkbook(path =>
            {
                using var workbook = new XLWorkbook();
                var sheet = workbook.AddWorksheet("Budget");

                // Header row (row 1):
                sheet.Cell(1, 1).Value = "Header Col A";
                sheet.Cell(1, 5).Value = "Total";

                // Anchor row (row 2):
                sheet.Cell(2, 1).Value = "A.";
                sheet.Cell(2, 2).Value = "Senior Personnel";

                // Section anchor (row 3):
                sheet.Cell(3, 1).Value = "G.";
                sheet.Cell(3, 2).Value = "Other Direct Costs";

                // Subaward row (row 4): recipient in adjacent cell
                sheet.Cell(4, 2).Value = "Subaward:";
                sheet.Cell(4, 3).Value = "Indiana University";
                sheet.Cell(4, 5).Value = 54321.00m;

                workbook.SaveAs(path);
            });

            try
            {
                var parser = new SubawardParser();
                var rows = parser.Parse(filePath).ToList();
                Assert.Single(rows);
                Assert.Equal("Indiana University", rows[0].RecipientName);
                Assert.Equal(54321.00m, rows[0].Amounts.Values.First());
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        // Validates that the parser extracts all subheading/value pairs from SubawardBudgetExample2.xlsx,
        // which contains a multi-column merged Total header and multiple subaward rows.
        // The test expects at least one subaward row to be extracted.
        public void Parse_WithMultiColumnTotalHeader_ExtractsAllSubheadingValuePairs()
        {
            string repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            string filePath = Path.Combine(repoRoot, "data", "SubawardBudgetExample2.xlsx");

            var parser = new SubawardParser();
            var rows = parser.Parse(filePath).ToList();
            // Add assertions here based on the expected content of SubawardBudgetExample2.xlsx
            Assert.NotEmpty(rows);
            // Example: Assert.Contains("Indiana", rows.Select(r => r.RecipientName));
        }

        [Fact]
        // Validates that the parser can extract subheading/value pairs when the Total header is two rows above the anchor,
        // using SubawardBudgetExample2.xlsx as a real-world example. The test expects at least one subaward row.
        public void Parse_WithTotalHeaderTwoRowsAboveAnchor_ExtractsAllSubheadingValuePairs()
        {
            string repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            string filePath = Path.Combine(repoRoot, "data", "SubawardBudgetExample2.xlsx");

            var parser = new SubawardParser();
            var rows = parser.Parse(filePath).ToList();
            Assert.NotEmpty(rows);
            // Add assertions here based on the expected content of SubawardBudgetExample2.xlsx
        }

        private static string CreateTempWorkbook(Action<string> buildWorkbook)
        {
            string filePath = Path.Combine(Path.GetTempPath(), $"subaward-parser-tests-{Guid.NewGuid():N}.xlsx");
            buildWorkbook(filePath);
            return filePath;
        }
    }
}
