using System.Linq;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using pk.api.Contracts;
using pk.api.Jobs;
using pk.api.Monitoring;
using pk.core.Amazon.Options;
using pk.core.Amazon.Services;
using pk.core.Amazon.Operations;
using pk.core.Amazon.Contracts;
using pk.core.Amazon;
using pk.core.contracts;
using pk.core.services;
using pk.data;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId();
    }, preserveStaticLogger: true);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var connectionString = builder.Configuration.GetConnectionString("AppDb")
        ?? throw new InvalidOperationException("Connection string 'AppDb' was not found.");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));

    builder.Services.AddScoped<KickstarterProjectQueryService>();
    builder.Services.AddScoped<ProjectFavoriteService>();

    builder.Services.Configure<AmazonModuleOptions>(builder.Configuration.GetSection(AmazonModuleOptions.SectionName));
    builder.Services.Configure<AmazonOperationalDashboardOptions>(builder.Configuration.GetSection(AmazonOperationalDashboardOptions.SectionName));
    builder.Services.AddHttpClient<IAmazonBestsellerSource, HtmlAgilityPackAmazonBestsellerSource>();
    builder.Services.AddSingleton<IAmazonOperationalDataSource, NoopAmazonOperationalDataSource>();
    builder.Services.AddScoped<AmazonIngestionService>();
    builder.Services.AddScoped<AmazonTrendAnalysisService>();
    builder.Services.AddScoped<AmazonReportingService>();
    builder.Services.AddScoped<AmazonDashboardService>();
    builder.Services.AddScoped<AmazonRecurringJobService>();
    builder.Services.AddScoped<AmazonOperationalIngestionService>();
    builder.Services.AddScoped<AmazonOperationalInsightService>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("tradeforge", policy =>
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>("database");

    builder.Services.AddSingleton<MetricsSnapshotService>();
    builder.Services.AddHostedService(provider => provider.GetRequiredService<MetricsSnapshotService>());

    var hangfireDisabled = builder.Configuration.GetValue("Hangfire:Disabled", false);
    if (hangfireDisabled)
    {
        Log.Information("Hangfire services are disabled via configuration.");
    }

    if (!hangfireDisabled)
    {
        builder.Services.AddHangfire(config =>
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(connectionString));

        builder.Services.AddHangfireServer();
        builder.Services.AddScoped<AmazonJobScheduler>();
    }

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            if (db.Database.IsRelational())
            {
                db.Database.Migrate();
            }
        }
        catch (Exception migrateEx)
        {
            Log.Fatal(migrateEx, "数据库迁移失败");
            throw;
        }

        if (!hangfireDisabled)
        {
            var scheduler = scope.ServiceProvider.GetRequiredService<AmazonJobScheduler>();
            await scheduler.InitializeAsync(CancellationToken.None);
        }
    }

    app.UseCors("tradeforge");

    if (!app.Environment.IsProduction())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.MapHealthChecks("/health").WithName("Health").WithTags("Monitoring");
    app.MapGet("/monitoring/metrics", (MetricsSnapshotService metrics) => Results.Ok(metrics.CreateSnapshot()))
        .WithName("GetMetrics").WithTags("Monitoring")
        .WithOpenApi(operation =>
        {
            operation.Summary = "获取运行时指标快照";
            operation.Description = "返回当前累积的计数指标与直方图数据，用于 CLI 或监控面板展示。";
            return operation;
        });

    // 暂未加权限控制，生产环境需加认证后再开放仪表盘。
    if (!hangfireDisabled)
    {
        app.UseHangfireDashboard("/hangfire");
    }

