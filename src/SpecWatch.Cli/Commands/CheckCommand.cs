using SpecWatch.Core;
using SpecWatch.Core.ChangeDetection;
using SpecWatch.Core.Configuration;
using SpecWatch.Core.Pipeline;
using SpecWatch.Core.Sources;

namespace SpecWatch.Cli.Commands;

/// <summary>
/// Implements <c>specwatch check --manifest specwatch.yml</c>
/// (see AGENTS.md, Section 9 "Check for Changes"). Exit codes: 0 no changes,
/// 2 changes detected, 4 source fetch failed, 1 other error, 3 invalid manifest.
/// </summary>
internal static class CheckCommand
{
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter error)
    {
        var manifestPath = CommandLineOptions.GetOption(args, "--manifest", CommandLineOptions.DefaultManifestPath);

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
        var options = new SpecWatchRunOptions
        {
            ManifestPath = manifestPath,
            BaseDirectory = string.IsNullOrEmpty(baseDir) ? Directory.GetCurrentDirectory() : baseDir,
        };

        using var httpClient = new HttpClient();
        var runner = new SpecWatchRunner(
            new OpenApiSourceFactory(httpClient, baseDirectory: options.BaseDirectory),
            new HashSpecChangeDetector());

        CheckRunResult result;
        try
        {
            result = runner.CheckAsync(manifest, options).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            error.WriteLine($"SpecWatch check failed: {ex.Message}");
            return ExitCodes.GeneralError;
        }

        foreach (var api in result.Report.Apis)
        {
            if (api.Error is not null)
            {
                error.WriteLine($"  {api.Name}: ERROR {api.Error}");
            }
            else
            {
                output.WriteLine($"  {api.Name}: {(api.Changed ? "CHANGED" : "unchanged")}");
            }
        }

        output.WriteLine(
            $"Checked {result.Report.Summary.TotalApis} API(s): {result.Report.Summary.ChangedApis} changed.");

        if (result.HasFetchError)
        {
            return ExitCodes.SourceFetchFailed;
        }

        return result.HasChanges ? ExitCodes.ChangesDetected : ExitCodes.Success;
    }
}
