using System;
using System.Collections.Generic;

namespace pk.core.Template;

/// <summary>
/// 系统基础配置。
/// </summary>
public sealed record TemplateSettings(
    string ProjectName,
    string? LogoUrl,
    string? ContactEmail,
    string? ApiBaseUrl,
    string? ThemeColor);

/// <summary>
/// 个人账户配置。
/// </summary>
public sealed record TemplateProfile(
    string DisplayName,
    string Email,
    string? AccessToken);

/// <summary>
/// 通用字典条目。
/// </summary>
public sealed record TemplateDictionaryItem(
    Guid Id,
    string Category,
    string Key,
    string Value,
    string? Notes);
