using SpecWatch.Core;
using SpecWatch.Core.ChangeDetection;
using SpecWatch.Core.Configuration;
using SpecWatch.Core.Execution;
using SpecWatch.Core.Generation;
using SpecWatch.Core.Pipeline;
using SpecWatch.Core.Sources;

namespace SpecWatch.Cli.Commands;

/// <summary>
/// Implements <c>specwatch update --manifest specwatch.yml</c>
/// (see AGENTS.md, Section 9 "Update Clients"). Fetches specs, updates changed
/// snapshots, regenerates clients, runs validation, and writes a JSON report.
/// </summary>
internal static class UpdateCommand
{
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter error)
    {
        var manifestPath = CommandLineOptions.GetOption(args, "--manifest", CommandLineOptions.DefaultManifestPath);
        var reportPath = CommandLineOptions.GetOption(args, "--report", "specwatch-report.json");

        SpecWatchManifest manifest;
        try
        {
            manifest = new ManifestLoader().LoadFromFile(manifestPath);
        }
        catch (ManifestLoadException ex)
        {
            error.WriteLine($"Manifest validation failed: {ex.Message}");
            return ExitCodes.ManifestValidationFailed;
        }

        var validation = new ManifestValidator().Validate(manifest);
        if (!validation.IsValid)
        {
            error.WriteLine($"Manifest validation failed for '{manifestPath}':");
            foreach (var message in validation.Errors)
            {
                error.WriteLine($"  - {message}");
            }

            return ExitCodes.ManifestValidationFailed;
        }

        var baseDir = Path.GetDirectoryName(Path.GetFullPath(manifestPath));
        baseDir = string.IsNullOrEmpty(baseDir) ? Directory.GetCurrentDirectory() : baseDir;
        var options = new SpecWatchRunOptions
        {
            ManifestPath = manifestPath,
            BaseDirectory = baseDir,
            ReportPath = reportPath,
        };

        using var httpClient = new HttpClient();
        var commandRunner = new ProcessCommandRunner();
        var runner = new SpecWatchRunner(
            new OpenApiSourceFactory(httpClient, baseDirectory: baseDir),
            new HashSpecChangeDetector(),
            new ClientGeneratorFactory(commandRunner, baseDir),
            commandRunner);

        UpdateRunResult result;
        try
        {
            result = runner.UpdateAsync(manifest, options).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            error.WriteLine($"SpecWatch update failed: {ex.Message}");
            return ExitCodes.GeneralError;
        }

        foreach (var api in result.Report.Apis)
        {
            if (api.Error is not null)
            {
                error.WriteLine($"  {api.Name}: ERROR {api.Error}");
            }
            else if (api.Changed)
            {
                var gen = api.GenerationSucceeded == true ? "regenerated" : "generation skipped";
                output.WriteLine($"  {api.Name}: CHANGED ({gen})");
            }
            else
            {
                output.WriteLine($"  {api.Name}: unchanged");
            }
        }

        output.WriteLine(
            $"Updated {result.Report.Summary.ChangedApis} of {result.Report.Summary.TotalApis} API(s). " +
            $"Report written to '{reportPath}'.");

        if (result.HasFetchError)
        {
            return ExitCodes.SourceFetchFailed;
        }

        if (result.HasGenerationError)
        {
            return ExitCodes.GenerationFailed;
        }

        var failOnValidationError = manifest.Settings?.Validation?.FailOnValidationError ?? true;
        if (result.HasValidationError && failOnValidationError)
        {
            return ExitCodes.ValidationCommandFailed;
        }

        return ExitCodes.Success;
    }
}