app.MapGet("/projects", async (
    [AsParameters] ProjectQueryRequest request,
    KickstarterProjectQueryService queryService,
    CancellationToken cancellationToken) =>
{
    var options = new ProjectQueryOptions
    {
        Search = request.Search,
        States = request.States?.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToArray(),
        Countries = request.Countries?.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToArray(),
        Categories = request.Categories?.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToArray(),
        MinGoal = request.MinGoal,
        MaxGoal = request.MaxGoal,
        MinPercentFunded = request.MinPercentFunded,
        LaunchedAfter = request.LaunchedAfter,
        LaunchedBefore = request.LaunchedBefore,
    };

    if (request.Page.HasValue)
    {
        options.Page = request.Page.Value;
    }

    if (request.PageSize.HasValue)
    {
        options.PageSize = request.PageSize.Value;
    }

    var result = await queryService.QueryAsync(options, cancellationToken);

    var items = result.Items
            .Select(MapProjectToDto)
        .ToList();

    var topProjectDto = result.Stats.TopProject is not null
        ? MapProjectToDto(result.Stats.TopProject)
        : null;

    var response = new ProjectQueryResponseDto
    {
        Total = result.TotalCount,
        Items = items,
        Stats = new ProjectQueryStatsDto
        {
            SuccessfulCount = result.Stats.SuccessfulCount,
            TotalPledged = result.Stats.TotalPledged,
            AveragePercentFunded = result.Stats.AveragePercentFunded,
            TotalBackers = result.Stats.TotalBackers,
            AverageGoal = result.Stats.AverageGoal,
            TopProject = topProjectDto,
        }
    };

    return Results.Ok(response);
})
.WithName("GetProjects")
.WithOpenApi(operation =>
{
    operation.Summary = "分页检索 Kickstarter 项目";
    operation.Description = "支持按状态、国家、类别、金额、上线时间等条件过滤，并返回分页结果与统计信息。";
    return operation;
});

app.MapGet("/filters", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
{
    var stateCounts = await dbContext.KickstarterProjects
        .AsNoTracking()
        .Where(p => p.State != null && p.State != string.Empty)
        .GroupBy(p => p.State!)
        .Select(g => new { Value = g.Key, Count = g.Count() })
        .OrderByDescending(x => x.Count)
        .ThenBy(x => x.Value)
        .ToListAsync(cancellationToken);

    var countryCounts = await dbContext.KickstarterProjects
        .AsNoTracking()
        .Where(p => p.Country != null && p.Country != string.Empty)
        .GroupBy(p => new { p.Country, p.CountryDisplayableName })
        .Select(g => new
        {
            Value = g.Key.Country!,
            Label = g.Key.CountryDisplayableName ?? g.Key.Country!,
            Count = g.Count()
        })
        .OrderByDescending(x => x.Count)
        .ThenBy(x => x.Label)
        .ToListAsync(cancellationToken);

    var categoryCounts = await dbContext.KickstarterProjects
        .AsNoTracking()
        .Where(p => p.Category != null && p.Category.Name != null && p.Category.Name != string.Empty)
        .GroupBy(p => p.Category!.Name!)
        .Select(g => new
        {
            Value = g.Key,
            Label = g.Key,
            Count = g.Count()
        })
        .OrderByDescending(x => x.Count)
        .ThenBy(x => x.Label)
        .ToListAsync(cancellationToken);

    var response = new ProjectFiltersDto
    {
        States = stateCounts
            .Select(s => new FilterOptionDto(s.Value, s.Value, s.Count))
            .ToList(),
        Countries = countryCounts
            .Select(c => new FilterOptionDto(c.Value, c.Label, c.Count))
            .ToList(),
        Categories = categoryCounts
            .Select(c => new FilterOptionDto(c.Value, c.Label, c.Count))
            .ToList(),
    };

    return Results.Ok(response);
}).WithName("GetFilters").WithTags("Projects").WithOpenApi(operation =>
{
    operation.Summary = "获取项目筛选器选项";
    operation.Description = "统计状态、国家、类别的可选项及数量，用于前端构建筛选器。";
    return operation;
});

app.MapGet("/projects/summary", async (
    [AsParameters] AnalyticsFilterRequest request,
    KickstarterProjectQueryService queryService,
    CancellationToken cancellationToken) =>
{
    var summary = await queryService.GetSummaryAsync(MapFilters(request), cancellationToken);

    return Results.Ok(new ProjectSummaryDto
    {
        TotalProjects = summary.TotalProjects,
        SuccessfulProjects = summary.SuccessfulProjects,
        TotalPledged = summary.TotalPledged,
        DistinctCountries = summary.DistinctCountries,
    });
}).WithName("GetProjectSummary").WithTags("Projects").WithOpenApi(operation =>
{
    operation.Summary = "获取项目概览指标";
    operation.Description = "根据筛选条件返回项目总量、成功量、筹资金额等总结数据。";
    return operation;
});

