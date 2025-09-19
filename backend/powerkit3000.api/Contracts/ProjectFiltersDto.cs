namespace powerkit3000.api.Contracts;

public class ProjectFiltersDto
{
    public required IReadOnlyList<string> States { get; init; }
    public required IReadOnlyList<string> Countries { get; init; }
    public required IReadOnlyList<string> Categories { get; init; }
}
