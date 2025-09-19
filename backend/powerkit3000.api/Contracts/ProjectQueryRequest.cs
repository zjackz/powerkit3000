namespace powerkit3000.api.Contracts;

public class ProjectQueryRequest
{
    public string? Search { get; set; }
    public IEnumerable<string>? States { get; set; }
    public IEnumerable<string>? Countries { get; set; }
    public IEnumerable<string>? Categories { get; set; }
    public decimal? MinGoal { get; set; }
    public decimal? MaxGoal { get; set; }
    public decimal? MinPercentFunded { get; set; }
    public DateTime? LaunchedAfter { get; set; }
    public DateTime? LaunchedBefore { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
