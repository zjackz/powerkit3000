using Microsoft.Extensions.Logging;

namespace powerkit3000.core.translations;

public class DeepSeekTranslationProvider : ITranslationProvider
{
    private readonly ILogger<DeepSeekTranslationProvider> _logger;

    public DeepSeekTranslationProvider(ILogger<DeepSeekTranslationProvider> logger)
    {
        _logger = logger;
    }

    public string Name => "deepseek";

    public Task<IReadOnlyList<TranslationResult>> TranslateBatchAsync(
        IReadOnlyList<TranslationInput> inputs,
        TranslationOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogError("DeepSeek 翻译 Provider 尚未实现，请在 Translation:Provider 中选择已实现的提供方或扩展此类。");
        throw new NotSupportedException("DeepSeek translation provider is not implemented yet.");
    }
}