static AnalyticsFilterOptions MapFilters(AnalyticsFilterRequest request) => new()
{
    LaunchedAfter = request.LaunchedAfter,
    LaunchedBefore = request.LaunchedBefore,
    Countries = request.Countries?.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).ToArray(),
    Categories = request.Categories?.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).ToArray(),
    MinPercentFunded = request.MinPercentFunded,
};

app.MapGet("/analytics/categories", async (
    int? minProjects,
    [AsParameters] AnalyticsFilterRequest request,
    KickstarterProjectQueryService queryService,
    CancellationToken cancellationToken) =>
{
    var result = await queryService.GetCategoryInsightsAsync(minProjects ?? 5, MapFilters(request), cancellationToken);

    return Results.Ok(result.Select(item => new CategoryInsightDto
    {
        CategoryName = item.CategoryName,
        TotalProjects = item.TotalProjects,
        SuccessfulProjects = item.SuccessfulProjects,
        SuccessRate = item.SuccessRate,
        AveragePercentFunded = item.AveragePercentFunded,
        TotalPledged = item.TotalPledged,
    }));
}).WithName("GetCategoryInsights").WithTags("Analytics").WithOpenApi(operation =>
{
    operation.Summary = "按类别统计项目表现";
    operation.Description = "返回各类别的项目数量、成功率与筹资金额，可指定最小样本数量。";
    return operation;
});

app.MapGet("/analytics/countries", async (
    int? minProjects,
    [AsParameters] AnalyticsFilterRequest request,
    KickstarterProjectQueryService queryService,
    CancellationToken cancellationToken) =>
{
    var result = await queryService.GetCountryInsightsAsync(minProjects ?? 5, MapFilters(request), cancellationToken);

    return Results.Ok(result.Select(item => new CountryInsightDto
    {
        Country = item.Country,
        TotalProjects = item.TotalProjects,
        SuccessfulProjects = item.SuccessfulProjects,
        SuccessRate = item.SuccessRate,
        TotalPledged = item.TotalPledged,
    }));
}).WithName("GetCountryInsights").WithTags("Analytics").WithOpenApi(operation =>
{
    operation.Summary = "按国家统计项目表现";
    operation.Description = "返回各国家的项目数量、成功率与筹资金额，可指定最小样本数量。";
    return operation;
});

app.MapGet("/analytics/top-projects", async (
    int? limit,
    [AsParameters] AnalyticsFilterRequest request,
    KickstarterProjectQueryService queryService,
    CancellationToken cancellationToken) =>
{
    var result = await queryService.GetTopProjectsAsync(limit ?? 10, MapFilters(request), cancellationToken);

    return Results.Ok(result.Select(MapHighlightToDto));
}).WithName("GetTopProjects").WithTags("Analytics").WithOpenApi(operation =>
{
    operation.Summary = "获取表现最佳项目";
    operation.Description = "按筹资表现排序返回高潜项目列表，可设置返回数量。";
    return operation;
});

app.MapGet("/analytics/hype", async (
    int? limit,
    decimal? minPercentFunded,
    [AsParameters] AnalyticsFilterRequest request,
    KickstarterProjectQueryService queryService,
    CancellationToken cancellationToken) =>
{
    var result = await queryService.GetHypeProjectsAsync(limit ?? 10, minPercentFunded, MapFilters(request), cancellationToken);

    return Results.Ok(result.Select(MapHighlightToDto));
}).WithName("GetHypeProjects").WithTags("Analytics").WithOpenApi(operation =>
{
    operation.Summary = "获取爆款潜力项目";
    operation.Description = "结合达成率与筹资速度筛选热点项目，可限制数量和达成率门槛。";
    return operation;
});

