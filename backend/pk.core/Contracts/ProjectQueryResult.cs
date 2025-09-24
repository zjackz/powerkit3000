namespace pk.core.contracts;

public class ProjectQueryResult
{
    public required IReadOnlyList<ProjectListItem> Items { get; init; }
    public int TotalCount { get; init; }
    public ProjectQueryStats Stats { get; init; } = new();
}
