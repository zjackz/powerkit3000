namespace powerkit3000.api.Contracts;

public record ProjectListItemDto(
    long Id,
    string Name,
    string? Blurb,
    string CategoryName,
    string Country,
    string State,
    decimal Goal,
    decimal Pledged,
    decimal PercentFunded,
    int BackersCount,
    string Currency,
    DateTime LaunchedAt,
    DateTime Deadline,
    string CreatorName,
    string? LocationName
);