app.MapGet("/analytics/category-keywords", async (
    string? category,
    int? top,
    [AsParameters] AnalyticsFilterRequest request,
    KickstarterProjectQueryService queryService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(category))
    {
        return Results.BadRequest("Category is required.");
    }

    var result = await queryService.GetCategoryKeywordsAsync(category, top ?? 30, MapFilters(request), cancellationToken);

    return Results.Ok(result.Select(item => new CategoryKeywordDto
    {
        Keyword = item.Keyword,
        ProjectCount = item.ProjectCount,
        OccurrenceCount = item.OccurrenceCount,
        AveragePercentFunded = item.AveragePercentFunded,
    }));
}).WithName("GetCategoryKeywords").WithTags("Analytics").WithOpenApi(operation =>
{
    operation.Summary = "统计类别关键词";
    operation.Description = "针对指定类别生成关键词云，包括词频、覆盖项目数与平均达成率。";
    return operation;
});

app.MapGet("/favorites", async (
    string clientId,
    ProjectFavoriteService favoriteService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(clientId))
    {
        return Results.BadRequest("clientId is required");
    }

    var favorites = await favoriteService.GetFavoritesAsync(clientId, cancellationToken);

    return Results.Ok(favorites.Select(MapFavoriteToDto));
}).WithName("GetFavorites").WithTags("Favorites").WithOpenApi(operation =>
{
    operation.Summary = "查询收藏列表";
    operation.Description = "按客户端标识返回已收藏的项目及备注信息。";
    return operation;
});

app.MapPost("/favorites", async (
    UpsertFavoriteRequest request,
    ProjectFavoriteService favoriteService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.ClientId))
    {
        return Results.BadRequest("clientId is required");
    }

    if (request.ProjectId <= 0)
    {
        return Results.BadRequest("projectId must be greater than zero");
    }

    var favorite = await favoriteService.UpsertAsync(request.ClientId, request.ProjectId, request.Note, cancellationToken);
    if (favorite is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(MapFavoriteToDto(favorite));
}).WithName("SaveFavorite").WithTags("Favorites").WithOpenApi(operation =>
{
    operation.Summary = "新增或更新收藏";
    operation.Description = "根据客户端标识与项目 ID 创建或更新收藏记录，支持备注。";
    return operation;
});

app.MapDelete("/favorites/{projectId:long}", async (
    string clientId,
    long projectId,
    ProjectFavoriteService favoriteService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(clientId))
    {
        return Results.BadRequest("clientId is required");
    }

    var removed = await favoriteService.RemoveAsync(clientId, projectId, cancellationToken);
    return removed ? Results.NoContent() : Results.NotFound();
}).WithName("DeleteFavorite").WithTags("Favorites").WithOpenApi(operation =>
{
    operation.Summary = "删除指定收藏";
    operation.Description = "按客户端标识和项目 ID 移除收藏记录。";
    return operation;
});

app.MapDelete("/favorites", async (
    string clientId,
    ProjectFavoriteService favoriteService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(clientId))
    {
        return Results.BadRequest("clientId is required");
    }

    var removed = await favoriteService.ClearAsync(clientId, cancellationToken);
    return Results.Ok(new { removed });
}).WithName("ClearFavorites").WithTags("Favorites").WithOpenApi(operation =>
{
    operation.Summary = "清空收藏列表";
    operation.Description = "删除客户端下所有收藏，并返回移除数量。";
    return operation;
});

app.MapGet("/analytics/monthly-trend", async (
    int? months,
    [AsParameters] AnalyticsFilterRequest request,
    KickstarterProjectQueryService queryService,
    CancellationToken cancellationToken) =>
{
    var result = await queryService.GetMonthlyTrendAsync(MapFilters(request), months ?? 12, cancellationToken);

    return Results.Ok(result.Select(item => new MonthlyTrendPointDto
    {
        Year = item.Year,
        Month = item.Month,
        TotalProjects = item.TotalProjects,
        SuccessfulProjects = item.SuccessfulProjects,
        TotalPledged = item.TotalPledged,
    }));
}).WithName("GetMonthlyTrend").WithTags("Analytics").WithOpenApi(operation =>
{
    operation.Summary = "获取月度趋势";
    operation.Description = "返回按月汇总的项目数量、成功数量及筹资金额，默认追溯 12 个月。";
    return operation;
});

