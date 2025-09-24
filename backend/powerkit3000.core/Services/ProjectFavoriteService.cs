using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using powerkit3000.core.contracts;
using powerkit3000.data;
using powerkit3000.data.Models;

namespace powerkit3000.core.services;

public class ProjectFavoriteService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProjectFavoriteService> _logger;

    public ProjectFavoriteService(AppDbContext context, ILogger<ProjectFavoriteService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProjectFavoriteRecord>> GetFavoritesAsync(string clientId, CancellationToken cancellationToken)
    {
        var normalizedClientId = NormalizeClientId(clientId);

        var favorites = await _context.ProjectFavorites
            .AsNoTracking()
            .Where(f => f.ClientId == normalizedClientId)
            .Include(f => f.Project)
                .ThenInclude(p => p.Category)
            .Include(f => f.Project)
                .ThenInclude(p => p.Creator)
            .Include(f => f.Project)
                .ThenInclude(p => p.Location)
            .OrderByDescending(f => f.SavedAt)
            .ToListAsync(cancellationToken);

        return favorites
            .Select(MapToRecord)
            .ToList();
    }

    public async Task<ProjectFavoriteRecord?> UpsertAsync(string clientId, long projectId, string? note, CancellationToken cancellationToken)
    {
        var normalizedClientId = NormalizeClientId(clientId);
        var trimmedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

        var project = await _context.KickstarterProjects
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Creator)
            .Include(p => p.Location)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
        {
            _logger.LogWarning("尝试收藏不存在的项目 {ProjectId}", projectId);
            return null;
        }

        var favorite = await _context.ProjectFavorites
            .FirstOrDefaultAsync(f => f.ClientId == normalizedClientId && f.ProjectId == projectId, cancellationToken);

        if (favorite is null)
        {
            favorite = new ProjectFavorite
            {
                ClientId = normalizedClientId,
                ProjectId = projectId,
                Note = trimmedNote,
                SavedAt = DateTime.UtcNow,
            };

            _context.ProjectFavorites.Add(favorite);
        }
        else
        {
            favorite.Note = trimmedNote;
            favorite.SavedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        favorite.Project = project;
        return MapToRecord(favorite);
    }

    public async Task<bool> RemoveAsync(string clientId, long projectId, CancellationToken cancellationToken)
    {
        var normalizedClientId = NormalizeClientId(clientId);

        var favorite = await _context.ProjectFavorites
            .FirstOrDefaultAsync(f => f.ClientId == normalizedClientId && f.ProjectId == projectId, cancellationToken);

        if (favorite is null)
        {
            return false;
        }

        _context.ProjectFavorites.Remove(favorite);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> ClearAsync(string clientId, CancellationToken cancellationToken)
    {
        var normalizedClientId = NormalizeClientId(clientId);

        var favorites = await _context.ProjectFavorites
            .Where(f => f.ClientId == normalizedClientId)
            .ToListAsync(cancellationToken);

        if (favorites.Count == 0)
        {
            return 0;
        }

        _context.ProjectFavorites.RemoveRange(favorites);
        await _context.SaveChangesAsync(cancellationToken);
        return favorites.Count;
    }

    private static string NormalizeClientId(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentException("ClientId is required", nameof(clientId));
        }

        return clientId.Trim();
    }

    private static ProjectFavoriteRecord MapToRecord(ProjectFavorite favorite)
    {
        var project = favorite.Project ?? throw new InvalidOperationException("Favorite missing project navigation");

        return new ProjectFavoriteRecord(
            favorite.Id,
            favorite.ClientId,
            MapProject(project),
            favorite.Note,
            favorite.SavedAt);
    }

    private static ProjectListItem MapProject(KickstarterProject project)
    {
        var fundingVelocity = CalculateFundingVelocity(project.LaunchedAt, project.Pledged);

        return new ProjectListItem(
            project.Id,
            project.Name ?? string.Empty,
            project.NameCn,
            project.Blurb,
            project.BlurbCn,
            project.Category?.Name ?? string.Empty,
            project.Country ?? string.Empty,
            project.State ?? string.Empty,
            project.Goal,
            project.Pledged,
            project.PercentFunded,
            fundingVelocity,
            project.BackersCount,
            project.Currency ?? string.Empty,
            project.LaunchedAt,
            project.Deadline,
            project.Creator?.Name ?? string.Empty,
            project.Location?.DisplayableName ?? project.Location?.Name
        );
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
}
