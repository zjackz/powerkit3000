using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace pk.core.translations;

public class OpenAiTranslationProvider : ITranslationProvider
{
    private const string DefaultApiBase = "https://api.openai.com/v1/";
    private const string DefaultModel = "gpt-4o-mini";
    private readonly ILogger<OpenAiTranslationProvider> _logger;

    public OpenAiTranslationProvider(ILogger<OpenAiTranslationProvider> logger)
    {
        _logger = logger;
    }

    public string Name => "openai";

    public async Task<IReadOnlyList<TranslationResult>> TranslateBatchAsync(
        IReadOnlyList<TranslationInput> inputs,
        TranslationOptions options,
        CancellationToken cancellationToken = default)
    {
        if (inputs.Count == 0)
        {
            return Array.Empty<TranslationResult>();
        }

        var providerOptions = options.GetProviderOptions(Name);
        if (string.IsNullOrWhiteSpace(providerOptions.ApiKey))
        {
            throw new InvalidOperationException("OpenAI 翻译提供方需要配置 Translation:Providers:openai:ApiKey。");
        }

        var apiBase = providerOptions.ApiBase ?? DefaultApiBase;
        var model = providerOptions.Model ?? DefaultModel;
        using var httpClient = CreateHttpClient(apiBase, options.RequestTimeoutSeconds, providerOptions.ApiKey);

        var requestPayload = BuildRequestPayload(inputs, options, model);
        var requestContent = new StringContent(JsonSerializer.Serialize(requestPayload), System.Text.Encoding.UTF8, "application/json");

        using var response = await httpClient.PostAsync("chat/completions", requestContent, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("OpenAI 翻译接口返回错误：{Status} - {Message}", response.StatusCode, errorText);
            return inputs.Select(input => new TranslationResult(input.SourceText, null, false,
                $"OpenAI 请求失败：{response.StatusCode}", input.Identifier)).ToList();
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var json = await JsonSerializer.DeserializeAsync<ChatCompletionResponse>(stream, cancellationToken: cancellationToken);
        if (json?.Choices is not { Length: > 0 })
        {
            _logger.LogError("OpenAI 返回结果为空或格式不正确。");
            return inputs.Select(input => new TranslationResult(input.SourceText, null, false,
                "OpenAI 返回结果为空", input.Identifier)).ToList();
        }

        var content = json.Choices[0].Message?.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            return inputs.Select(input => new TranslationResult(input.SourceText, null, false,
                "OpenAI 返回空内容", input.Identifier)).ToList();
        }

        TranslationEnvelope? envelope = null;
        try
        {
            envelope = JsonSerializer.Deserialize<TranslationEnvelope>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "解析 OpenAI 翻译结果失败，原始文本：{Content}", content);
        }

        if (envelope?.Items is null)
        {
            return inputs.Select(input => new TranslationResult(input.SourceText, null, false,
                "OpenAI 翻译结果解析失败", input.Identifier)).ToList();
        }

        var results = new List<TranslationResult>(inputs.Count);
        foreach (var input in inputs)
        {
            var key = input.Identifier ?? input.SourceText;
            if (envelope.Items.TryGetValue(key, out var translated) && !string.IsNullOrWhiteSpace(translated))
            {
                results.Add(new TranslationResult(input.SourceText, translated, true, Identifier: input.Identifier));
            }
            else
            {
                results.Add(new TranslationResult(input.SourceText, null, false,
                    "未找到匹配的翻译条目", input.Identifier));
            }
        }

        return results;
    }

    private static object BuildRequestPayload(IReadOnlyList<TranslationInput> inputs, TranslationOptions options, string model)
    {
        var payloadItems = inputs.Select((input, index) => new PromptItem
        {
            Id = input.Identifier ?? index.ToString(),
            Text = input.SourceText
        }).ToList();

        var promptBody = new PromptEnvelope
        {
            TargetLanguage = options.TargetLanguage,
            SourceLanguage = options.SourceLanguage,
            Items = payloadItems
        };

        var serializedEnvelope = JsonSerializer.Serialize(promptBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var systemPrompt = "You are a professional translation engine. Translate provided texts into the target language while keeping special characters, markdown, HTML tags and placeholders intact.";
        var userPrompt = $"Return a JSON object with key 'items', mapping each item's id to its translated text. Keep line breaks and punctuation identical unless strictly required by grammar. Here is the payload: {serializedEnvelope}";

        return new
        {
            model,
            temperature = 0,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };
    }

    private static HttpClient CreateHttpClient(string apiBase, int timeoutSeconds, string apiKey)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(apiBase),
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return client;
    }

    private sealed class PromptEnvelope
    {
        [JsonPropertyName("targetLanguage")]
        public string TargetLanguage { get; set; } = "zh-CN";

        [JsonPropertyName("sourceLanguage")]
        public string SourceLanguage { get; set; } = "en";

        [JsonPropertyName("items")]
        public List<PromptItem> Items { get; set; } = new();
    }

    private sealed class PromptItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    private sealed class ChatCompletionResponse
    {
        [JsonPropertyName("choices")]
        public Choice[]? Choices { get; set; }
    }

    private sealed class Choice
    {
        [JsonPropertyName("message")]
        public ChoiceMessage? Message { get; set; }
    }

    private sealed class ChoiceMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private sealed class TranslationEnvelope
    {
        [JsonPropertyName("items")]
        public Dictionary<string, string>? Items { get; set; }
    }
}