app.MapGet("/analytics/funding-distribution", async (
    [AsParameters] AnalyticsFilterRequest request,
    KickstarterProjectQueryService queryService,
    CancellationToken cancellationToken) =>
{
    var result = await queryService.GetFundingDistributionAsync(MapFilters(request), cancellationToken);

    return Results.Ok(result.Select(item => new FundingDistributionDto
    {
        Label = item.Label,
        MinPercent = item.MinPercent,
        MaxPercent = item.MaxPercent,
        TotalProjects = item.TotalProjects,
        SuccessfulProjects = item.SuccessfulProjects,
    }));
}).WithName("GetFundingDistribution").WithTags("Analytics").WithOpenApi(operation =>
{
    operation.Summary = "查看筹资分布";
    operation.Description = "根据达成率区间统计项目数量与成功数量。";
    return operation;
});

app.MapGet("/analytics/creators", async (
    int? minProjects,
    int? limit,
    [AsParameters] AnalyticsFilterRequest request,
    KickstarterProjectQueryService queryService,
    CancellationToken cancellationToken) =>
{
    var result = await queryService.GetTopCreatorsAsync(minProjects ?? 3, limit ?? 10, MapFilters(request), cancellationToken);

    return Results.Ok(result.Select(item => new CreatorPerformanceDto
    {
        CreatorId = item.CreatorId,
        CreatorName = item.CreatorName,
        TotalProjects = item.TotalProjects,
        SuccessfulProjects = item.SuccessfulProjects,
        SuccessRate = item.SuccessRate,
        AveragePercentFunded = item.AveragePercentFunded,
        TotalPledged = item.TotalPledged,
    }));
}).WithName("GetCreatorPerformance").WithTags("Analytics").WithOpenApi(operation =>
{
    operation.Summary = "获取创作者表现榜";
    operation.Description = "按项目数量、成功率等指标返回表现最佳的创作者列表，可设置最小项目数与数量上限。";
    return operation;
});

// -------- Amazon 模块 API：覆盖类目同步、快照采集、趋势与报告查询 --------

app.MapPost("/amazon/categories/ensure", async (AmazonIngestionService ingestionService, CancellationToken cancellationToken) =>
{
    var affected = await ingestionService.EnsureCategoriesAsync(cancellationToken);
    return Results.Ok(new { affected });
}).WithName("EnsureAmazonCategories").WithTags("Amazon").WithOpenApi(operation =>
{
    operation.Summary = "同步 Amazon 类目配置";
    operation.Description = "读取配置文件中的类目列表并写入数据库，存在则更新名称。";
    return operation;
});

app.MapPost("/amazon/snapshots", async (AmazonFetchSnapshotRequest request, AmazonIngestionService ingestionService, CancellationToken cancellationToken) =>
{
    var snapshotId = await ingestionService.CaptureSnapshotAsync(request.CategoryId, request.BestsellerType, cancellationToken);
    return Results.Ok(new { snapshotId });
}).WithName("CreateAmazonSnapshot").WithTags("Amazon").WithOpenApi(operation =>
{
    operation.Summary = "在线抓取 Amazon 榜单快照";
    operation.Description = "根据内部类目主键发起抓取任务，成功后返回快照 ID。";
    return operation;
});

app.MapPost("/amazon/snapshots/import", async (
    AmazonImportSnapshotRequest request,
    AmazonIngestionService ingestionService,
    AppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    if (request.Entries == null || request.Entries.Count == 0)
    {
        return Results.BadRequest(new { error = "Entries must contain at least one item." });
    }

    int? categoryId = request.CategoryId;
    if (!categoryId.HasValue)
    {
        if (string.IsNullOrWhiteSpace(request.AmazonCategoryId))
        {
            return Results.BadRequest(new { error = "CategoryId or AmazonCategoryId is required." });
        }

        categoryId = await dbContext.AmazonCategories
            .Where(c => c.AmazonCategoryId == request.AmazonCategoryId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!categoryId.HasValue)
        {
            return Results.BadRequest(new { error = $"Amazon category '{request.AmazonCategoryId}' does not exist." });
        }
    }

    var capturedAt = request.CapturedAt ?? DateTime.UtcNow;
    var entries = request.Entries
        .Select(e => new AmazonBestsellerEntry(
            e.Asin,
            e.Title,
            e.Brand,
            e.ImageUrl,
            e.Rank,
            e.Price,
            e.Rating,
            e.ReviewsCount,
            e.ListingDate))
        .ToList();

    var importModel = new AmazonSnapshotImportModel(categoryId.Value, request.BestsellerType, capturedAt, entries);
    var snapshotId = await ingestionService.ImportSnapshotAsync(importModel, cancellationToken);
    return Results.Ok(new { snapshotId });
}).WithName("ImportAmazonSnapshot").WithTags("Amazon").WithOpenApi(operation =>
{
    operation.Summary = "导入 Amazon 榜单快照";
    operation.Description = "由外部抓取工具提交榜单数据，生成一条快照并写入商品及数据点。";
    return operation;
});

