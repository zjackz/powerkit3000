using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pk.core.contracts;
using pk.data;
using pk.data.Models;

namespace pk.core.services;

public class KickstarterProjectQueryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<KickstarterProjectQueryService> _logger;

    private static readonly char[] KeywordSeparators =
    {
        ' ', '\n', '\r', '\t', ',', '.', ';', ':', '"', '\'', '!', '?', '(', ')', '[', ']', '{', '}', '/', '\\', '-', '_', '#', '&'
    };

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "with", "from", "this", "that", "your", "project", "kickstarter",
        "a", "an", "of", "to", "in", "on", "by", "is", "are", "at", "us"
    };

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

        var projectedItems = await filteredQuery
            .OrderByDescending(p => p.LaunchedAt)
            .Skip((options.Page - 1) * options.PageSize)
            .Take(options.PageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.NameCn,
                p.Blurb,
                p.BlurbCn,
                CategoryName = p.Category != null ? p.Category.Name ?? string.Empty : string.Empty,
                Country = p.Country ?? string.Empty,
                State = p.State ?? string.Empty,
                p.Goal,
                p.Pledged,
                p.PercentFunded,
                p.BackersCount,
                Currency = p.Currency ?? string.Empty,
                p.LaunchedAt,
                p.Deadline,
                CreatorName = p.Creator != null ? p.Creator.Name ?? string.Empty : string.Empty,
                LocationName = p.Location != null ? p.Location.DisplayableName ?? p.Location.Name : null
            })
            .ToListAsync(cancellationToken);

        var items = projectedItems
            .Select(p => new ProjectListItem(
                p.Id,
                p.Name ?? string.Empty,
                p.NameCn,
                p.Blurb,
                p.BlurbCn,
                p.CategoryName,
                p.Country,
                p.State,
                p.Goal,
                p.Pledged,
                p.PercentFunded,
                CalculateFundingVelocity(p.LaunchedAt, p.Pledged),
                p.BackersCount,
                p.Currency,
                p.LaunchedAt,
                p.Deadline,
                p.CreatorName,
                p.LocationName
            ))
            .ToList();

        ProjectListItem? topProject = null;
        if (totalCount > 0)
        {
            var topProjection = await filteredQuery
                .OrderByDescending(p => p.PercentFunded)
                .ThenByDescending(p => p.Pledged)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.NameCn,
                    p.Blurb,
                    p.BlurbCn,
                    CategoryName = p.Category != null ? p.Category.Name ?? string.Empty : string.Empty,
                    Country = p.Country ?? string.Empty,
                    State = p.State ?? string.Empty,
                    p.Goal,
                    p.Pledged,
                    p.PercentFunded,
                    p.BackersCount,
                    Currency = p.Currency ?? string.Empty,
                    p.LaunchedAt,
                    p.Deadline,
                    CreatorName = p.Creator != null ? p.Creator.Name ?? string.Empty : string.Empty,
                    LocationName = p.Location != null ? p.Location.DisplayableName ?? p.Location.Name : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (topProjection is not null)
            {
                topProject = new ProjectListItem(
                    topProjection.Id,
                    topProjection.Name ?? string.Empty,
                    topProjection.NameCn,
                    topProjection.Blurb,
                    topProjection.BlurbCn,
                    topProjection.CategoryName,
                    topProjection.Country,
                    topProjection.State,
                    topProjection.Goal,
                    topProjection.Pledged,
                    topProjection.PercentFunded,
                    CalculateFundingVelocity(topProjection.LaunchedAt, topProjection.Pledged),
                    topProjection.BackersCount,
                    topProjection.Currency,
                    topProjection.LaunchedAt,
                    topProjection.Deadline,
                    topProjection.CreatorName,
                    topProjection.LocationName
                );
            }
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

        if (filters?.MinPercentFunded is not null)
        {
            query = query.Where(p => p.PercentFunded >= filters.MinPercentFunded.Value);
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

        if (filters?.MinPercentFunded is not null)
        {
            query = query.Where(p => p.PercentFunded >= filters.MinPercentFunded.Value);
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

        if (filters?.MinPercentFunded is not null)
        {
            query = query.Where(p => p.PercentFunded >= filters.MinPercentFunded.Value);
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

        if (filters?.MinPercentFunded is not null)
        {
            query = query.Where(p => p.PercentFunded >= filters.MinPercentFunded.Value);
        }

        var projections = await query
            .OrderByDescending(p => p.PercentFunded)
            .ThenByDescending(p => p.Pledged)
            .Take(limit)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.NameCn,
                CategoryName = p.Category != null ? p.Category.Name ?? string.Empty : string.Empty,
                Country = p.Country ?? string.Empty,
                p.PercentFunded,
                p.Pledged,
                p.BackersCount,
                Currency = p.Currency ?? string.Empty,
                p.LaunchedAt,
            })
            .ToListAsync(cancellationToken);

        return projections
            .Select(p => new ProjectHighlight
            {
                Id = p.Id,
                Name = p.Name ?? string.Empty,
                NameCn = p.NameCn,
                CategoryName = p.CategoryName,
                Country = p.Country,
                PercentFunded = p.PercentFunded,
                Pledged = p.Pledged,
                FundingVelocity = CalculateFundingVelocity(p.LaunchedAt, p.Pledged),
                BackersCount = p.BackersCount,
                Currency = p.Currency,
                LaunchedAt = p.LaunchedAt,
            })
            .ToList();
    }

    public async Task<IReadOnlyList<ProjectHighlight>> GetHypeProjectsAsync(
        int limit = 10,
        decimal? minPercentFunded = null,
        AnalyticsFilterOptions? filters = null,
        CancellationToken cancellationToken = default)
    {
        limit = limit <= 0 ? 10 : limit;
        var effectiveMinPercent = minPercentFunded ?? filters?.MinPercentFunded ?? 200m;

        var query = _context.KickstarterProjects
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.Pledged > 0)
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

        if (filters?.MinPercentFunded is not null)
        {
            query = query.Where(p => p.PercentFunded >= filters.MinPercentFunded.Value);
        }

        if (effectiveMinPercent > 0)
        {
            query = query.Where(p => p.PercentFunded >= effectiveMinPercent);
        }

        query = query.Where(p => p.LaunchedAt <= DateTime.UtcNow);

        var sampleSize = Math.Max(limit * 6, 120);

        var projections = await query
            .OrderByDescending(p => p.LaunchedAt)
            .Take(sampleSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.NameCn,
                CategoryName = p.Category != null ? p.Category.Name ?? string.Empty : string.Empty,
                Country = p.Country ?? string.Empty,
                p.PercentFunded,
                p.Pledged,
                p.BackersCount,
                Currency = p.Currency ?? string.Empty,
                p.LaunchedAt,
            })
            .ToListAsync(cancellationToken);

        return projections
            .Select(p => new ProjectHighlight
            {
                Id = p.Id,
                Name = p.Name ?? string.Empty,
                NameCn = p.NameCn,
                CategoryName = p.CategoryName,
                Country = p.Country,
                PercentFunded = p.PercentFunded,
                Pledged = p.Pledged,
                FundingVelocity = CalculateFundingVelocity(p.LaunchedAt, p.Pledged),
                BackersCount = p.BackersCount,
                Currency = p.Currency,
                LaunchedAt = p.LaunchedAt,
            })
            .OrderByDescending(p => p.FundingVelocity)
            .ThenByDescending(p => p.PercentFunded)
            .ThenByDescending(p => p.Pledged)
            .Take(limit)
            .ToList();
    }

    public async Task<IReadOnlyList<CategoryKeywordInsight>> GetCategoryKeywordsAsync(
        string categoryName,
        int top = 30,
        AnalyticsFilterOptions? filters = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return Array.Empty<CategoryKeywordInsight>();
        }

        top = top <= 0 ? 30 : top;

        var query = _context.KickstarterProjects
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.State == "successful")
            .Where(p => p.Category != null && p.Category.Name != null && p.Category.Name == categoryName)
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

        if (filters?.MinPercentFunded is not null)
        {
            query = query.Where(p => p.PercentFunded >= filters.MinPercentFunded.Value);
        }

        var projects = await query
            .Select(p => new
            {
                p.Name,
                p.NameCn,
                p.Blurb,
                p.BlurbCn,
                p.PercentFunded
            })
            .ToListAsync(cancellationToken);

        if (projects.Count == 0)
        {
            return Array.Empty<CategoryKeywordInsight>();
        }

        var aggregates = new Dictionary<string, KeywordAggregate>(StringComparer.OrdinalIgnoreCase);

        foreach (var project in projects)
        {
            var tokens = ExtractKeywords(project.Name)
                .Concat(ExtractKeywords(project.NameCn))
                .Concat(ExtractKeywords(project.Blurb))
                .Concat(ExtractKeywords(project.BlurbCn))
                .ToList();

            if (tokens.Count == 0)
            {
                continue;
            }

            var uniqueTokens = tokens.Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var token in tokens)
            {
                if (!aggregates.TryGetValue(token, out var aggregate))
                {
                    aggregate = new KeywordAggregate();
                    aggregates[token] = aggregate;
                }

                aggregate.OccurrenceCount++;
            }

            foreach (var token in uniqueTokens)
            {
                var aggregate = aggregates[token];
                aggregate.ProjectCount++;
                aggregate.TotalPercentFunded += project.PercentFunded;
            }
        }

        return aggregates
            .Where(kvp => kvp.Value.ProjectCount > 1)
            .Select(kvp => new CategoryKeywordInsight
            {
                Keyword = kvp.Key,
                ProjectCount = kvp.Value.ProjectCount,
                OccurrenceCount = kvp.Value.OccurrenceCount,
                AveragePercentFunded = kvp.Value.ProjectCount == 0
                    ? 0
                    : Math.Round(kvp.Value.TotalPercentFunded / kvp.Value.ProjectCount, 1, MidpointRounding.AwayFromZero)
            })
            .OrderByDescending(k => k.ProjectCount)
            .ThenByDescending(k => k.OccurrenceCount)
            .Take(top)
            .ToList();
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

        if (filters?.MinPercentFunded is not null)
        {
            query = query.Where(p => p.PercentFunded >= filters.MinPercentFunded.Value);
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

        if (filters?.MinPercentFunded is not null)
        {
            query = query.Where(p => p.PercentFunded >= filters.MinPercentFunded.Value);
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

        if (filters?.MinPercentFunded is not null)
        {
            query = query.Where(p => p.PercentFunded >= filters.MinPercentFunded.Value);
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

    private static decimal CalculateFundingVelocity(DateTime launchedAt, decimal pledged)
    {
        var elapsedDays = (DateTime.UtcNow - launchedAt).TotalDays;
        if (elapsedDays <= 0)
        {
            elapsedDays = 1;
        }

        var velocity = pledged / (decimal)elapsedDays;
        return decimal.Round(velocity, 2, MidpointRounding.AwayFromZero);
    }

    private static IEnumerable<string> ExtractKeywords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        foreach (var raw in text.Split(KeywordSeparators, StringSplitOptions.RemoveEmptyEntries))
        {
            var cleaned = raw.Trim();
            if (cleaned.Length < 2)
            {
                continue;
            }

            var normalized = cleaned.ToLowerInvariant();
            if (StopWords.Contains(normalized))
            {
                continue;
            }

            if (normalized.All(static ch => char.IsLetterOrDigit(ch)))
            {
                yield return normalized;
            }
        }
    }

    private sealed class KeywordAggregate
    {
        public int ProjectCount { get; set; }
        public int OccurrenceCount { get; set; }
        public decimal TotalPercentFunded { get; set; }
    }
}
