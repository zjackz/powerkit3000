namespace powerkit3000.api.Contracts;

public class ProjectQueryResponseDto
{
    public required int Total { get; init; }
    public required IReadOnlyList<ProjectListItemDto> Items { get; init; }
}
