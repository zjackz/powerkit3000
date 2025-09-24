namespace powerkit3000.core.contracts;

public class AnalyticsFilterOptions
{
    public DateTime? LaunchedAfter { get; set; }
    public DateTime? LaunchedBefore { get; set; }
    public IReadOnlyCollection<string>? Countries { get; set; }
    public IReadOnlyCollection<string>? Categories { get; set; }
    public decimal? MinPercentFunded { get; set; }
}
