using SubawardReader.Models;

namespace SubawardReader.Parsing;

public interface ISubawardParser
{
    IEnumerable<SubawardRow> Parse(string filePath);
}
