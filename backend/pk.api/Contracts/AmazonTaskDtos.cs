using System;
using System.Collections.Generic;
using pk.core.Amazon.Contracts;

namespace pk.api.Contracts;

/// <summary>
/// Amazon 任务查询请求。
/// </summary>
/// <param name="Status">状态过滤。</param>
/// <param name="Site">站点过滤。</param>
/// <param name="Search">关键字。</param>
public sealed record AmazonTaskQueryRequest(string? Status, string? Site, string? Search);

/// <summary>
/// Amazon 任务列表响应。
/// </summary>
public sealed class AmazonTaskListResponseDto
{
    public IReadOnlyList<AmazonTaskListItemDto> Items { get; init; } = Array.Empty<AmazonTaskListItemDto>();

    public int Total { get; init; }
}

/// <summary>
/// Amazon 任务展示 DTO。
/// </summary>
/// <param name="Id">唯一标识。</param>
/// <param name="Name">任务名称。</param>
/// <param name="Site">站点。</param>
/// <param name="Status">任务状态。</param>
/// <param name="Categories">类目选择。</param>
/// <param name="Leaderboards">榜单类型。</param>
/// <param name="Schedule">调度信息。</param>
/// <param name="ProxyPolicy">代理策略。</param>
/// <param name="Notes">备注。</param>
/// <param name="CreatedAt">创建时间。</param>
/// <param name="UpdatedAt">最近更新时间。</param>
public sealed record AmazonTaskListItemDto(
    Guid Id,
    string Name,
    string Site,
    string Status,
    IReadOnlyList<AmazonTaskCategorySelector> Categories,
    IReadOnlyList<string> Leaderboards,
    AmazonTaskSchedule Schedule,
    AmazonTaskPriceRange PriceRange,
    AmazonTaskFilterRules Filters,
    AmazonTaskKeywordRules Keywords,
    AmazonTaskLimits Limits,
    string ProxyPolicy,
    string? Notes,
    string? Summary,
    DateTime CreatedAt,
    DateTime UpdatedAt);
