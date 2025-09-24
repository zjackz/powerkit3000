using System.Collections.Generic;

namespace pk.core.Amazon.Contracts;

/// <summary>
/// 描述 Amazon 采集任务的核心配置，用于序列化存储与调度。
/// </summary>
public sealed record AmazonTaskDefinition(
    string Name,
    string Site,
    IReadOnlyList<AmazonTaskCategorySelector> Categories,
    IReadOnlyList<string> Leaderboards,
    AmazonTaskPriceRange PriceRange,
    AmazonTaskKeywordRules Keywords,
    AmazonTaskFilterRules Filters,
    AmazonTaskSchedule Schedule,
    AmazonTaskLimits Limits,
    string ProxyPolicy,
    string Status,
    string? Notes = null);

public sealed record AmazonTaskCategorySelector(string Type, string Value);

public sealed record AmazonTaskPriceRange(decimal? Min, decimal? Max);

public sealed record AmazonTaskKeywordRules(IReadOnlyList<string> Include, IReadOnlyList<string> Exclude);

public sealed record AmazonTaskFilterRules(decimal? MinRating, int? MinReviews);

public sealed record AmazonTaskSchedule(string Type, string Cron, string TimeZone);

public sealed record AmazonTaskLimits(int? MaxProducts, int? MaxRequestsPerHour);
