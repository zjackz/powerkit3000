namespace pk.api.Contracts;

public class MonthlyTrendPointDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int TotalProjects { get; init; }
    public int SuccessfulProjects { get; init; }
    public decimal TotalPledged { get; init; }
}
