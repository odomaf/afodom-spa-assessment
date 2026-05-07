using ClosedXML.Excel;
using SubawardReader.Parsing;

namespace SubawardReader.Tests;

public class SubawardParserTests
{
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

    private static string CreateTempWorkbook(Action<string> buildWorkbook)
    {
        string filePath = Path.Combine(Path.GetTempPath(), $"subaward-parser-tests-{Guid.NewGuid():N}.xlsx");
        buildWorkbook(filePath);
        return filePath;
    }
}
