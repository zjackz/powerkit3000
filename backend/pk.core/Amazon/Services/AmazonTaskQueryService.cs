using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using pk.core.Amazon.Contracts;
using pk.data;
using pk.data.Models;

namespace pk.core.Amazon.Services;

/// <summary>
/// 提供 Amazon 采集任务的查询与辅助操作。
/// </summary>
public class AmazonTaskQueryService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly AppDbContext _dbContext;

    public AmazonTaskQueryService(AppDbContext dbContext) => _dbContext = dbContext;

    /// <summary>
    /// 查询任务集合。
    /// </summary>
    public async Task<AmazonTaskListResult> GetTasksAsync(AmazonTaskListQuery query, CancellationToken cancellationToken)
    {
        var tasksQuery = _dbContext.AmazonTasks.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status) && !string.Equals(query.Status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var normalized = query.Status.Trim();
            tasksQuery = tasksQuery.Where(t => t.Status == normalized);
        }

        if (!string.IsNullOrWhiteSpace(query.Site))
        {
            var site = query.Site.Trim();
            tasksQuery = tasksQuery.Where(t => t.Site == site);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            tasksQuery = tasksQuery.Where(t =>
                EF.Functions.ILike(t.Name, $"%{term}%") ||
                (t.Notes != null && EF.Functions.ILike(t.Notes, $"%{term}%")));
        }

        var total = await tasksQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        var tasks = await tasksQuery
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var models = tasks.Select(MapToModel).ToList();
        return new AmazonTaskListResult(models, total);
    }

    /// <summary>
    /// 检查任务是否存在。
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.AmazonTasks
            .AsNoTracking()
            .AnyAsync(t => t.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    private static AmazonTaskModel MapToModel(AmazonTask task)
    {
        var categories = DeserializeOrDefault<List<AmazonTaskCategorySelector>>(task.CategoriesJson, []);
        var leaderboards = DeserializeOrDefault<List<string>>(task.LeaderboardsJson, []);
        var schedule = DeserializeOrDefault(task.ScheduleJson, new AmazonTaskSchedule("recurring", string.Empty, "UTC"));
        var limits = DeserializeOrDefault(task.LimitsJson, new AmazonTaskLimits(null, null));
        var filters = DeserializeOrDefault(task.FiltersJson, new AmazonTaskFilterRules(null, null));
        var priceRange = DeserializeOrDefault(task.PriceRangeJson, new AmazonTaskPriceRange(null, null));
        var keywords = DeserializeOrDefault(task.KeywordsJson, new AmazonTaskKeywordRules(Array.Empty<string>(), Array.Empty<string>()));

        return new AmazonTaskModel(
            task.Id,
            task.Name,
            task.Site,
            task.Status,
            categories,
            leaderboards,
            priceRange,
            keywords,
            filters,
            schedule,
            limits,
            task.ProxyPolicy,
            task.Notes,
            task.LlmSummary,
            task.CreatedAt,
            task.UpdatedAt);
    }

    private static T DeserializeOrDefault<T>(string json, T fallback)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return fallback;
        }

        try
        {
            var result = JsonSerializer.Deserialize<T>(json, JsonOptions);
            return result ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }
}

/// <summary>
/// 任务查询条件。
/// </summary>
/// <param name="Status">状态过滤。</param>
/// <param name="Site">站点过滤。</param>
/// <param name="Search">关键字。</param>
public sealed record AmazonTaskListQuery(string? Status, string? Site, string? Search);

/// <summary>
/// 任务查询结果。
/// </summary>
/// <param name="Items">任务集合。</param>
/// <param name="Total">任务总数。</param>
public sealed record AmazonTaskListResult(IReadOnlyList<AmazonTaskModel> Items, int Total);

/// <summary>
/// 任务模型。
/// </summary>
public sealed record AmazonTaskModel(
    Guid Id,
    string Name,
    string Site,
    string Status,
    IReadOnlyList<AmazonTaskCategorySelector> Categories,
    IReadOnlyList<string> Leaderboards,
    AmazonTaskPriceRange PriceRange,
    AmazonTaskKeywordRules Keywords,
    AmazonTaskFilterRules Filters,
    AmazonTaskSchedule Schedule,
    AmazonTaskLimits Limits,
    string ProxyPolicy,
    string? Notes,
    string? Summary,
    DateTime CreatedAt,
    DateTime UpdatedAt);
