using System;
using System.ComponentModel.DataAnnotations;
using pk.core.Template;

namespace pk.api.Contracts;

/// <summary>
/// 系统设置请求体。
/// </summary>
public sealed class UpdateTemplateSettingsRequest
{
    [Required]
    public string ProjectName { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; }

    public string? ApiBaseUrl { get; set; }

    public string? ThemeColor { get; set; }

    public TemplateSettings ToSettings() =>
        new(ProjectName.Trim(), LogoUrl?.Trim(), ContactEmail?.Trim(), ApiBaseUrl?.Trim(), ThemeColor?.Trim());
}

/// <summary>
/// 个人资料请求体。
/// </summary>
public sealed class UpdateTemplateProfileRequest
{
    [Required]
    public string DisplayName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? AccessToken { get; set; }

    public TemplateProfile ToProfile() =>
        new(DisplayName.Trim(), Email.Trim(), string.IsNullOrWhiteSpace(AccessToken) ? null : AccessToken.Trim());
}

/// <summary>
/// 字典条目请求体。
/// </summary>
public sealed class TemplateDictionaryItemRequest
{
    public Guid? Id { get; set; }

    [Required]
    public string Category { get; set; } = string.Empty;

    [Required]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public TemplateDictionaryItem ToItem() =>
        new(Id ?? Guid.Empty, Category.Trim(), Key.Trim(), Value.Trim(), string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim());
}
