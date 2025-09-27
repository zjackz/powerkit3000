using System;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Hosting;
using pk.core.Diagnostics;

namespace pk.api.Monitoring;

/// <summary>
/// 捕获 PowerKit 运行时指标的内存快照，便于通过 HTTP 暴露监控数据。
/// </summary>
public sealed class MetricsSnapshotService : IHostedService, IDisposable
{
    private readonly MeterListener _listener;
    private readonly ConcurrentDictionary<string, double> _counters = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, HistogramAggregate> _histograms = new(StringComparer.Ordinal);
    private DateTimeOffset _lastUpdated = DateTimeOffset.MinValue;
    private bool _disposed;

    /// <summary>
    /// 初始化监听器并注册指标回调。
    /// </summary>
    public MetricsSnapshotService()
    {
        _listener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == PowerKitMetrics.MeterName)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            }
        };

        _listener.SetMeasurementEventCallback<long>(OnCounterRecorded);
        _listener.SetMeasurementEventCallback<double>(OnHistogramRecorded);
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        _lastUpdated = DateTimeOffset.UtcNow;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        DisposeListener();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeListener();
    }

    /// <summary>
    /// 释放监听器资源。
    /// </summary>
    private void DisposeListener()
    {
        if (_disposed)
        {
            return;
        }

        _listener.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// 构建当前内存指标的快照。
    /// </summary>
    public MetricsSnapshot CreateSnapshot()
    {
        return new MetricsSnapshot(
            _counters.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            _histograms.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToSummary()),
            _lastUpdated);
    }

    /// <summary>
    /// 计数器事件回调。
    /// </summary>
    private void OnCounterRecorded(
        Instrument instrument,
        long measurement,
        ReadOnlySpan<KeyValuePair<string, object?>> tags,
        object? state)
    {
        if (measurement == 0)
        {
            return;
        }

        var key = BuildKey(instrument.Name, tags);
        _counters.AddOrUpdate(key, measurement, (_, current) => current + measurement);
        _lastUpdated = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 直方图事件回调。
    /// </summary>
    private void OnHistogramRecorded(
        Instrument instrument,
        double measurement,
        ReadOnlySpan<KeyValuePair<string, object?>> tags,
        object? state)
    {
        var key = BuildKey(instrument.Name, tags);
        _histograms.AddOrUpdate(key,
            _ => HistogramAggregate.FromValue(measurement),
            (_, aggregate) => aggregate.Add(measurement));
        _lastUpdated = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 根据指标名称与标签构建键。
    /// </summary>
    private static string BuildKey(string instrumentName, ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        if (tags.Length == 0)
        {
            return instrumentName;
        }

        var builder = new StringBuilder(instrumentName);
        builder.Append('{');
        for (var i = 0; i < tags.Length; i++)
        {
            var tag = tags[i];
            builder.Append(tag.Key);
            builder.Append('=');
            builder.Append(tag.Value ?? "null");
            if (i < tags.Length - 1)
            {
                builder.Append(',');
            }
        }

        builder.Append('}');
        return builder.ToString();
    }

    /// <summary>
    /// 直方图聚合中间结果。
    /// </summary>
    private readonly record struct HistogramAggregate(double Sum, long Count, double Min, double Max)
    {
        public static HistogramAggregate FromValue(double value) => new(value, 1, value, value);

        public HistogramAggregate Add(double value)
        {
            var newSum = Sum + value;
            var newCount = Count + 1;
            var newMin = Math.Min(Min, value);
            var newMax = Math.Max(Max, value);
            return new HistogramAggregate(newSum, newCount, newMin, newMax);
        }

        public HistogramSummary ToSummary()
        {
            var average = Count == 0 ? 0 : Sum / Count;
            return new HistogramSummary(Count, Sum, Min, Max, average);
        }
    }
}

/// <summary>
/// 直方图汇总信息。
/// </summary>
/// <param name="Count">样本数量。</param>
/// <param name="Sum">总和。</param>
/// <param name="Min">最小值。</param>
/// <param name="Max">最大值。</param>
/// <param name="Average">平均值。</param>
public sealed record HistogramSummary(long Count, double Sum, double Min, double Max, double Average);

/// <summary>
/// 指标快照数据。
/// </summary>
/// <param name="Counters">计数器集合。</param>
/// <param name="Histograms">直方图集合。</param>
/// <param name="LastUpdatedUtc">最后更新时间。</param>
public sealed record MetricsSnapshot(
    IReadOnlyDictionary<string, double> Counters,
    IReadOnlyDictionary<string, HistogramSummary> Histograms,
    DateTimeOffset LastUpdatedUtc);
