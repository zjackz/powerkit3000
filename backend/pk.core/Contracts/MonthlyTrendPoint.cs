namespace pk.core.contracts;

public class MonthlyTrendPoint
{
    public required int Year { get; init; }
    public required int Month { get; init; }
    public int TotalProjects { get; init; }
    public int SuccessfulProjects { get; init; }
    public decimal TotalPledged { get; init; }
}
