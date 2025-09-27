using System.Collections.Generic;

namespace pk.core.Amazon.Contracts;

/// <summary>
/// 描述 Amazon 采集任务的核心配置，用于序列化存储与调度。
/// </summary>
/// <param name="Name">任务名称。</param>
/// <param name="Site">站点标识。</param>
/// <param name="Categories">类目选择配置。</param>
/// <param name="Leaderboards">榜单类型集合。</param>
/// <param name="PriceRange">价格筛选区间。</param>
/// <param name="Keywords">关键词规则。</param>
/// <param name="Filters">其他过滤规则。</param>
/// <param name="Schedule">调度信息。</param>
/// <param name="Limits">执行限制条件。</param>
/// <param name="ProxyPolicy">代理策略。</param>
/// <param name="Status">任务状态。</param>
/// <param name="Notes">备注信息。</param>
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

/// <summary>
/// 任务选择的类目节点配置。
/// </summary>
public sealed record AmazonTaskCategorySelector(string Type, string Value);

/// <summary>
/// 任务筛选的价格区间。
/// </summary>
public sealed record AmazonTaskPriceRange(decimal? Min, decimal? Max);

/// <summary>
/// 关键词包含/排除规则。
/// </summary>
public sealed record AmazonTaskKeywordRules(IReadOnlyList<string> Include, IReadOnlyList<string> Exclude);

/// <summary>
/// 排序过滤条件（评分、评论数等）。
/// </summary>
public sealed record AmazonTaskFilterRules(decimal? MinRating, int? MinReviews);

/// <summary>
/// 调度信息，包括类型、Cron 与时区。
/// </summary>
public sealed record AmazonTaskSchedule(string Type, string Cron, string TimeZone);

/// <summary>
/// 任务执行的限制条件（最大商品数/请求频率）。
/// </summary>
public sealed record AmazonTaskLimits(int? MaxProducts, int? MaxRequestsPerHour);
