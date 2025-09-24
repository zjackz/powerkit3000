namespace pk.api.Contracts;

public class ProjectFavoriteDto
{
    public required int Id { get; init; }
    public required string ClientId { get; init; }
    public required ProjectListItemDto Project { get; init; }
    public string? Note { get; init; }
    public DateTime SavedAt { get; init; }
}
