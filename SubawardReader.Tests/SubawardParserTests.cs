using System;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using SubawardReader.Parsing;
using Xunit;

namespace SubawardReader.Tests
{
    public class SubawardParserTests
    {
        [Fact]
        public void Parse_WithMultiColumnTotals_GrabsAllSubheadersAndOutputsCorrectly()
        {
            string filePath = CreateTempWorkbook(path =>
            {
                using var workbook = new XLWorkbook();
                var sheet = workbook.AddWorksheet("Budget");

                // Header row (row 1):
                sheet.Cell(1, 1).Value = "Header Col A";
                sheet.Cell(1, 5).Value = "Sponsor Share Total";
                sheet.Cell(1, 6).Value = "Cost Share Total";

                // Anchor row (row 2):
                sheet.Cell(2, 1).Value = "A.";
                sheet.Cell(2, 2).Value = "Senior Personnel";

                // Section anchor (row 3):
                sheet.Cell(3, 1).Value = "G.";
                sheet.Cell(3, 2).Value = "Other Direct Costs";

                // Subaward row (row 4):
                sheet.Cell(4, 2).Value = "Subaward: Indiana University";
                sheet.Cell(4, 5).Value = 11111.00m; // Sponsor Share
                sheet.Cell(4, 6).Value = 2222.00m;  // Cost Share

                // Subaward row (row 5):
                sheet.Cell(5, 2).Value = "Subaward: Purdue";
                sheet.Cell(5, 5).Value = 33333.00m; // Sponsor Share
                sheet.Cell(5, 6).Value = 4444.00m;  // Cost Share

                workbook.SaveAs(path);
            });

            try
            {
                var parser = new SubawardParser();
                var rows = parser.Parse(filePath).ToList();
                Assert.Equal(2, rows.Count);

                var indiana = rows.First(r => r.RecipientName.Contains("Indiana"));
                var purdue = rows.First(r => r.RecipientName.Contains("Purdue"));

                Assert.Equal(11111.00m, indiana.Amounts["Sponsor Share Total"]);
                Assert.Equal(2222.00m, indiana.Amounts["Cost Share Total"]);
                Assert.Equal(33333.00m, purdue.Amounts["Sponsor Share Total"]);
                Assert.Equal(4444.00m, purdue.Amounts["Cost Share Total"]);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
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
                Assert.Contains("No 'Total' column found", exception.Message);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
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
        public void Parse_WithMultiColumnTotalHeader_ExtractsAllSubheadingValuePairs()
        {
            string filePath = CreateTempWorkbook(path =>
            {
                using var workbook = new XLWorkbook();
                var sheet = workbook.AddWorksheet("Budget");

                // Anchor row (row 3):
                sheet.Cell(3, 1).Value = "A.";
                sheet.Cell(3, 2).Value = "Senior Personnel";

                // Header row (row 2):
                sheet.Cell(2, 1).Value = "Header Col A";
                sheet.Cell(2, 5).Value = "Sponsor Share Total";
                sheet.Cell(2, 6).Value = "Cost Share Total";

                // Section anchor (row 4):
                sheet.Cell(4, 1).Value = "G.";
                sheet.Cell(4, 2).Value = "Other Direct Costs";

                // Subaward row (row 5):
                sheet.Cell(5, 2).Value = "Subaward: Indiana University";
                sheet.Cell(5, 5).Value = 10000.00m; // Sponsor Share
                sheet.Cell(5, 6).Value = 2000.00m;  // Cost Share

                workbook.SaveAs(path);
            });

            try
            {
                var parser = new SubawardParser();
                var rows = parser.Parse(filePath).ToList();
                Assert.Single(rows);
                var amounts = rows[0].Amounts;
                Assert.Equal(2, amounts.Count);
                Assert.True(amounts.ContainsKey("Sponsor Share Total"));
                Assert.True(amounts.ContainsKey("Cost Share Total"));
                Assert.Equal(10000.00m, amounts["Sponsor Share Total"]);
                Assert.Equal(2000.00m, amounts["Cost Share Total"]);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public void Parse_WithTotalHeaderTwoRowsAboveAnchor_ExtractsAllSubheadingValuePairs()
        {
            string filePath = CreateTempWorkbook(path =>
            {
                using var workbook = new XLWorkbook();
                var sheet = workbook.AddWorksheet("Budget");

                // Anchor row (row 4):
                sheet.Cell(4, 1).Value = "A.";
                sheet.Cell(4, 2).Value = "Senior Personnel";

                // Header row (row 2):
                sheet.Cell(2, 1).Value = "Header Col A";
                sheet.Cell(2, 5).Value = "Sponsor Share Total";
                sheet.Cell(2, 6).Value = "Cost Share Total";

                // Extra row (row 3):
                sheet.Cell(3, 1).Value = "Some Other Row";

                // Section anchor (row 5):
                sheet.Cell(5, 1).Value = "G.";
                sheet.Cell(5, 2).Value = "Other Direct Costs";

                // Subaward row (row 6):
                sheet.Cell(6, 2).Value = "Subaward: Indiana University";
                sheet.Cell(6, 5).Value = 15000.00m; // Sponsor Share
                sheet.Cell(6, 6).Value = 3000.00m;  // Cost Share

                workbook.SaveAs(path);
            });

            try
            {
                var parser = new SubawardParser();
                var rows = parser.Parse(filePath).ToList();
                Assert.Single(rows);
                var amounts = rows[0].Amounts;
                Assert.Equal(2, amounts.Count);
                Assert.True(amounts.ContainsKey("Sponsor Share Total"));
                Assert.True(amounts.ContainsKey("Cost Share Total"));
                Assert.Equal(15000.00m, amounts["Sponsor Share Total"]);
                Assert.Equal(3000.00m, amounts["Cost Share Total"]);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        private static string CreateTempWorkbook(Action<string> buildWorkbook)
        {
            string filePath = Path.Combine(Path.GetTempPath(), $"subaward-parser-tests-{Guid.NewGuid():N}.xlsx");
            buildWorkbook(filePath);
            return filePath;
        }
    }
}
