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
    ProjectQueryRequest request,
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

app.MapGet("/projects/summary", async (KickstarterProjectQueryService queryService, CancellationToken cancellationToken) =>
{
    var summary = await queryService.GetSummaryAsync(cancellationToken);

    return Results.Ok(new ProjectSummaryDto
    {
        TotalProjects = summary.TotalProjects,
        SuccessfulProjects = summary.SuccessfulProjects,
        TotalPledged = summary.TotalPledged,
        DistinctCountries = summary.DistinctCountries,
    });
}).WithName("GetProjectSummary").WithOpenApi();

app.Run();
