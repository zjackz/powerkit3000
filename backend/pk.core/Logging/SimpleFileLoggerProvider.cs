using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace pk.core.Logging;

/// <summary>
/// Minimal file logger used when we want deterministic, on-disk logs without pulling extra dependencies.
/// </summary>
public sealed class SimpleFileLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly string _filePath;
    private readonly LogLevel _minimumLevel;
    private readonly string _timestampFormat;
    private readonly object _writeLock = new();
    private IExternalScopeProvider? _scopeProvider;
    private readonly ConcurrentDictionary<string, SimpleFileLogger> _loggers = new(StringComparer.Ordinal);

    public SimpleFileLoggerProvider(string filePath, LogLevel minimumLevel, string timestampFormat)
    {
        _filePath = filePath;
        _minimumLevel = minimumLevel;
        _timestampFormat = string.IsNullOrWhiteSpace(timestampFormat)
            ? "yyyy-MM-ddTHH:mm:ss.fffZ"
            : timestampFormat;

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new SimpleFileLogger(name, this));
    }

    internal bool IsEnabled(LogLevel level) => level >= _minimumLevel;

    internal void WriteEntry(string category, LogLevel level, string message, Exception? exception, string? scopes)
    {
        if (!IsEnabled(level))
        {
            return;
        }

        var timestamp = DateTimeOffset.UtcNow.ToString(_timestampFormat);
        var builder = new StringBuilder(256);
        builder.Append('[').Append(timestamp).Append("] ");
        builder.Append('[').Append(level).Append("] ");
        builder.Append(category).Append(" - ").Append(message);

        if (!string.IsNullOrWhiteSpace(scopes))
        {
            builder.Append(" | Scopes: ").Append(scopes);
        }

        if (exception is not null)
        {
            builder.AppendLine();
            builder.Append(exception);
        }

        builder.AppendLine();
        var entry = builder.ToString();

        lock (_writeLock)
        {
            File.AppendAllText(_filePath, entry);
        }
    }

    internal IExternalScopeProvider? ScopeProvider => _scopeProvider;

    public void Dispose()
    {
        _loggers.Clear();
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    private sealed class SimpleFileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly SimpleFileLoggerProvider _provider;

        public SimpleFileLogger(string categoryName, SimpleFileLoggerProvider provider)
        {
            _categoryName = categoryName;
            _provider = provider;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _provider.ScopeProvider?.Push(state);
        }

        public bool IsEnabled(LogLevel logLevel) => _provider.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message) && exception is null)
            {
                return;
            }

            string? scopes = null;
            var scopeProvider = _provider.ScopeProvider;
            if (scopeProvider is not null)
            {
                var scopeBuilder = new StringBuilder();
                scopeProvider.ForEachScope((scope, builder) =>
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(" => ");
                    }
                    builder.Append(scope);
                }, scopeBuilder);
                scopes = scopeBuilder.ToString();
            }

            _provider.WriteEntry(_categoryName, logLevel, message, exception, scopes);
        }
    }
}
