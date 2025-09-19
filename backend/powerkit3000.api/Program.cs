using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using powerkit3000.api.Contracts;
using powerkit3000.core.contracts;
using powerkit3000.core.services;
using powerkit3000.data;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

app.UseCors("tradeforge");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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

    var response = new ProjectQueryResponseDto
    {
        Total = result.TotalCount,
        Items = result.Items
            .Select(item => new ProjectListItemDto(
                item.Id,
                item.Name,
                item.Blurb,
                item.CategoryName,
                item.Country,
                item.State,
                item.Goal,
                item.Pledged,
                item.PercentFunded,
                item.BackersCount,
                item.Currency,
                item.LaunchedAt,
                item.Deadline,
                item.CreatorName,
                item.LocationName
            ))
            .ToList()
    };

    return Results.Ok(response);
})
.WithName("GetProjects")
.WithOpenApi();

app.MapGet("/filters", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
{
    var states = await dbContext.KickstarterProjects
        .AsNoTracking()
        .Select(p => p.State)
        .Where(s => s != null && s != string.Empty)
        .Distinct()
        .OrderBy(s => s)
        .ToListAsync(cancellationToken);

    var countries = await dbContext.KickstarterProjects
        .AsNoTracking()
        .Select(p => p.Country)
        .Where(c => c != null && c != string.Empty)
        .Distinct()
        .OrderBy(c => c)
        .ToListAsync(cancellationToken);

    var categories = await dbContext.Categories
        .AsNoTracking()
        .Select(c => c.Name)
        .Where(n => n != null && n != string.Empty)
        .Distinct()
        .OrderBy(n => n)
        .ToListAsync(cancellationToken);

    return Results.Ok(new ProjectFiltersDto
    {
        States = states!
            .Select(s => s!)
            .ToList(),
        Countries = countries!
            .Select(c => c!)
            .ToList(),
        Categories = categories!
            .Select(c => c!)
            .ToList(),
    });
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
        CategoryName = item.CategoryName,
        Country = item.Country,
        PercentFunded = item.PercentFunded,
        Pledged = item.Pledged,
        BackersCount = item.BackersCount,
        Currency = item.Currency,
        LaunchedAt = item.LaunchedAt,
    }));
}).WithName("GetTopProjects").WithOpenApi();

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
