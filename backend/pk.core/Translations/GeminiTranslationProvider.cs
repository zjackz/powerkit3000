using Microsoft.Extensions.Logging;

namespace pk.core.translations;

public class GeminiTranslationProvider : ITranslationProvider
{
    private readonly ILogger<GeminiTranslationProvider> _logger;

    public GeminiTranslationProvider(ILogger<GeminiTranslationProvider> logger)
    {
        _logger = logger;
    }

    public string Name => "gemini";

    public Task<IReadOnlyList<TranslationResult>> TranslateBatchAsync(
        IReadOnlyList<TranslationInput> inputs,
        TranslationOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogError("Gemini 翻译 Provider 尚未实现，请在 Translation:Provider 中选择已实现的提供方或扩展此类。");
        throw new NotSupportedException("Gemini translation provider is not implemented yet.");
    }
}
