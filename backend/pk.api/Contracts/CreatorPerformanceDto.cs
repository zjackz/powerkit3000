namespace pk.api.Contracts;

public class CreatorPerformanceDto
{
    public long CreatorId { get; init; }
    public required string CreatorName { get; init; }
    public int TotalProjects { get; init; }
    public int SuccessfulProjects { get; init; }
    public decimal SuccessRate { get; init; }
    public decimal AveragePercentFunded { get; init; }
    public decimal TotalPledged { get; init; }
}
