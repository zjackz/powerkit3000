using System;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Hosting;
using pk.core.Diagnostics;

namespace pk.api.Monitoring;

/// <summary>
/// Captures in-memory aggregates for PowerKit metrics so they can be surfaced via HTTP endpoints without external dependencies.
/// </summary>
public sealed class MetricsSnapshotService : IHostedService, IDisposable
{
    private readonly MeterListener _listener;
    private readonly ConcurrentDictionary<string, double> _counters = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, HistogramAggregate> _histograms = new(StringComparer.Ordinal);
    private DateTimeOffset _lastUpdated = DateTimeOffset.MinValue;
    private bool _disposed;

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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        _lastUpdated = DateTimeOffset.UtcNow;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        DisposeListener();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        DisposeListener();
    }

    private void DisposeListener()
    {
        if (_disposed)
        {
            return;
        }

        _listener.Dispose();
        _disposed = true;
    }

    public MetricsSnapshot CreateSnapshot()
    {
        return new MetricsSnapshot(
            _counters.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            _histograms.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToSummary()),
            _lastUpdated);
    }

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

public sealed record HistogramSummary(long Count, double Sum, double Min, double Max, double Average);

public sealed record MetricsSnapshot(
    IReadOnlyDictionary<string, double> Counters,
    IReadOnlyDictionary<string, HistogramSummary> Histograms,
    DateTimeOffset LastUpdatedUtc);
