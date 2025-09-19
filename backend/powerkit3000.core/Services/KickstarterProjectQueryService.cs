using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using powerkit3000.core.contracts;
using powerkit3000.data;

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

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.LaunchedAt)
            .Skip((options.Page - 1) * options.PageSize)
            .Take(options.PageSize)
            .Select(p => new ProjectListItem(
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
            ))
            .ToListAsync(cancellationToken);

        _logger.LogInformation("查询 Kickstarter 项目，共匹配 {Total} 条记录。", totalCount);

        return new ProjectQueryResult
        {
            TotalCount = totalCount,
            Items = items,
        };
    }

    public async Task<ProjectSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var totalProjects = await _context.KickstarterProjects.CountAsync(cancellationToken);
        var successfulProjects = await _context.KickstarterProjects
            .CountAsync(p => p.State == "successful", cancellationToken);
        var totalPledged = await _context.KickstarterProjects
            .Where(p => p.Pledged > 0)
            .SumAsync(p => (decimal?)p.Pledged, cancellationToken) ?? 0m;
        var distinctCountries = await _context.KickstarterProjects
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
}
