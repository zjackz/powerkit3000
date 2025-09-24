using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using powerkit3000.api.Contracts;
using powerkit3000.core.contracts;
using powerkit3000.core.services;
using powerkit3000.data;
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
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var connectionString = builder.Configuration.GetConnectionString("AppDb")
        ?? throw new InvalidOperationException("Connection string 'AppDb' was not found.");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));

    builder.Services.AddScoped<KickstarterProjectQueryService>();

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

    var app = builder.Build();

    app.UseCors("tradeforge");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.MapHealthChecks("/health").WithName("Health");

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
        .Select(item => new ProjectListItemDto(
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
        ))
        .ToList();

    var topProjectDto = result.Stats.TopProject is not null
        ? new ProjectListItemDto(
            result.Stats.TopProject.Id,
            result.Stats.TopProject.Name,
            result.Stats.TopProject.NameCn,
            result.Stats.TopProject.Blurb,
            result.Stats.TopProject.BlurbCn,
            result.Stats.TopProject.CategoryName,
            result.Stats.TopProject.Country,
            result.Stats.TopProject.State,
            result.Stats.TopProject.Goal,
            result.Stats.TopProject.Pledged,
            result.Stats.TopProject.PercentFunded,
            result.Stats.TopProject.FundingVelocity,
            result.Stats.TopProject.BackersCount,
            result.Stats.TopProject.Currency,
            result.Stats.TopProject.LaunchedAt,
            result.Stats.TopProject.Deadline,
            result.Stats.TopProject.CreatorName,
            result.Stats.TopProject.LocationName
        )
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

    return Results.Ok(result.Select(item => new ProjectHighlightDto
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
    }));
}).WithName("GetTopProjects").WithOpenApi();

app.MapGet("/analytics/hype", async (
    int? limit,
    decimal? minPercentFunded,
    [AsParameters] AnalyticsFilterRequest request,
    KickstarterProjectQueryService queryService,
    CancellationToken cancellationToken) =>
{
    var result = await queryService.GetHypeProjectsAsync(limit ?? 10, minPercentFunded, MapFilters(request), cancellationToken);

    return Results.Ok(result.Select(item => new ProjectHighlightDto
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
    }));
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

public partial class Program;
