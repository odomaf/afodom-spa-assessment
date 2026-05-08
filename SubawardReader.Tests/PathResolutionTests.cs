using System;
using System.IO;
using Xunit;

namespace SubawardReader.Tests
{
    public class PathResolutionTests
    {
        [Theory]
        [InlineData("data", true)] // relative directory
        [InlineData("data/SubawardBudgetExample1.xlsx", false)] // relative file
        [InlineData("./data", true)] // relative with dot
        [InlineData("./data/SubawardBudgetExample1.xlsx", false)] // relative file with dot
        [InlineData("../afodom-spa-assessment/data", true)] // parent relative
        [InlineData("../afodom-spa-assessment/data/SubawardBudgetExample1.xlsx", false)] // parent relative file
        public void PathResolution_ResolvesToAbsolute(string input, bool isDirectory)
        {
            // Arrange
            string resolved = Path.GetFullPath(input);

            // Act & Assert
            if (isDirectory)
            {
                Assert.True(Path.IsPathRooted(resolved));
                Assert.EndsWith("data", resolved.Replace("\\", "/"));
            }
            else
            {
                Assert.True(Path.IsPathRooted(resolved));
                Assert.EndsWith("SubawardBudgetExample1.xlsx", resolved.Replace("\\", "/"));
            }
        }
    }
}
