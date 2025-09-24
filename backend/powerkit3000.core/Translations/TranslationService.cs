using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace powerkit3000.core.translations;

public class TranslationService : ITranslationService
{
    private readonly IReadOnlyDictionary<string, ITranslationProvider> _providers;
    private readonly TranslationOptions _options;
    private readonly ILogger<TranslationService> _logger;
    private readonly Dictionary<string, string> _memoryCache = new(StringComparer.Ordinal);

    public TranslationService(
        IEnumerable<ITranslationProvider> providers,
        IOptions<TranslationOptions> options,
        ILogger<TranslationService> logger)
    {
        _providers = providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TranslationResult>> TranslateAsync(
        IReadOnlyList<TranslationInput> inputs,
        CancellationToken cancellationToken = default)
    {
        if (inputs.Count == 0)
        {
            return Array.Empty<TranslationResult>();
        }

        var providerName = _options.Provider;
        if (!_providers.TryGetValue(providerName, out var provider))
        {
            throw new InvalidOperationException($"未找到名为 '{providerName}' 的翻译提供方，请检查配置。");
        }

        var deduplicatedInputs = DeduplicateInputs(inputs);
        var toTranslate = deduplicatedInputs
            .Select(tuple => tuple.Item1)
            .Where(input => !_options.EnableCaching || !_memoryCache.ContainsKey(input.SourceText))
            .ToList();

        var sessionResults = new Dictionary<string, string>(StringComparer.Ordinal);

        if (toTranslate.Count > 0)
        {
            await TranslateWithRetriesAsync(providerName, provider, toTranslate, sessionResults, cancellationToken);
        }

        var results = new List<TranslationResult>(inputs.Count);
        foreach (var (input, _) in inputs.Select((value, index) => (value, index)))
        {
            if (_memoryCache.TryGetValue(input.SourceText, out var cached))
            {
                results.Add(new TranslationResult(input.SourceText, cached, true, Identifier: input.Identifier));
            }
            else if (sessionResults.TryGetValue(input.SourceText, out var sessionValue))
            {
                results.Add(new TranslationResult(input.SourceText, sessionValue, true, Identifier: input.Identifier));
            }
            else
            {
                results.Add(new TranslationResult(input.SourceText, null, false, "未获得翻译结果", input.Identifier));
            }
        }

        return results;
    }

    private async Task TranslateWithRetriesAsync(
        string providerName,
        ITranslationProvider provider,
        List<TranslationInput> pending,
        Dictionary<string, string> sessionResults,
        CancellationToken cancellationToken)
    {
        var attempt = 0;
        var remaining = pending;

        while (remaining.Count > 0 && attempt <= _options.MaxRetries)
        {
            if (attempt > 0)
            {
                var delaySeconds = Math.Pow(2, attempt);
                _logger.LogWarning("翻译提供方 {Provider} 第 {Attempt} 次重试，等待 {Delay} 秒后再次尝试。", providerName, attempt, delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
            }

            _logger.LogInformation("第 {Attempt} 次向 {Provider} 发送 {Count} 条文本", attempt + 1, providerName, remaining.Count);

            IReadOnlyList<TranslationResult> translateResults;
            try
            {
                translateResults = await provider.TranslateBatchAsync(remaining, _options, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用翻译提供方 {Provider} 失败。", providerName);
                attempt++;
                continue;
            }

            var failed = new List<TranslationInput>();
            foreach (var result in translateResults)
            {
                if (result.Success && !string.IsNullOrWhiteSpace(result.TranslatedText))
                {
                    sessionResults[result.OriginalText] = result.TranslatedText!;
                    if (_options.EnableCaching)
                    {
                        _memoryCache[result.OriginalText] = result.TranslatedText!;
                    }
                }
                else
                {
                    _logger.LogWarning("文本翻译失败：{Identifier} - {Error}", result.Identifier ?? result.OriginalText, result.Error);
                    var input = pending.FirstOrDefault(p => p.SourceText == result.OriginalText);
                    if (input is not null)
                    {
                        failed.Add(input);
                    }
                }
            }

            if (failed.Count == 0)
            {
                break;
            }

            remaining = failed;
            attempt++;
        }

        if (remaining.Count > 0)
        {
            _logger.LogError("在尝试 {Attempts} 次后仍有 {Count} 条文本翻译失败。", attempt, remaining.Count);
        }
    }

    private List<(TranslationInput Item1, int Index)> DeduplicateInputs(IReadOnlyList<TranslationInput> inputs)
    {
        var seen = new HashSet<string>();
        var result = new List<(TranslationInput, int)>(inputs.Count);

        foreach (var (input, index) in inputs.Select((input, idx) => (input, idx)))
        {
            var cacheKey = input.SourceText;
            if (!_options.EnableCaching || seen.Add(cacheKey))
            {
                result.Add((input, index));
            }
        }

        return result;
    }
}