app.MapPost("/amazon/snapshots/{snapshotId:long}/analyze", async (long snapshotId, AmazonTrendAnalysisService analysisService, CancellationToken cancellationToken) =>
{
    var trendCount = await analysisService.AnalyzeSnapshotAsync(snapshotId, cancellationToken);
    return Results.Ok(new { snapshotId, trendCount });
}).WithName("AnalyzeAmazonSnapshot").WithTags("Amazon").WithOpenApi(operation =>
{
    operation.Summary = "重新计算快照趋势";
    operation.Description = "对指定快照生成新晋上榜、排名飙升等趋势标签。";
    return operation;
});

app.MapGet("/amazon/core-metrics", async (AmazonDashboardService dashboardService, CancellationToken cancellationToken) =>
{
    var metrics = await dashboardService.GetLatestCoreMetricsAsync(cancellationToken);
    return metrics is null
        ? Results.NoContent()
        : Results.Ok(new AmazonCoreMetricsResponseDto(metrics.SnapshotId, metrics.CapturedAt, metrics.TotalProducts, metrics.TotalNewEntries, metrics.TotalRankSurges, metrics.TotalConsistentPerformers));
}).WithName("GetAmazonCoreMetrics").WithTags("Amazon").WithOpenApi(operation =>
{
    operation.Summary = "获取最新 Amazon 核心指标";
    operation.Description = "返回最新榜单快照的核心指标（若无快照则 204）。";
    return operation;
});

app.MapGet("/amazon/products", async ([AsParameters] AmazonProductsQueryRequest request, AmazonDashboardService dashboardService, CancellationToken cancellationToken) =>
{
    var products = await dashboardService.GetProductsAsync(request.CategoryId, request.Search, cancellationToken);
    var response = products.Select(p => new AmazonProductListItemDto(p.Asin, p.Title, p.CategoryName, p.ListingDate, p.LatestRank, p.LatestPrice, p.LatestRating, p.LatestReviewsCount, p.LastUpdated)).ToList();
    return Results.Ok(response);
}).WithName("GetAmazonProducts").WithTags("Amazon").WithOpenApi(operation =>
{
    operation.Summary = "查询 Amazon 榜单商品";
    operation.Description = "按照类目与搜索词筛选商品，并补充最新榜单数据。";
    return operation;
});

app.MapGet("/amazon/trends", async ([AsParameters] AmazonTrendsQueryRequest request, AmazonDashboardService dashboardService, CancellationToken cancellationToken) =>
{
    var trends = await dashboardService.GetLatestTrendsAsync(request.TrendType, cancellationToken);
    var response = trends.Select(t => new AmazonTrendListItemDto(t.Asin, t.Title, t.TrendType, t.Description, t.RecordedAt)).ToList();
    return Results.Ok(response);
}).WithName("GetAmazonTrends").WithTags("Amazon").WithOpenApi(operation =>
{
    operation.Summary = "获取最新 Amazon 趋势列表";
    operation.Description = "返回最新快照的趋势记录，可按趋势类型过滤。";
    return operation;
});

app.MapGet("/amazon/products/{asin}/history", async (string asin, AmazonDashboardService dashboardService, CancellationToken cancellationToken) =>
{
    var history = await dashboardService.GetProductHistoryAsync(asin, cancellationToken);
    var response = history.Select(h => new AmazonProductHistoryPointDto(h.Timestamp, h.Rank, h.Price, h.Rating, h.ReviewsCount)).ToList();
    return response.Count == 0 ? Results.NotFound() : Results.Ok(response);
}).WithName("GetAmazonProductHistory").WithTags("Amazon").WithOpenApi(operation =>
{
    operation.Summary = "查询指定 ASIN 的历史走势";
    operation.Description = "返回指定 ASIN 在历史快照中的排名与价格变化，无数据则返回 404。";
    return operation;
});

