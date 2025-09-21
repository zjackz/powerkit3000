namespace powerkit3000.api.Contracts;

public class ProjectFiltersDto
{
    public required IReadOnlyList<FilterOptionDto> States { get; init; }
    public required IReadOnlyList<FilterOptionDto> Countries { get; init; }
    public required IReadOnlyList<FilterOptionDto> Categories { get; init; }
}
