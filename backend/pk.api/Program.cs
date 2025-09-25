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
    builder.Services.AddHttpClient<IAmazonBestsellerSource, HtmlAgilityPackAmazonBestsellerSource>();
    builder.Services.AddScoped<AmazonIngestionService>();
    builder.Services.AddScoped<AmazonTrendAnalysisService>();
    builder.Services.AddScoped<AmazonReportingService>();
    builder.Services.AddScoped<AmazonDashboardService>();
    builder.Services.AddScoped<AmazonRecurringJobService>();

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

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.MapHealthChecks("/health").WithName("Health");
    app.MapGet("/monitoring/metrics", (MetricsSnapshotService metrics) => Results.Ok(metrics.CreateSnapshot()))
        .WithName("GetMetrics")
        .WithOpenApi();

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
.WithOpenApi();

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
}).WithName("GetFilters").WithOpenApi();

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
}).WithName("GetProjectSummary").WithOpenApi();

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
}).WithName("GetCategoryInsights").WithOpenApi();

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
}).WithName("GetCountryInsights").WithOpenApi();

app.MapGet("/analytics/top-projects", async (
    int? limit,
    [AsParameters] AnalyticsFilterRequest request,
    KickstarterProjectQueryService queryService,
    CancellationToken cancellationToken) =>
{
    var result = await queryService.GetTopProjectsAsync(limit ?? 10, MapFilters(request), cancellationToken);

    return Results.Ok(result.Select(MapHighlightToDto));
}).WithName("GetTopProjects").WithOpenApi();

app.MapGet("/analytics/hype", async (
    int? limit,
    decimal? minPercentFunded,
    [AsParameters] AnalyticsFilterRequest request,
    KickstarterProjectQueryService queryService,
    CancellationToken cancellationToken) =>
{
    var result = await queryService.GetHypeProjectsAsync(limit ?? 10, minPercentFunded, MapFilters(request), cancellationToken);

    return Results.Ok(result.Select(MapHighlightToDto));
}).WithName("GetHypeProjects").WithOpenApi();

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
}).WithName("GetCategoryKeywords").WithOpenApi();

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
}).WithName("GetFavorites").WithOpenApi();

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
}).WithName("SaveFavorite").WithOpenApi();

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
}).WithName("DeleteFavorite").WithOpenApi();

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
}).WithName("ClearFavorites").WithOpenApi();

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
}).WithName("GetMonthlyTrend").WithOpenApi();

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
}).WithName("GetFundingDistribution").WithOpenApi();

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
}).WithName("GetCreatorPerformance").WithOpenApi();

// -------- Amazon 模块 API：覆盖类目同步、快照采集、趋势与报告查询 --------

app.MapPost("/amazon/categories/ensure", async (AmazonIngestionService ingestionService, CancellationToken cancellationToken) =>
{
    var affected = await ingestionService.EnsureCategoriesAsync(cancellationToken);
    return Results.Ok(new { affected });
}).WithName("EnsureAmazonCategories").WithOpenApi();

app.MapPost("/amazon/snapshots", async (AmazonFetchSnapshotRequest request, AmazonIngestionService ingestionService, CancellationToken cancellationToken) =>
{
    var snapshotId = await ingestionService.CaptureSnapshotAsync(request.CategoryId, request.BestsellerType, cancellationToken);
    return Results.Ok(new { snapshotId });
}).WithName("CreateAmazonSnapshot").WithOpenApi();

app.MapPost("/amazon/snapshots/{snapshotId:long}/analyze", async (long snapshotId, AmazonTrendAnalysisService analysisService, CancellationToken cancellationToken) =>
{
    var trendCount = await analysisService.AnalyzeSnapshotAsync(snapshotId, cancellationToken);
    return Results.Ok(new { snapshotId, trendCount });
}).WithName("AnalyzeAmazonSnapshot").WithOpenApi();

app.MapGet("/amazon/core-metrics", async (AmazonDashboardService dashboardService, CancellationToken cancellationToken) =>
{
    var metrics = await dashboardService.GetLatestCoreMetricsAsync(cancellationToken);
    return metrics is null
        ? Results.NoContent()
        : Results.Ok(new AmazonCoreMetricsResponseDto(metrics.SnapshotId, metrics.CapturedAt, metrics.TotalProducts, metrics.TotalNewEntries, metrics.TotalRankSurges, metrics.TotalConsistentPerformers));
}).WithName("GetAmazonCoreMetrics").WithOpenApi();

app.MapGet("/amazon/products", async ([AsParameters] AmazonProductsQueryRequest request, AmazonDashboardService dashboardService, CancellationToken cancellationToken) =>
{
    var products = await dashboardService.GetProductsAsync(request.CategoryId, request.Search, cancellationToken);
    var response = products.Select(p => new AmazonProductListItemDto(p.Asin, p.Title, p.CategoryName, p.ListingDate, p.LatestRank, p.LatestPrice, p.LatestRating, p.LatestReviewsCount, p.LastUpdated)).ToList();
    return Results.Ok(response);
}).WithName("GetAmazonProducts").WithOpenApi();

app.MapGet("/amazon/trends", async ([AsParameters] AmazonTrendsQueryRequest request, AmazonDashboardService dashboardService, CancellationToken cancellationToken) =>
{
    var trends = await dashboardService.GetLatestTrendsAsync(request.TrendType, cancellationToken);
    var response = trends.Select(t => new AmazonTrendListItemDto(t.Asin, t.Title, t.TrendType, t.Description, t.RecordedAt)).ToList();
    return Results.Ok(response);
}).WithName("GetAmazonTrends").WithOpenApi();

app.MapGet("/amazon/products/{asin}/history", async (string asin, AmazonDashboardService dashboardService, CancellationToken cancellationToken) =>
{
    var history = await dashboardService.GetProductHistoryAsync(asin, cancellationToken);
    var response = history.Select(h => new AmazonProductHistoryPointDto(h.Timestamp, h.Rank, h.Price, h.Rating, h.ReviewsCount)).ToList();
    return response.Count == 0 ? Results.NotFound() : Results.Ok(response);
}).WithName("GetAmazonProductHistory").WithOpenApi();

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
}).WithName("GetLatestAmazonReport").WithOpenApi();

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
