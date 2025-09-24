namespace powerkit3000.api.Contracts;

public class UpsertFavoriteRequest
{
    public required string ClientId { get; init; }
    public required long ProjectId { get; init; }
    public string? Note { get; init; }
}
