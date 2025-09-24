namespace powerkit3000.api.Contracts;

public class ProjectHighlightDto
{
    public long Id { get; init; }
    public required string Name { get; init; }
    public string? NameCn { get; init; }
    public required string CategoryName { get; init; }
    public required string Country { get; init; }
    public decimal PercentFunded { get; init; }
    public decimal Pledged { get; init; }
    public decimal FundingVelocity { get; init; }
    public int BackersCount { get; init; }
    public required string Currency { get; init; }
    public DateTime LaunchedAt { get; init; }
}
