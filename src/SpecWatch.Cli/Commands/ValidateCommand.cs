using SpecWatch.Core;
using SpecWatch.Core.Configuration;

namespace SpecWatch.Cli.Commands;

/// <summary>
/// Implements <c>specwatch validate --manifest specwatch.yml</c>
/// (see AGENTS.md, Section 9.1). Returns exit code 3 on validation failure.
/// </summary>
internal static class ValidateCommand
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

        var result = new ManifestValidator().Validate(manifest);
        if (result.IsValid)
        {
            output.WriteLine($"Manifest '{manifestPath}' is valid. {manifest.Apis.Count} API(s) configured.");
            return ExitCodes.Success;
        }

        error.WriteLine($"Manifest validation failed for '{manifestPath}':");
        foreach (var message in result.Errors)
        {
            error.WriteLine($"  - {message}");
        }

        return ExitCodes.ManifestValidationFailed;
    }
}
