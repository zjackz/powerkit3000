using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pk.core.Amazon.Contracts;
using pk.data;
using pk.data.Models;

namespace pk.core.Amazon.Services;

/// <summary>
/// 负责将 Amazon 采集任务定义持久化到数据库的服务。
/// </summary>
public class AmazonTaskService
{
    /// <summary>
    /// 序列化任务配置时使用的统一 JSON 选项。
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    private readonly AppDbContext _dbContext;
    private readonly ILogger<AmazonTaskService> _logger;

    /// <summary>
    /// 初始化 <see cref="AmazonTaskService"/>。
    /// </summary>
    public AmazonTaskService(AppDbContext dbContext, ILogger<AmazonTaskService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 批量创建或更新采集任务定义。
    /// </summary>
    /// <param name="definitions">任务定义集合。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>包含影响行数的结果。</returns>
    public async Task<AmazonTaskUpsertResult> UpsertTasksAsync(IEnumerable<AmazonTaskDefinition> definitions, CancellationToken cancellationToken)
    {
        var definitionList = definitions.ToList();
        if (definitionList.Count == 0)
        {
            return AmazonTaskUpsertResult.Empty;
        }

        var names = definitionList.Select(d => d.Name).ToArray();
        var existing = await _dbContext.AmazonTasks
            .Where(t => names.Contains(t.Name))
            .ToDictionaryAsync(t => t.Name, cancellationToken);

        var now = DateTime.UtcNow;
        var created = 0;
        var updated = 0;

        foreach (var definition in definitionList)
        {
            if (!existing.TryGetValue(definition.Name, out var task))
            {
                task = new AmazonTask
                {
                    Id = Guid.NewGuid(),
                    Name = definition.Name,
                    CreatedAt = now,
                };
                _dbContext.AmazonTasks.Add(task);
                existing[definition.Name] = task;
                created++;
            }
            else
            {
                updated++;
            }

            task.Site = definition.Site;
            task.CategoriesJson = Serialize(definition.Categories);
            task.LeaderboardsJson = Serialize(definition.Leaderboards);
            task.PriceRangeJson = Serialize(definition.PriceRange);
            task.KeywordsJson = Serialize(definition.Keywords);
            task.FiltersJson = Serialize(definition.Filters);
            task.ScheduleJson = Serialize(definition.Schedule);
            task.LimitsJson = Serialize(definition.Limits);
            task.ProxyPolicy = definition.ProxyPolicy;
            task.Status = definition.Status;
            task.Notes = definition.Notes;
            task.UpdatedAt = now;
        }

        var affected = await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Upserted {Count} Amazon tasks (created {Created}, updated {Updated}, db changes {Affected}).", definitionList.Count, created, updated, affected);
        return new AmazonTaskUpsertResult(definitionList.Count, created, updated, affected);
    }

    /// <summary>
    /// 使用统一配置序列化对象。
    /// </summary>
    private static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, JsonOptions);
    }
}

/// <summary>
/// 保存任务定义时的影响统计。
/// </summary>
/// <param name="Requested">请求的任务数量。</param>
/// <param name="Created">新增任务数量。</param>
/// <param name="Updated">更新任务数量。</param>
/// <param name="DbChanges">EF Core 保存的变更条数。</param>
public readonly record struct AmazonTaskUpsertResult(int Requested, int Created, int Updated, int DbChanges)
{
    /// <summary>
    /// 表示无操作的默认结果。
    /// </summary>
    public static AmazonTaskUpsertResult Empty => new(0, 0, 0, 0);
}
