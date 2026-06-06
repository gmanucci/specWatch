using SpecWatch.Core.ChangeDetection;
using SpecWatch.Core.Configuration;
using SpecWatch.Core.Reporting;
using SpecWatch.Core.Sources;

namespace SpecWatch.Core.Pipeline;

/// <summary>The outcome of a <c>check</c> run.</summary>
public sealed class CheckRunResult
{
    public required UpdateReport Report { get; init; }

    /// <summary>True if at least one enabled API's spec changed.</summary>
    public bool HasChanges { get; init; }

    /// <summary>True if at least one source could not be fetched.</summary>
    public bool HasFetchError { get; init; }
}

/// <summary>
/// Orchestrates SpecWatch operations over a manifest (see AGENTS.md, Sections 6
/// and 9). Sprint 2 implements <c>check</c>; <c>update</c> is added in Sprint 3.
/// </summary>
public sealed class SpecWatchRunner
{
    private readonly IOpenApiSourceFactory _sourceFactory;
    private readonly ISpecChangeDetector _changeDetector;

    public SpecWatchRunner(IOpenApiSourceFactory sourceFactory, ISpecChangeDetector changeDetector)
    {
        ArgumentNullException.ThrowIfNull(sourceFactory);
        ArgumentNullException.ThrowIfNull(changeDetector);
        _sourceFactory = sourceFactory;
        _changeDetector = changeDetector;
    }

    /// <summary>
    /// Fetches all configured specs and compares them to their snapshots without
    /// modifying any files (see AGENTS.md, Section 9 "Check for Changes").
    /// </summary>
    public async Task<CheckRunResult> CheckAsync(
        SpecWatchManifest manifest,
        SpecWatchRunOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(options);

        var baseDir = options.BaseDirectory ?? Directory.GetCurrentDirectory();
        var report = new UpdateReport
        {
            StartedAt = DateTimeOffset.UtcNow,
            ManifestPath = options.ManifestPath,
        };

        var hasChanges = false;
        var hasFetchError = false;

        foreach (var api in manifest.Apis)
        {
            if (!api.Enabled)
            {
                continue;
            }

            var result = new ApiUpdateResult
            {
                Name = api.Name ?? "<unnamed>",
                Source = DescribeSource(api.Source),
                SnapshotPath = api.Snapshot?.Path,
                Generator = api.Client?.Generator,
                Language = api.Client?.Language,
                Output = api.Client?.Output,
            };

            try
            {
                var source = _sourceFactory.Create(api.Source!);
                var fetched = await source.FetchAsync(cancellationToken).ConfigureAwait(false);

                var snapshotPath = ResolvePath(baseDir, api.Snapshot!.Path!);
                var change = _changeDetector.Detect(fetched.Content, snapshotPath);
                result.Changed = change.Changed;
                if (change.Changed)
                {
                    hasChanges = true;
                }
            }
            catch (SourceFetchException ex)
            {
                hasFetchError = true;
                result.Error = ex.Message;
            }

            report.Apis.Add(result);
        }

        report.FinishedAt = DateTimeOffset.UtcNow;
        report.RecomputeSummary();

        return new CheckRunResult
        {
            Report = report,
            HasChanges = hasChanges,
            HasFetchError = hasFetchError,
        };
    }

    private static string? DescribeSource(SourceDefinition? source) =>
        source is null ? null : source.Url ?? source.Path;

    private static string ResolvePath(string baseDir, string path) =>
        Path.IsPathRooted(path) ? path : Path.Combine(baseDir, path);
}
