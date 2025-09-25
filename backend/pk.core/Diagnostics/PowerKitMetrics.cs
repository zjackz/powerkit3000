using System.Diagnostics.Metrics;

namespace pk.core.Diagnostics;

/// <summary>
/// Centralized application metrics for Kickstarter/Amazon workloads.
/// </summary>
public static class PowerKitMetrics
{
    public const string MeterName = "pk.core.metrics";
    public static readonly Meter Meter = new(MeterName, "1.0.0");

    public static readonly Counter<long> KickstarterImportBatches =
        Meter.CreateCounter<long>("pk_kickstarter_import_batches_total", description: "Total Kickstarter import batches processed");

    public static readonly Counter<long> KickstarterImportFiles =
        Meter.CreateCounter<long>("pk_kickstarter_import_files_total", description: "Number of Kickstarter import files processed");

    public static readonly Counter<long> KickstarterImportedProjects =
        Meter.CreateCounter<long>("pk_kickstarter_import_projects_total", description: "Number of Kickstarter projects imported successfully");

    public static readonly Counter<long> KickstarterImportSkippedProjects =
        Meter.CreateCounter<long>("pk_kickstarter_import_projects_skipped_total", description: "Number of Kickstarter projects skipped due to duplicates or validation issues");

    public static readonly Counter<long> KickstarterImportFailures =
        Meter.CreateCounter<long>("pk_kickstarter_import_failures_total", description: "Number of Kickstarter import batch failures");

    public static readonly Counter<long> KickstarterImportParseErrors =
        Meter.CreateCounter<long>("pk_kickstarter_import_parse_errors_total", description: "Number of Kickstarter records skipped due to parse errors");

    public static readonly Counter<long> KickstarterImportValidationErrors =
        Meter.CreateCounter<long>("pk_kickstarter_import_validation_errors_total", description: "Number of Kickstarter records skipped due to validation errors");

    public static readonly Histogram<double> KickstarterImportBatchDuration =
        Meter.CreateHistogram<double>("pk_kickstarter_import_batch_duration_ms", unit: "ms", description: "Duration of Kickstarter import batch processing");

    public static readonly Counter<long> KickstarterQueries =
        Meter.CreateCounter<long>("pk_kickstarter_queries_total", description: "Number of Kickstarter aggregate queries executed");

    public static readonly Histogram<double> KickstarterQueryDuration =
        Meter.CreateHistogram<double>("pk_kickstarter_query_duration_ms", unit: "ms", description: "Duration of Kickstarter aggregate queries");
}
