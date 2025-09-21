using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using powerkit3000.core.contracts;
using powerkit3000.data;
using powerkit3000.data.Models;
using System.Linq.Expressions;

namespace powerkit3000.core.services;

public class KickstarterProjectQueryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<KickstarterProjectQueryService> _logger;

    public KickstarterProjectQueryService(AppDbContext context, ILogger<KickstarterProjectQueryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProjectQueryResult> QueryAsync(ProjectQueryOptions options, CancellationToken cancellationToken = default)
    {
        var query = _context.KickstarterProjects
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Creator)
            .Include(p => p.Location)
            .AsQueryable();

        if (options.States is { Count: > 0 })
        {
            query = query.Where(p => options.States.Contains(p.State!));
        }

        if (options.Countries is { Count: > 0 })
        {
            query = query.Where(p => options.Countries.Contains(p.Country!));
        }

        if (options.Categories is { Count: > 0 })
        {
            query = query.Where(p => options.Categories.Contains(p.Category!.Name!));
        }

        if (options.MinGoal.HasValue)
        {
            query = query.Where(p => p.Goal >= options.MinGoal.Value);
        }

        if (options.MaxGoal.HasValue)
        {
            query = query.Where(p => p.Goal <= options.MaxGoal.Value);
        }

        if (options.MinPercentFunded.HasValue)
        {
            query = query.Where(p => p.PercentFunded >= options.MinPercentFunded.Value);
        }

        if (options.LaunchedAfter.HasValue)
        {
            query = query.Where(p => p.LaunchedAt >= options.LaunchedAfter.Value);
        }

        if (options.LaunchedBefore.HasValue)
        {
            query = query.Where(p => p.LaunchedAt <= options.LaunchedBefore.Value);
        }

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            var lowered = options.Search.ToLowerInvariant();
            query = query.Where(p =>
                (p.Name != null && EF.Functions.ILike(p.Name, $"%{lowered}%")) ||
                (p.Blurb != null && EF.Functions.ILike(p.Blurb, $"%{lowered}%")) ||
                (p.Creator != null && p.Creator.Name != null && EF.Functions.ILike(p.Creator.Name, $"%{lowered}%")) ||
                (p.Category != null && p.Category.Name != null && EF.Functions.ILike(p.Category.Name, $"%{lowered}%"))
            );
        }

        var filteredQuery = query;

        var aggregate = await filteredQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Successful = g.Count(p => p.State == "successful"),
                TotalPledged = g.Sum(p => p.Pledged),
                TotalBackers = g.Sum(p => p.BackersCount),
                AveragePercentFunded = g.Average(p => (decimal?)p.PercentFunded),
                AverageGoal = g.Average(p => (decimal?)p.Goal)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalCount = aggregate?.Total ?? 0;

        Expression<Func<KickstarterProject, ProjectListItem>> projectSelector = p => new ProjectListItem(
            p.Id,
            p.Name ?? string.Empty,
            p.Blurb,
            p.Category != null ? p.Category.Name ?? string.Empty : string.Empty,
            p.Country ?? string.Empty,
            p.State ?? string.Empty,
            p.Goal,
            p.Pledged,
            p.PercentFunded,
            p.BackersCount,
            p.Currency ?? string.Empty,
            p.LaunchedAt,
            p.Deadline,
            p.Creator != null ? p.Creator.Name ?? string.Empty : string.Empty,
            p.Location != null ? p.Location.DisplayableName ?? p.Location.Name : null
        );

        var items = await filteredQuery
            .OrderByDescending(p => p.LaunchedAt)
            .Skip((options.Page - 1) * options.PageSize)
            .Take(options.PageSize)
            .Select(projectSelector)
            .ToListAsync(cancellationToken);

        ProjectListItem? topProject = null;
        if (totalCount > 0)
        {
            topProject = await filteredQuery
                .OrderByDescending(p => p.PercentFunded)
                .ThenByDescending(p => p.Pledged)
                .Select(projectSelector)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var stats = new ProjectQueryStats
        {
            SuccessfulCount = aggregate?.Successful ?? 0,
            TotalPledged = aggregate?.TotalPledged is decimal pledged
                ? decimal.Round(pledged, 2, MidpointRounding.AwayFromZero)
                : 0m,
            AveragePercentFunded = aggregate?.AveragePercentFunded is decimal avgFunded
                ? decimal.Round(avgFunded, 1, MidpointRounding.AwayFromZero)
                : 0m,
            TotalBackers = aggregate?.TotalBackers ?? 0,
            AverageGoal = aggregate?.AverageGoal is decimal avgGoal
                ? decimal.Round(avgGoal, 2, MidpointRounding.AwayFromZero)
                : 0m,
            TopProject = topProject
        };

        _logger.LogInformation("查询 Kickstarter 项目，共匹配 {Total} 条记录。", totalCount);

        return new ProjectQueryResult
        {
            TotalCount = totalCount,
            Items = items,
            Stats = stats,
        };
    }

    public async Task<ProjectSummary> GetSummaryAsync(AnalyticsFilterOptions? filters = null, CancellationToken cancellationToken = default)
    {
        var query = _context.KickstarterProjects.AsNoTracking().AsQueryable();

        if (filters is { Countries: { Count: > 0 } countries })
        {
            query = query.Where(p => p.Country != null && countries.Contains(p.Country));
        }

        if (filters is { Categories: { Count: > 0 } categories })
        {
            query = query.Where(p => p.Category != null && categories.Contains(p.Category.Name ?? string.Empty));
        }

        if (filters?.LaunchedAfter is not null)
        {
            query = query.Where(p => p.LaunchedAt >= filters.LaunchedAfter.Value);
        }

        if (filters?.LaunchedBefore is not null)
        {
            query = query.Where(p => p.LaunchedAt <= filters.LaunchedBefore.Value);
        }

        var totalProjects = await query.CountAsync(cancellationToken);
        var successfulProjects = await query.CountAsync(p => p.State == "successful", cancellationToken);
        var totalPledged = await query.Where(p => p.Pledged > 0)
            .SumAsync(p => (decimal?)p.Pledged, cancellationToken) ?? 0m;
        var distinctCountries = await query
            .Select(p => p.Country)
            .Where(c => c != null && c != string.Empty)
            .Distinct()
            .CountAsync(cancellationToken);

        return new ProjectSummary
        {
            TotalProjects = totalProjects,
            SuccessfulProjects = successfulProjects,
            TotalPledged = decimal.Round(totalPledged, 2, MidpointRounding.AwayFromZero),
            DistinctCountries = distinctCountries,
        };
    }

    public async Task<IReadOnlyList<CategoryInsight>> GetCategoryInsightsAsync(int minimumProjects = 5, AnalyticsFilterOptions? filters = null, CancellationToken cancellationToken = default)
    {
        var query = _context.KickstarterProjects
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (filters is { Countries: { Count: > 0 } countries })
        {
            query = query.Where(p => p.Country != null && countries.Contains(p.Country));
        }

        if (filters?.LaunchedAfter is not null)
        {
            query = query.Where(p => p.LaunchedAt >= filters.LaunchedAfter.Value);
        }

        if (filters?.LaunchedBefore is not null)
        {
            query = query.Where(p => p.LaunchedAt <= filters.LaunchedBefore.Value);
        }

        var categories = await query
            .Where(p => p.Category != null && p.Category.Name != null && p.Category.Name != string.Empty)
            .GroupBy(p => p.Category!.Name!)
            .Select(g => new
            {
                CategoryName = g.Key,
                Total = g.Count(),
                Successful = g.Count(p => p.State == "successful"),
                AveragePercent = g.Average(p => p.PercentFunded),
                TotalPledged = g.Sum(p => p.Pledged)
            })
            .ToListAsync(cancellationToken);

        return categories
            .Where(c => c.Total >= minimumProjects)
            .Select(c => new CategoryInsight
            {
                CategoryName = c.CategoryName,
                TotalProjects = c.Total,
                SuccessfulProjects = c.Successful,
                SuccessRate = c.Total == 0 ? 0 : Math.Round((decimal)c.Successful / c.Total * 100, 1),
                AveragePercentFunded = Math.Round(c.AveragePercent, 1),
                TotalPledged = Math.Round(c.TotalPledged, 2)
            })
            .OrderByDescending(c => c.SuccessRate)
            .ThenByDescending(c => c.TotalProjects)
            .ToList();
    }

    public async Task<IReadOnlyList<CountryInsight>> GetCountryInsightsAsync(int minimumProjects = 5, AnalyticsFilterOptions? filters = null, CancellationToken cancellationToken = default)
    {
        var query = _context.KickstarterProjects.AsNoTracking().AsQueryable();

        if (filters is { Categories: { Count: > 0 } categories })
        {
            query = query.Where(p => p.Category != null && categories.Contains(p.Category.Name ?? string.Empty));
        }

        if (filters?.LaunchedAfter is not null)
        {
            query = query.Where(p => p.LaunchedAt >= filters.LaunchedAfter.Value);
        }

        if (filters?.LaunchedBefore is not null)
        {
            query = query.Where(p => p.LaunchedAt <= filters.LaunchedBefore.Value);
        }

        var countries = await query
            .Where(p => p.Country != null && p.Country != string.Empty)
            .GroupBy(p => p.Country!)
            .Select(g => new
            {
                Country = g.Key,
                Total = g.Count(),
                Successful = g.Count(p => p.State == "successful"),
                TotalPledged = g.Sum(p => p.Pledged)
            })
            .ToListAsync(cancellationToken);

        return countries
            .Where(c => c.Total >= minimumProjects)
            .Select(c => new CountryInsight
            {
                Country = c.Country,
                TotalProjects = c.Total,
                SuccessfulProjects = c.Successful,
                SuccessRate = c.Total == 0 ? 0 : Math.Round((decimal)c.Successful / c.Total * 100, 1),
                TotalPledged = Math.Round(c.TotalPledged, 2)
            })
            .OrderByDescending(c => c.SuccessRate)
            .ThenByDescending(c => c.TotalProjects)
            .ToList();
    }

    public async Task<IReadOnlyList<ProjectHighlight>> GetTopProjectsAsync(int limit = 10, AnalyticsFilterOptions? filters = null, CancellationToken cancellationToken = default)
    {
        limit = limit <= 0 ? 10 : limit;

        var query = _context.KickstarterProjects
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.PercentFunded > 0)
            .AsQueryable();

        if (filters is { Countries: { Count: > 0 } countries })
        {
            query = query.Where(p => p.Country != null && countries.Contains(p.Country));
        }

        if (filters is { Categories: { Count: > 0 } categories })
        {
            query = query.Where(p => p.Category != null && categories.Contains(p.Category.Name ?? string.Empty));
        }

        if (filters?.LaunchedAfter is not null)
        {
            query = query.Where(p => p.LaunchedAt >= filters.LaunchedAfter.Value);
        }

        if (filters?.LaunchedBefore is not null)
        {
            query = query.Where(p => p.LaunchedAt <= filters.LaunchedBefore.Value);
        }

        return await query
            .OrderByDescending(p => p.PercentFunded)
            .ThenByDescending(p => p.Pledged)
            .Take(limit)
            .Select(p => new ProjectHighlight
            {
                Id = p.Id,
                Name = p.Name ?? string.Empty,
                CategoryName = p.Category != null ? p.Category.Name ?? string.Empty : string.Empty,
                Country = p.Country ?? string.Empty,
                PercentFunded = p.PercentFunded,
                Pledged = p.Pledged,
                BackersCount = p.BackersCount,
                Currency = p.Currency ?? string.Empty,
                LaunchedAt = p.LaunchedAt,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MonthlyTrendPoint>> GetMonthlyTrendAsync(AnalyticsFilterOptions? filters = null, int months = 12, CancellationToken cancellationToken = default)
    {
        months = months <= 0 ? 12 : months;
        var cutoff = DateTime.UtcNow.AddMonths(-months + 1).Date;
        var query = _context.KickstarterProjects
            .AsNoTracking()
            .Where(p => p.LaunchedAt >= cutoff)
            .AsQueryable();

        if (filters is { Countries: { Count: > 0 } countries })
        {
            query = query.Where(p => p.Country != null && countries.Contains(p.Country));
        }

        if (filters is { Categories: { Count: > 0 } categories })
        {
            query = query.Where(p => p.Category != null && categories.Contains(p.Category.Name ?? string.Empty));
        }

        if (filters?.LaunchedAfter is not null)
        {
            query = query.Where(p => p.LaunchedAt >= filters.LaunchedAfter.Value);
        }

        if (filters?.LaunchedBefore is not null)
        {
            query = query.Where(p => p.LaunchedAt <= filters.LaunchedBefore.Value);
        }

        var trend = await query
            .GroupBy(p => new { p.LaunchedAt.Year, p.LaunchedAt.Month })
            .Select(g => new MonthlyTrendPoint
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalProjects = g.Count(),
                SuccessfulProjects = g.Count(p => p.State == "successful"),
                TotalPledged = Math.Round(g.Sum(p => p.Pledged), 2),
            })
            .OrderBy(p => p.Year)
            .ThenBy(p => p.Month)
            .ToListAsync(cancellationToken);

        return trend;
    }

    public async Task<IReadOnlyList<FundingDistributionBin>> GetFundingDistributionAsync(AnalyticsFilterOptions? filters = null, CancellationToken cancellationToken = default)
    {
        var bins = new List<(string Label, decimal Min, decimal Max)>
        {
            ("<50%", 0m, 50m),
            ("50%-100%", 50m, 100m),
            ("100%-200%", 100m, 200m),
            (">=200%", 200m, decimal.MaxValue),
        };

        var query = _context.KickstarterProjects
            .AsNoTracking()
            .AsQueryable();

        if (filters is { Countries: { Count: > 0 } countries })
        {
            query = query.Where(p => p.Country != null && countries.Contains(p.Country));
        }

        if (filters is { Categories: { Count: > 0 } categories })
        {
            query = query.Where(p => p.Category != null && categories.Contains(p.Category.Name ?? string.Empty));
        }

        if (filters?.LaunchedAfter is not null)
        {
            query = query.Where(p => p.LaunchedAt >= filters.LaunchedAfter.Value);
        }

        if (filters?.LaunchedBefore is not null)
        {
            query = query.Where(p => p.LaunchedAt <= filters.LaunchedBefore.Value);
        }

        var projects = await query
            .Select(p => new { p.PercentFunded, p.State })
            .ToListAsync(cancellationToken);

        return bins
            .Select(bin =>
            {
                var matching = projects.Where(p => p.PercentFunded >= bin.Min && (bin.Max == decimal.MaxValue || p.PercentFunded < bin.Max));
                var total = matching.Count();
                var successful = matching.Count(p => p.State == "successful");

                return new FundingDistributionBin
                {
                    Label = bin.Label,
                    MinPercent = bin.Min,
                    MaxPercent = bin.Max,
                    TotalProjects = total,
                    SuccessfulProjects = successful,
                };
            })
            .ToList();
    }

    public async Task<IReadOnlyList<CreatorPerformance>> GetTopCreatorsAsync(int minimumProjects = 3, int limit = 10, AnalyticsFilterOptions? filters = null, CancellationToken cancellationToken = default)
    {
        minimumProjects = minimumProjects <= 0 ? 3 : minimumProjects;
        limit = limit <= 0 ? 10 : limit;

        var query = _context.KickstarterProjects
            .AsNoTracking()
            .Where(p => p.Creator != null)
            .AsQueryable();

        if (filters is { Countries: { Count: > 0 } countries })
        {
            query = query.Where(p => p.Country != null && countries.Contains(p.Country));
        }

        if (filters is { Categories: { Count: > 0 } categories })
        {
            query = query.Where(p => p.Category != null && categories.Contains(p.Category.Name ?? string.Empty));
        }

        if (filters?.LaunchedAfter is not null)
        {
            query = query.Where(p => p.LaunchedAt >= filters.LaunchedAfter.Value);
        }

        if (filters?.LaunchedBefore is not null)
        {
            query = query.Where(p => p.LaunchedAt <= filters.LaunchedBefore.Value);
        }

        var creators = await query
            .GroupBy(p => new { p.Creator!.Id, p.Creator.Name })
            .Select(g => new
            {
                CreatorId = g.Key.Id,
                CreatorName = g.Key.Name ?? "Unknown",
                TotalProjects = g.Count(),
                SuccessfulProjects = g.Count(p => p.State == "successful"),
                AveragePercentFunded = g.Average(p => p.PercentFunded),
                TotalPledged = g.Sum(p => p.Pledged)
            })
            .ToListAsync(cancellationToken);

        return creators
            .Where(c => c.TotalProjects >= minimumProjects)
            .Select(c => new CreatorPerformance
            {
                CreatorId = c.CreatorId,
                CreatorName = c.CreatorName,
                TotalProjects = c.TotalProjects,
                SuccessfulProjects = c.SuccessfulProjects,
                SuccessRate = c.TotalProjects == 0 ? 0 : Math.Round((decimal)c.SuccessfulProjects / c.TotalProjects * 100, 1),
                AveragePercentFunded = Math.Round(c.AveragePercentFunded, 1),
                TotalPledged = Math.Round(c.TotalPledged, 2),
            })
            .OrderByDescending(c => c.SuccessRate)
            .ThenByDescending(c => c.TotalPledged)
            .Take(limit)
            .ToList();
    }
}
