// SubawardParseResult: Encapsulates the result of parsing a subaward worksheet, including output metadata.
using System.Collections.Generic;

namespace SubawardReader.Parsing
{
    public class SubawardParseResult
    {
        public List<Models.SubawardRow> Rows { get; set; } = new();
        public List<string> TotalColumnOrder { get; set; } = new();
        public bool IsMergedTotalHeader { get; set; }
        public List<string>? Subheadings { get; set; }
        public string? DebugInfo { get; set; }
    }
}
