namespace powerkit3000.api.Contracts;

public class FundingDistributionDto
{
    public required string Label { get; init; }
    public decimal MinPercent { get; init; }
    public decimal MaxPercent { get; init; }
    public int TotalProjects { get; init; }
    public int SuccessfulProjects { get; init; }
}
