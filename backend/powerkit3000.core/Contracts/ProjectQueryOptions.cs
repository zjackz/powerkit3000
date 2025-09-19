namespace powerkit3000.core.contracts;

public class ProjectQueryOptions
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 200;

    public string? Search { get; set; }
    public IReadOnlyCollection<string>? States { get; set; }
    public IReadOnlyCollection<string>? Countries { get; set; }
    public IReadOnlyCollection<string>? Categories { get; set; }
    public decimal? MinGoal { get; set; }
    public decimal? MaxGoal { get; set; }
    public decimal? MinPercentFunded { get; set; }
    public DateTime? LaunchedAfter { get; set; }
    public DateTime? LaunchedBefore { get; set; }

    private int _page = 1;
    public int Page
    {
        get => _page;
        set => _page = value <= 0 ? 1 : value;
    }

    private int _pageSize = DefaultPageSize;
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value <= 0)
            {
                _pageSize = DefaultPageSize;
                return;
            }

            _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