app.MapGet("/amazon/report/latest", async (AmazonDashboardService dashboardService, CancellationToken cancellationToken) =>
{
    var report = await dashboardService.GetLatestReportAsync(cancellationToken);
    return report is null
        ? Results.NoContent()
        : Results.Ok(new
        {
            metrics = new AmazonCoreMetricsResponseDto(report.CoreMetrics.SnapshotId, report.CoreMetrics.CapturedAt, report.CoreMetrics.TotalProducts, report.CoreMetrics.TotalNewEntries, report.CoreMetrics.TotalRankSurges, report.CoreMetrics.TotalConsistentPerformers),
            trends = report.Trends.Select(t => new AmazonTrendListItemDto(t.Asin, t.Title, t.TrendType, t.Description, t.RecordedAt)).ToList(),
            report.ReportText
        });
}).WithName("GetLatestAmazonReport").WithTags("Amazon").WithOpenApi(operation =>
{
    operation.Summary = "获取最新 Amazon 榜单报告";
    operation.Description = "返回最新快照的结构化报告，若无快照则 204。";
    return operation;
});

app.MapPost("/amazon/operations/ingest", async (AmazonOperationalIngestionService ingestionService, CancellationToken cancellationToken) =>
{
    var snapshotId = await ingestionService.IngestAsync(cancellationToken);
    return Results.Ok(new { snapshotId });
}).WithName("IngestAmazonOperationalMetrics").WithTags("Amazon").WithOpenApi(operation =>
{
    operation.Summary = "采集 Amazon 运营指标";
    operation.Description = "调用外部数据源采集运营指标并写入运营快照。";
    return operation;
});

app.MapGet("/amazon/operations/summary", async (AmazonOperationalInsightService insightService, CancellationToken cancellationToken) =>
{
    var summary = await insightService.GetSummaryAsync(cancellationToken);
    return Results.Ok(summary);
}).WithName("GetAmazonOperationalSummary").WithTags("Amazon").WithOpenApi(operation =>
{
    operation.Summary = "获取 Amazon 运营概要";
    operation.Description = "返回库存、差评等运营概览统计。";
    return operation;
});

app.MapGet("/amazon/operations/issues", async ([AsParameters] AmazonOperationalIssueRequest request, AmazonOperationalInsightService insightService, CancellationToken cancellationToken) =>
{
    var result = await insightService.GetIssuesAsync(request.ToQuery(), cancellationToken);
    return Results.Ok(result);
}).WithName("GetAmazonOperationalIssues").WithTags("Amazon").WithOpenApi(operation =>
{
    operation.Summary = "分页查询 Amazon 运营问题";
    operation.Description = "按类别、严重度与关键字筛选运营问题列表。";
    return operation;
});

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static ProjectListItemDto MapProjectToDto(ProjectListItem item) => new(
    item.Id,
    item.Name,
    item.NameCn,
    item.Blurb,
    item.BlurbCn,
    item.CategoryName,
    item.Country,
    item.State,
    item.Goal,
    item.Pledged,
    item.PercentFunded,
    item.FundingVelocity,
    item.BackersCount,
    item.Currency,
    item.LaunchedAt,
    item.Deadline,
    item.CreatorName,
    item.LocationName
);

static ProjectHighlightDto MapHighlightToDto(ProjectHighlight item) => new()
{
    Id = item.Id,
    Name = item.Name,
    NameCn = item.NameCn,
    CategoryName = item.CategoryName,
    Country = item.Country,
    PercentFunded = item.PercentFunded,
    Pledged = item.Pledged,
    FundingVelocity = item.FundingVelocity,
    BackersCount = item.BackersCount,
    Currency = item.Currency,
    LaunchedAt = item.LaunchedAt,
};

static ProjectFavoriteDto MapFavoriteToDto(ProjectFavoriteRecord record) => new()
{
    Id = record.Id,
    ClientId = record.ClientId,
    Project = MapProjectToDto(record.Project),
    Note = record.Note,
    SavedAt = record.SavedAt,
};

public partial class Program;
