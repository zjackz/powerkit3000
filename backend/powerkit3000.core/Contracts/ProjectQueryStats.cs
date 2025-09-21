namespace powerkit3000.core.contracts;

public class ProjectQueryStats
{
    public int SuccessfulCount { get; init; }
    public decimal TotalPledged { get; init; }
    public decimal AveragePercentFunded { get; init; }
    public int TotalBackers { get; init; }
    public decimal AverageGoal { get; init; }
    public ProjectListItem? TopProject { get; init; }
}
