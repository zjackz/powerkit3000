namespace pk.api.Contracts;

public class CategoryKeywordDto
{
    public required string Keyword { get; init; }
    public int ProjectCount { get; init; }
    public int OccurrenceCount { get; init; }
    public decimal AveragePercentFunded { get; init; }
}
