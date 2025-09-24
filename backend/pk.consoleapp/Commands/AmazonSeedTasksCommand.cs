using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using pk.core.Amazon.Contracts;
using pk.core.Amazon.Services;

namespace consoleapp.Commands;

/// <summary>
/// 初始化或更新默认的 Amazon 采集任务配置。
/// </summary>
public class AmazonSeedTasksCommand
{
    private readonly AmazonTaskService _taskService;
    private readonly ILogger<Program> _logger;

    public AmazonSeedTasksCommand(AmazonTaskService taskService, ILogger<Program> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var definitions = BuildDefaultDefinitions();
        var result = await _taskService.UpsertTasksAsync(definitions, cancellationToken);
        Console.WriteLine($"已同步 {result.Requested} 个任务（新增 {result.Created}，更新 {result.Updated}）。");
    }

    private static IReadOnlyList<AmazonTaskDefinition> BuildDefaultDefinitions()
    {
        var categoriesHome = new List<AmazonTaskCategorySelector>
        {
            new("url", "https://www.amazon.com/gp/new-releases/home-garden/ref=zg_bsnr_nav_home-garden_0")
        };

        var categoriesGarden = new List<AmazonTaskCategorySelector>
        {
            new("url", "https://www.amazon.com/Patio-Lawn-Garden/b/ref=dp_bc_1?ie=UTF8&node=2972638011")
        };

        var schedule = new AmazonTaskSchedule("recurring", "0 30 2 * * *", "UTC");
        var limits = new AmazonTaskLimits(200, 400);
        var keywords = new AmazonTaskKeywordRules(Array.Empty<string>(), Array.Empty<string>());
        var filters = new AmazonTaskFilterRules(4.0m, 25);
        var priceRange = new AmazonTaskPriceRange(30m, 50m);

        return new List<AmazonTaskDefinition>
        {
            new(
                Name: "home_new_releases_30_50",
                Site: "amazon.com",
                Categories: categoriesHome,
                Leaderboards: new[] { "NewReleases" },
                PriceRange: priceRange,
                Keywords: keywords,
                Filters: filters,
                Schedule: schedule,
                Limits: limits,
                ProxyPolicy: "default",
                Status: "active",
                Notes: "MVP: 家居新上架（30-50 USD）"
            ),
            new(
                Name: "garden_best_sellers_30_50",
                Site: "amazon.com",
                Categories: categoriesGarden,
                Leaderboards: new[] { "BestSellers" },
                PriceRange: priceRange,
                Keywords: keywords,
                Filters: filters,
                Schedule: schedule,
                Limits: limits,
                ProxyPolicy: "default",
                Status: "active",
                Notes: "MVP: 花园热销（30-50 USD）"
            )
        };
    }
}
