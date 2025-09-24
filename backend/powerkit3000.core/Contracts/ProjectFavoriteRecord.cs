namespace powerkit3000.core.contracts;

public record ProjectFavoriteRecord(
    int Id,
    string ClientId,
    ProjectListItem Project,
    string? Note,
    DateTime SavedAt
);
