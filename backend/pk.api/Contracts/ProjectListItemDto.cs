namespace pk.api.Contracts;

public record ProjectListItemDto(
    long Id,
    string Name,
    string? NameCn,
    string? Blurb,
    string? BlurbCn,
    string CategoryName,
    string Country,
    string State,
    decimal Goal,
    decimal Pledged,
    decimal PercentFunded,
    decimal FundingVelocity,
    int BackersCount,
    string Currency,
    DateTime LaunchedAt,
    DateTime Deadline,
    string CreatorName,
    string? LocationName
);
