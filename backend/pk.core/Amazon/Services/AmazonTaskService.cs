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
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    private readonly AppDbContext _dbContext;
    private readonly ILogger<AmazonTaskService> _logger;

    public AmazonTaskService(AppDbContext dbContext, ILogger<AmazonTaskService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

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

    private static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, JsonOptions);
    }
}

public readonly record struct AmazonTaskUpsertResult(int Requested, int Created, int Updated, int DbChanges)
{
    public static AmazonTaskUpsertResult Empty => new(0, 0, 0, 0);
}
