using System.ComponentModel.DataAnnotations;

namespace pk.core.translations;

public class TranslationOptions
{
    public const string SectionName = "Translation";

    [Required]
    public string Provider { get; set; } = "noop";

    public string SourceLanguage { get; set; } = "en";

    public string TargetLanguage { get; set; } = "zh-CN";

    [Range(1, 200)]
    public int BatchSize { get; set; } = 20;

    [Range(0, 10)]
    public int MaxRetries { get; set; } = 3;

    [Range(5, 300)]
    public int RequestTimeoutSeconds { get; set; } = 30;

    public bool EnableCaching { get; set; } = true;

    public Dictionary<string, TranslationProviderOptions> Providers { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public TranslationProviderOptions GetProviderOptions(string provider)
    {
        if (Providers.TryGetValue(provider, out var options))
        {
            return options;
        }

        return new TranslationProviderOptions();
    }
}

public class TranslationProviderOptions
{
    public string? ApiKey { get; set; }
    public string? ApiBase { get; set; }
    public string? Model { get; set; }
    public Dictionary<string, string> Extra { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
