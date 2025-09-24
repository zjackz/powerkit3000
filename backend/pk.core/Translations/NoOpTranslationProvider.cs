using Microsoft.Extensions.Logging;

namespace pk.core.translations;

/// <summary>
/// 默认空实现，用于本地开发或未配置真实翻译服务时直接返回原文。
/// </summary>
public class NoOpTranslationProvider : ITranslationProvider
{
    private readonly ILogger<NoOpTranslationProvider> _logger;

    public NoOpTranslationProvider(ILogger<NoOpTranslationProvider> logger)
    {
        _logger = logger;
    }

    public string Name => "noop";

    public Task<IReadOnlyList<TranslationResult>> TranslateBatchAsync(
        IReadOnlyList<TranslationInput> inputs,
        TranslationOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("使用 NoOp 翻译提供方，所有文本将原样返回。请在配置中设置实际的翻译 Provider。");
        var results = inputs
            .Select(input => new TranslationResult(
                input.SourceText,
                input.SourceText,
                true,
                Identifier: input.Identifier))
            .ToList();

        return Task.FromResult<IReadOnlyList<TranslationResult>>(results);
    }
}
