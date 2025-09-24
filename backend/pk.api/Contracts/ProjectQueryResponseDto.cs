namespace pk.api.Contracts;

public class ProjectQueryResponseDto
{
    public required int Total { get; init; }
    public required IReadOnlyList<ProjectListItemDto> Items { get; init; }
    public required ProjectQueryStatsDto Stats { get; init; }
}
