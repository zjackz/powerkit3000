namespace pk.core.contracts;

public class CategoryInsight
{
    public required string CategoryName { get; init; }
    public int TotalProjects { get; init; }
    public int SuccessfulProjects { get; init; }
    public decimal SuccessRate { get; init; }
    public decimal AveragePercentFunded { get; init; }
    public decimal TotalPledged { get; init; }
}
