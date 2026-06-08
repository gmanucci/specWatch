using SpecWatch.Core.ChangeDetection;
using SpecWatch.Core.Configuration;
using SpecWatch.Core.Execution;
using SpecWatch.Core.Generation;
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

/// <summary>The outcome of an <c>update</c> run.</summary>
public sealed class UpdateRunResult
{
    public required UpdateReport Report { get; init; }

    public bool HasChanges { get; init; }

    public bool HasFetchError { get; init; }

    public bool HasGenerationError { get; init; }

    public bool HasValidationError { get; init; }
}

/// <summary>
/// Orchestrates SpecWatch operations over a manifest (see AGENTS.md, Sections 6
/// and 9). Implements <c>check</c> (compare only) and <c>update</c> (compare,
/// update snapshots, regenerate clients, validate, and report).
/// </summary>
public sealed class SpecWatchRunner
{
    private readonly IOpenApiSourceFactory _sourceFactory;
    private readonly ISpecChangeDetector _changeDetector;
    private readonly IClientGeneratorFactory? _generatorFactory;
    private readonly ICommandRunner? _commandRunner;

    public SpecWatchRunner(
        IOpenApiSourceFactory sourceFactory,
        ISpecChangeDetector changeDetector,
        IClientGeneratorFactory? generatorFactory = null,
        ICommandRunner? commandRunner = null)
    {
        ArgumentNullException.ThrowIfNull(sourceFactory);
        ArgumentNullException.ThrowIfNull(changeDetector);
        _sourceFactory = sourceFactory;
        _changeDetector = changeDetector;
        _generatorFactory = generatorFactory;
        _commandRunner = commandRunner;
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
        var report = NewReport(options);
        var hasChanges = false;
        var hasFetchError = false;

        foreach (var api in EnabledApis(manifest))
        {
            var result = NewApiResult(api);
            try
            {
                var fetched = await FetchAsync(api, cancellationToken).ConfigureAwait(false);
                var change = _changeDetector.Detect(fetched.Content, ResolvePath(baseDir, api.Snapshot!.Path!));
                result.Changed = change.Changed;
                hasChanges |= change.Changed;
            }
            catch (SourceFetchException ex)
            {
                hasFetchError = true;
                result.Error = ex.Message;
            }

            report.Apis.Add(result);
        }

        FinalizeReport(report);
        return new CheckRunResult { Report = report, HasChanges = hasChanges, HasFetchError = hasFetchError };
    }

    /// <summary>
    /// Fetches specs, updates changed snapshots, regenerates changed clients,
    /// runs validation commands, and writes a JSON report (see AGENTS.md,
    /// Section 9 "Update Clients").
    /// </summary>
    public async Task<UpdateRunResult> UpdateAsync(
        SpecWatchManifest manifest,
        SpecWatchRunOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(options);

        if (_generatorFactory is null)
        {
            throw new InvalidOperationException("UpdateAsync requires a client generator factory.");
        }

        var baseDir = options.BaseDirectory ?? Directory.GetCurrentDirectory();
        var failOnValidationError = manifest.Settings?.Validation?.FailOnValidationError ?? true;
        var report = NewReport(options);

        var hasChanges = false;
        var hasFetchError = false;
        var hasGenerationError = false;
        var hasValidationError = false;

        foreach (var api in EnabledApis(manifest))
        {
            var result = NewApiResult(api);
            try
            {
                var fetched = await FetchAsync(api, cancellationToken).ConfigureAwait(false);
                var snapshotPath = ResolvePath(baseDir, api.Snapshot!.Path!);
                var change = _changeDetector.Detect(fetched.Content, snapshotPath);
                result.Changed = change.Changed;

                if (!change.Changed)
                {
                    report.Apis.Add(result);
                    continue;
                }

                hasChanges = true;

                // 1. Update the snapshot on disk.
                WriteSnapshot(snapshotPath, fetched.Content);

                // 2. Regenerate the client.
                var generator = _generatorFactory.Create(api.Client!.Generator!);
                var generation = await generator.GenerateAsync(api, cancellationToken).ConfigureAwait(false);
                result.GenerationSucceeded = generation.Success;
                if (!generation.Success)
                {
                    hasGenerationError = true;
                    result.Error = Truncate(generation.StandardError) ?? "Client generation failed.";
                    report.Apis.Add(result);
                    continue;
                }

                // 3. Run validation commands.
                if (api.Validation?.Commands is { Count: > 0 } commands)
                {
                    var validationOk = await RunValidationAsync(commands, baseDir, cancellationToken)
                        .ConfigureAwait(false);
                    result.ValidationSucceeded = validationOk;
                    if (!validationOk)
                    {
                        hasValidationError = true;
                        if (failOnValidationError)
                        {
                            result.Error = "One or more validation commands failed.";
                        }
                    }
                }
            }
            catch (SourceFetchException ex)
            {
                hasFetchError = true;
                result.Error = ex.Message;
            }

            report.Apis.Add(result);
        }

        FinalizeReport(report);

        if (!string.IsNullOrWhiteSpace(options.ReportPath))
        {
            new ReportWriter().WriteToFile(report, ResolvePath(baseDir, options.ReportPath));
        }

        return new UpdateRunResult
        {
            Report = report,
            HasChanges = hasChanges,
            HasFetchError = hasFetchError,
            HasGenerationError = hasGenerationError,
            HasValidationError = hasValidationError,
        };
    }

    private async Task<bool> RunValidationAsync(
        IReadOnlyList<string> commands,
        string baseDir,
        CancellationToken cancellationToken)
    {
        if (_commandRunner is null)
        {
            throw new InvalidOperationException("Running validation commands requires a command runner.");
        }

        foreach (var command in commands)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                continue;
            }

            var (fileName, arguments) = CommandLineParser.Parse(command);
            var result = await _commandRunner
                .RunAsync(fileName, arguments, baseDir, cancellationToken)
                .ConfigureAwait(false);

            if (!result.Success)
            {
                return false;
            }
        }

        return true;
    }

    private Task<OpenApiFetchResult> FetchAsync(ApiWatchDefinition api, CancellationToken cancellationToken)
    {
        var source = _sourceFactory.Create(api.Source!);
        return source.FetchAsync(cancellationToken);
    }

    private static void WriteSnapshot(string snapshotPath, byte[] content)
    {
        var directory = Path.GetDirectoryName(snapshotPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(snapshotPath, content);
    }

    private static IEnumerable<ApiWatchDefinition> EnabledApis(SpecWatchManifest manifest) =>
        manifest.Apis.Where(a => a.Enabled);

    private static UpdateReport NewReport(SpecWatchRunOptions options) => new()
    {
        StartedAt = DateTimeOffset.UtcNow,
        ManifestPath = options.ManifestPath,
    };

    private static ApiUpdateResult NewApiResult(ApiWatchDefinition api) => new()
    {
        Name = api.Name ?? "<unnamed>",
        Source = api.Source?.Url ?? api.Source?.Path,
        SnapshotPath = api.Snapshot?.Path,
        Generator = api.Client?.Generator,
        Language = api.Client?.Language,
        Output = api.Client?.Output,
    };

    private static void FinalizeReport(UpdateReport report)
    {
        report.FinishedAt = DateTimeOffset.UtcNow;
        report.RecomputeSummary();
    }

    private static string ResolvePath(string baseDir, string path) =>
        Path.IsPathRooted(path) ? path : Path.Combine(baseDir, path);

    private static string? Truncate(string? value, int max = 500)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        value = value.Trim();
        return value.Length <= max ? value : value[..max] + "…";
    }
}
