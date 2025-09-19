namespace powerkit3000.core.contracts;

public class ProjectQueryResult
{
    public required IReadOnlyList<ProjectListItem> Items { get; init; }
    public int TotalCount { get; init; }
}
