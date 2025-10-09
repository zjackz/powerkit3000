using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using pk.data;
using pk.data.Models;

namespace pk.core.Template;

/// <summary>
/// 负责读取和写入模板项目的基础配置（数据库持久化）。
/// </summary>
public sealed class TemplateConfigService
{
    private readonly AppDbContext _dbContext;

    public TemplateConfigService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 获取系统设置。
    /// </summary>
    public async Task<TemplateSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var config = await GetOrCreateConfigurationAsync(cancellationToken).ConfigureAwait(false);
        return MapToSettings(config);
    }

    /// <summary>
    /// 更新系统设置。
    /// </summary>
    public async Task<TemplateSettings> UpdateSettingsAsync(TemplateSettings settings, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.ProjectName))
        {
            throw new ArgumentException("Project name is required.", nameof(settings));
        }

        var config = await GetOrCreateConfigurationAsync(cancellationToken).ConfigureAwait(false);

        config.ProjectName = settings.ProjectName.Trim();
        config.LogoUrl = string.IsNullOrWhiteSpace(settings.LogoUrl) ? null : settings.LogoUrl.Trim();
        config.ContactEmail = string.IsNullOrWhiteSpace(settings.ContactEmail) ? null : settings.ContactEmail.Trim();
        config.ApiBaseUrl = string.IsNullOrWhiteSpace(settings.ApiBaseUrl) ? null : settings.ApiBaseUrl.Trim();
        config.ThemeColor = string.IsNullOrWhiteSpace(settings.ThemeColor) ? null : settings.ThemeColor.Trim();
        config.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return MapToSettings(config);
    }

    /// <summary>
    /// 获取个人账户配置。
    /// </summary>
    public async Task<TemplateProfile> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var config = await GetOrCreateConfigurationAsync(cancellationToken).ConfigureAwait(false);
        return MapToProfile(config);
    }

    /// <summary>
    /// 更新个人账户配置。
    /// </summary>
    public async Task<TemplateProfile> UpdateProfileAsync(TemplateProfile profile, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(profile.DisplayName))
        {
            throw new ArgumentException("Display name is required.", nameof(profile));
        }

        if (string.IsNullOrWhiteSpace(profile.Email))
        {
            throw new ArgumentException("Email is required.", nameof(profile));
        }

        var config = await GetOrCreateConfigurationAsync(cancellationToken).ConfigureAwait(false);

        config.DisplayName = profile.DisplayName.Trim();
        config.Email = profile.Email.Trim();
        config.AccessToken = string.IsNullOrWhiteSpace(profile.AccessToken) ? null : profile.AccessToken.Trim();
        config.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return MapToProfile(config);
    }

    /// <summary>
    /// 获取字典条目列表。
    /// </summary>
    public async Task<IReadOnlyList<TemplateDictionaryItem>> GetDictionaryAsync(string? category, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TemplateDictionaryEntries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalized = category.Trim();
            query = query.Where(entry => entry.Category == normalized);
        }

        var entries = await query
            .OrderBy(entry => entry.Category)
            .ThenBy(entry => entry.Key)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return entries
            .Select(MapToDictionaryItem)
            .ToList();
    }

    /// <summary>
    /// 新增或更新字典条目。
    /// </summary>
    public async Task<TemplateDictionaryItem> UpsertDictionaryAsync(TemplateDictionaryItem item, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(item.Category))
        {
            throw new ArgumentException("Category is required.", nameof(item));
        }

        if (string.IsNullOrWhiteSpace(item.Key))
        {
            throw new ArgumentException("Key is required.", nameof(item));
        }

        var normalizedCategory = item.Category.Trim();
        var normalizedKey = item.Key.Trim();
        var normalizedValue = (item.Value ?? string.Empty).Trim();
        var normalizedNotes = string.IsNullOrWhiteSpace(item.Notes) ? null : item.Notes.Trim();
        var now = DateTime.UtcNow;

        var duplicate = await _dbContext.TemplateDictionaryEntries
            .FirstOrDefaultAsync(
                e => e.Category == normalizedCategory
                     && e.Key == normalizedKey
                     && (item.Id == Guid.Empty || e.Id != item.Id),
                cancellationToken)
            .ConfigureAwait(false);

        if (duplicate != null)
        {
            throw new InvalidOperationException($"Dictionary item '{normalizedCategory}:{normalizedKey}' already exists.");
        }

        TemplateDictionaryEntry entry;
        if (item.Id == Guid.Empty)
        {
            entry = new TemplateDictionaryEntry
            {
                Category = normalizedCategory,
                Key = normalizedKey,
                Value = normalizedValue,
                Notes = normalizedNotes,
                CreatedAt = now,
                UpdatedAt = now,
            };
            _dbContext.TemplateDictionaryEntries.Add(entry);
        }
        else
        {
            entry = await _dbContext.TemplateDictionaryEntries
                .FirstOrDefaultAsync(e => e.Id == item.Id, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Dictionary item '{item.Id}' does not exist.");

            entry.Category = normalizedCategory;
            entry.Key = normalizedKey;
            entry.Value = normalizedValue;
            entry.Notes = normalizedNotes;
            entry.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return MapToDictionaryItem(entry);
    }

    /// <summary>
    /// 删除字典条目。
    /// </summary>
    public async Task<bool> DeleteDictionaryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _dbContext.TemplateDictionaryEntries
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (entry == null)
        {
            return false;
        }

        _dbContext.TemplateDictionaryEntries.Remove(entry);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private async Task<TemplateConfiguration> GetOrCreateConfigurationAsync(CancellationToken cancellationToken)
    {
        var config = await _dbContext.TemplateConfigurations.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (config != null)
        {
            return config;
        }

        config = TemplateConfiguration.CreateDefault();
        _dbContext.TemplateConfigurations.Add(config);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return config;
    }

    private static TemplateSettings MapToSettings(TemplateConfiguration configuration) =>
        new(
            configuration.ProjectName,
            configuration.LogoUrl,
            configuration.ContactEmail,
            configuration.ApiBaseUrl,
            configuration.ThemeColor);

    private static TemplateProfile MapToProfile(TemplateConfiguration configuration) =>
        new(
            configuration.DisplayName,
            configuration.Email,
            configuration.AccessToken);

    private static TemplateDictionaryItem MapToDictionaryItem(TemplateDictionaryEntry entry) =>
        new(
            entry.Id,
            entry.Category,
            entry.Key,
            entry.Value,
            entry.Notes);
}
