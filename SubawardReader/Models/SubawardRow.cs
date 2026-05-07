using System.Collections.Generic;

namespace SubawardReader.Models;

public record SubawardRow(
	string FileName,
	string RecipientName,
	IReadOnlyDictionary<string, decimal> Amounts
);
