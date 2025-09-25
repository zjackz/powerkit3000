using System.Globalization;
using consoleapp.Monitoring;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace consoleapp.Commands;

public class MetricsCommand
{
    private readonly MetricsSnapshotClient _client;
    private readonly ILogger<MetricsCommand> _logger;

    public MetricsCommand(MetricsSnapshotClient client, ILogger<MetricsCommand> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var options = MetricsCommandOptions.Parse(args, _client.DefaultEndpoint);
        _logger.LogInformation("请求监控指标，地址 {Url}，watch={Watch} interval={Interval}s", options.Url, options.Watch, options.RefreshIntervalSeconds);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (options.Watch)
        {
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                if (!cts.IsCancellationRequested)
                {
                    eventArgs.Cancel = true;
                    cts.Cancel();
                }
            };

            await WatchAsync(options, cts.Token);
        }
        else
        {
            await RenderOnceAsync(options.Url, previous: null, cancellationToken);
        }
    }

    private async Task WatchAsync(MetricsCommandOptions options, CancellationToken cancellationToken)
    {
        MetricsSnapshot? previous = null;
        var refresh = TimeSpan.FromSeconds(options.RefreshIntervalSeconds);

        while (!cancellationToken.IsCancellationRequested)
        {
            previous = await RenderOnceAsync(options.Url, previous, cancellationToken);

            try
            {
                await Task.Delay(refresh, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private async Task<MetricsSnapshot?> RenderOnceAsync(string url, MetricsSnapshot? previous, CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = await _client.GetSnapshotAsync(url, cancellationToken);
            if (snapshot == null)
            {
                AnsiConsole.MarkupLine("[yellow]未获取到指标数据，请确认 API 是否可访问。[/]");
                return previous;
            }

            var anomalies = DetectAnomalies(snapshot);
            AnsiConsole.Clear();
            RenderSnapshot(url, snapshot, previous, anomalies);
            return snapshot;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "获取监控指标失败");
            AnsiConsole.MarkupLine($"[red]获取监控指标失败：{Markup.Escape(ex.Message)}[/]");
            return previous;
        }
    }

    private static void RenderSnapshot(string apiUrl, MetricsSnapshot snapshot, MetricsSnapshot? previous, IReadOnlyList<Anomaly> anomalies)
    {
        var header = new Panel(new Markup($"[bold]监控端点：[/]{Markup.Escape(apiUrl)}\n[bold]最新更新时间：[/]{snapshot.LastUpdatedUtc:yyyy-MM-dd HH:mm:ss} UTC"))
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 1, 1, 1)
        };
        AnsiConsole.Write(header);

        if (anomalies.Count > 0)
        {
            var alert = new Panel(new Markup(string.Join("\n", anomalies.Select(a => a.ToMarkup()))))
            {
                Header = new PanelHeader("⚠ 预警"),
                Border = BoxBorder.Double,
                BorderStyle = new Style(Color.Red)
            };
            AnsiConsole.Write(alert);
        }

        if (snapshot.Counters.Count == 0 && snapshot.Histograms.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]当前没有可展示的指标。[/]");
            return;
        }

        var counterDelta = previous?.Counters ?? ReadOnlyDictionaryCache.EmptyDouble;
        if (snapshot.Counters.Count > 0)
        {
            var countersTable = new Table().Border(TableBorder.Rounded).Title("计数指标");
            countersTable.AddColumn("名称");
            countersTable.AddColumn(new TableColumn("值").RightAligned());
            countersTable.AddColumn(new TableColumn("增量").RightAligned());

            foreach (var counter in snapshot.Counters.OrderByDescending(kvp => kvp.Value))
            {
                var current = counter.Value;
                var delta = counterDelta.TryGetValue(counter.Key, out var previousValue)
                    ? current - previousValue
                    : (double?)null;

                countersTable.AddRow(
                    counter.Key,
                    current.ToString("N0", CultureInfo.InvariantCulture),
                    FormatDelta(delta));
            }

            AnsiConsole.Write(countersTable);
        }

        if (snapshot.Histograms.Count > 0)
        {
            var histogramTable = new Table().Border(TableBorder.Rounded).Title("耗时/分布指标");
            histogramTable.AddColumn("名称");
            histogramTable.AddColumn(new TableColumn("计数").RightAligned());
            histogramTable.AddColumn(new TableColumn("平均值").RightAligned());
            histogramTable.AddColumn(new TableColumn("最小值").RightAligned());
            histogramTable.AddColumn(new TableColumn("最大值").RightAligned());

            foreach (var histogram in snapshot.Histograms.OrderByDescending(kvp => kvp.Value.Average))
            {
                histogramTable.AddRow(
                    histogram.Key,
                    histogram.Value.Count.ToString("N0", CultureInfo.InvariantCulture),
                    FormatDouble(histogram.Value.Average),
                    FormatDouble(histogram.Value.Min),
                    FormatDouble(histogram.Value.Max));
            }

            AnsiConsole.Write(histogramTable);
        }

        RenderParseErrorBreakdown(snapshot);
    }

    private static string FormatDouble(double value)
    {
        if (Math.Abs(value) >= 1000)
        {
            return value.ToString("N0", CultureInfo.InvariantCulture);
        }
        if (Math.Abs(value) >= 1)
        {
            return value.ToString("N2", CultureInfo.InvariantCulture);
        }
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string FormatDelta(double? delta)
    {
        if (delta == null)
        {
            return "-";
        }

        if (Math.Abs(delta.Value) < double.Epsilon)
        {
            return "0";
        }

        var formatted = FormatDouble(delta.Value);
        return delta.Value switch
        {
            > 0 => $"[green]+{formatted}[/]",
            < 0 => $"[red]{formatted}[/]",
            _ => formatted
        };
    }

    private static IReadOnlyList<Anomaly> DetectAnomalies(MetricsSnapshot snapshot)
    {
        var findings = new List<Anomaly>();

        if (snapshot.Counters.TryGetValue("pk_kickstarter_import_failures_total", out var failures) && failures > 0)
        {
            findings.Add(new Anomaly("Import Failures", "[red]检测到导入失败，请检查导入日志[/]"));
        }

        if (snapshot.Counters.TryGetValue("pk_kickstarter_import_parse_errors_total", out var parseErrors) && parseErrors > 0)
        {
            var topSource = GetTopParseErrorLabel(snapshot, "source");
            var message = topSource != null
                ? $"[yellow]解析失败记录主要来源于 {Markup.Escape(topSource)}，请检查数据源格式[/]"
                : "[yellow]存在解析失败的记录，请确认数据源格式[/]";
            findings.Add(new Anomaly("Parse Errors", message));
        }

        if (snapshot.Histograms.TryGetValue("pk_kickstarter_query_duration_ms", out var queryHistogram) && queryHistogram.Average > 2_000)
        {
            findings.Add(new Anomaly("Slow Queries", $"[yellow]查询平均耗时 {FormatDouble(queryHistogram.Average)} ms，高于阈值 2000 ms[/]"));
        }

        return findings;
    }

    private sealed record Anomaly(string Key, string Message)
    {
        public string ToMarkup() => $"[bold]{Key}[/]: {Message}";
    }

    private static string? GetTopParseErrorLabel(MetricsSnapshot snapshot, string tagKey)
    {
        var grouped = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var counter in snapshot.Counters)
        {
            if (!counter.Key.StartsWith("pk_kickstarter_import_parse_errors_total", StringComparison.Ordinal))
            {
                continue;
            }

            if (!TryParseInstrumentKey(counter.Key, out _, out var tags))
            {
                continue;
            }

            if (tags.TryGetValue(tagKey, out var tagValue))
            {
                grouped[tagValue] = grouped.TryGetValue(tagValue, out var existing)
                    ? existing + counter.Value
                    : counter.Value;
            }
        }

        return grouped.Count == 0
            ? null
            : grouped.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    private static void RenderParseErrorBreakdown(MetricsSnapshot snapshot)
    {
        var records = new List<(string Source, string Reason, double Count)>();

        foreach (var counter in snapshot.Counters)
        {
            if (!counter.Key.StartsWith("pk_kickstarter_import_parse_errors_total", StringComparison.Ordinal))
            {
                continue;
            }

            if (!TryParseInstrumentKey(counter.Key, out _, out var tags))
            {
                continue;
            }

            var source = tags.TryGetValue("source", out var sourceTag) ? sourceTag : "unknown";
            var reason = tags.TryGetValue("reason", out var reasonTag) ? reasonTag : "unknown";

            records.Add((source, reason, counter.Value));
        }

        if (records.Count == 0)
        {
            return;
        }

        var table = new Table().Border(TableBorder.Rounded).Title("解析错误明细 (Top 10)");
        table.AddColumn("来源文件");
        table.AddColumn("原因");
        table.AddColumn(new TableColumn("次数").RightAligned());

        foreach (var row in records
                     .OrderByDescending(r => r.Count)
                     .Take(10))
        {
            table.AddRow(row.Source, row.Reason, row.Count.ToString("N0", CultureInfo.InvariantCulture));
        }

        AnsiConsole.Write(table);
    }

    private static bool TryParseInstrumentKey(string key, out string baseName, out Dictionary<string, string> tags)
    {
        tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var start = key.IndexOf('{');
        if (start < 0)
        {
            baseName = key;
            return true;
        }

        baseName = key[..start];
        var end = key.LastIndexOf('}');
        if (end <= start)
        {
            return false;
        }

        var content = key.Substring(start + 1, end - start - 1);
        if (string.IsNullOrWhiteSpace(content))
        {
            return true;
        }

        var parts = content.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2)
            {
                continue;
            }

            tags[kv[0].Trim()] = kv[1].Trim();
        }

        return true;
    }

    private sealed record MetricsCommandOptions(string Url, bool Watch, int RefreshIntervalSeconds)
    {
        public static MetricsCommandOptions Parse(string[] args, string defaultUrl)
        {
            var url = defaultUrl;
            var watch = false;
            var intervalSeconds = 15;

            for (var i = 0; i < args.Length; i++)
            {
                var current = args[i];
                switch (current.ToLowerInvariant())
                {
                    case "--url":
                    case "-u":
                        if (i + 1 < args.Length)
                        {
                            url = args[i + 1];
                            i++;
                        }
                        break;

                    case "--watch":
                    case "-w":
                        watch = true;
                        break;

                    case "--interval":
                    case "-i":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out var parsed) && parsed > 0)
                        {
                            intervalSeconds = parsed;
                            i++;
                        }
                        break;
                }
            }

            return new MetricsCommandOptions(url, watch, intervalSeconds);
        }
    }

    private static class ReadOnlyDictionaryCache
    {
        public static readonly IReadOnlyDictionary<string, double> EmptyDouble = new Dictionary<string, double>();
    }
}
