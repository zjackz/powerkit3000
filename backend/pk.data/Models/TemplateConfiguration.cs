using System;

namespace pk.data.Models;

/// <summary>
/// 存储模板项目的基础配置（系统设置 + 个人资料）。
/// </summary>
public class TemplateConfiguration
{
    public int Id { get; set; } = 1;

    public string ProjectName { get; set; } = "PowerKit Template";

    public string? LogoUrl { get; set; }

    public string? ContactEmail { get; set; }

    public string? ApiBaseUrl { get; set; }

    public string? ThemeColor { get; set; } = "#177ddc";

    public string DisplayName { get; set; } = "Owner";

    public string Email { get; set; } = "you@example.com";

    public string? AccessToken { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static TemplateConfiguration CreateDefault() => new();
}
