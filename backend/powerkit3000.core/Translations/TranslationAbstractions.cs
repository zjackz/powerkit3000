namespace powerkit3000.core.translations;

public sealed record TranslationInput(
    string SourceText,
    string TargetLanguage,
    string? SourceLanguage = null,
    string? Context = null,
    string? Identifier = null
);

public sealed record TranslationResult(
    string OriginalText,
    string? TranslatedText,
    bool Success,
    string? Error = null,
    string? Identifier = null
);

public interface ITranslationProvider
{
    string Name { get; }

    Task<IReadOnlyList<TranslationResult>> TranslateBatchAsync(
        IReadOnlyList<TranslationInput> inputs,
        TranslationOptions options,
        CancellationToken cancellationToken = default);
}

public interface ITranslationService
{
    Task<IReadOnlyList<TranslationResult>> TranslateAsync(
        IReadOnlyList<TranslationInput> inputs,
        CancellationToken cancellationToken = default);
}
