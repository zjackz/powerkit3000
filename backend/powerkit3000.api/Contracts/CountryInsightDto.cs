namespace powerkit3000.api.Contracts;

public class CountryInsightDto
{
    public required string Country { get; init; }
    public int TotalProjects { get; init; }
    public int SuccessfulProjects { get; init; }
    public decimal SuccessRate { get; init; }
    public decimal TotalPledged { get; init; }
}
