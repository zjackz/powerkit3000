namespace powerkit3000.api.Contracts;

public class ProjectQueryStatsDto
{
    public int SuccessfulCount { get; init; }
    public decimal TotalPledged { get; init; }
    public decimal AveragePercentFunded { get; init; }
    public int TotalBackers { get; init; }
    public decimal AverageGoal { get; init; }
    public ProjectListItemDto? TopProject { get; init; }
}
