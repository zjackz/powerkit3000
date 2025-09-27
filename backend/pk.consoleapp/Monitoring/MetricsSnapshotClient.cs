using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace consoleapp.Monitoring;

public class MetricsSnapshotClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MetricsSnapshotClient> _logger;

    public MetricsSnapshotClient(HttpClient httpClient, ILogger<MetricsSnapshotClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var envUrl = Environment.GetEnvironmentVariable("PK_MONITORING_BASE_URL");
        if (!string.IsNullOrWhiteSpace(envUrl))
        {
            DefaultEndpoint = envUrl.TrimEnd('/') + "/monitoring/metrics";
        }
    }

    public string DefaultEndpoint { get; set; } = "http://172.31.69.200:5200/monitoring/metrics";

    public async Task<MetricsSnapshot?> GetSnapshotAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("请求 {Url} 失败，状态码 {StatusCode}", url, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<MetricsSnapshot>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "请求 {Url} 失败", url);
            throw;
        }
    }
}

public sealed record MetricsSnapshot(
    IReadOnlyDictionary<string, double> Counters,
    IReadOnlyDictionary<string, HistogramSummary> Histograms,
    DateTimeOffset LastUpdatedUtc);

public sealed record HistogramSummary(long Count, double Sum, double Min, double Max, double Average);
