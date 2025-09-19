namespace powerkit3000.api.Contracts;

public class AnalyticsFilterRequest
{
    public DateTime? LaunchedAfter { get; set; }
    public DateTime? LaunchedBefore { get; set; }
    public IEnumerable<string>? Countries { get; set; }
    public IEnumerable<string>? Categories { get; set; }
}
