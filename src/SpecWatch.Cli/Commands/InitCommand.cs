using SpecWatch.Core;

namespace SpecWatch.Cli.Commands;

/// <summary>
/// Scaffolds CI/CD templates. Currently supports <c>init azure-devops</c>, which
/// writes an example <c>azure-pipelines.yml</c> and <c>specwatch.yml</c>
/// (see AGENTS.md, Section 9.1 "Generate Azure Pipeline Template").
/// </summary>
internal static class InitCommand
{
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter error)
    {
        if (args.Count == 0)
        {
            error.WriteLine("Usage: specwatch init <target> [--force]");
            error.WriteLine("Supported targets: azure-devops");
            return ExitCodes.GeneralError;
        }

        var target = args[0];
        var force = args.Skip(1).Any(a => a is "--force" or "-f");
        var directory = Directory.GetCurrentDirectory();

        switch (target)
        {
            case "azure-devops":
                return ScaffoldAzureDevOps(directory, force, output, error);

            default:
                error.WriteLine($"Unknown init target '{target}'. Supported targets: azure-devops.");
                return ExitCodes.GeneralError;
        }
    }

    private static int ScaffoldAzureDevOps(string directory, bool force, TextWriter output, TextWriter error)
    {
        var files = new (string Name, string Content)[]
        {
            ("specwatch.yml", InitTemplates.SpecWatchManifest),
            ("azure-pipelines.yml", InitTemplates.AzurePipeline),
        };

        foreach (var (name, _) in files)
        {
            var path = Path.Combine(directory, name);
            if (File.Exists(path) && !force)
            {
                error.WriteLine($"'{name}' already exists. Use --force to overwrite.");
                return ExitCodes.GeneralError;
            }
        }

        foreach (var (name, content) in files)
        {
            var path = Path.Combine(directory, name);
            File.WriteAllText(path, content);
            output.WriteLine($"Wrote {name}.");
        }

        return ExitCodes.Success;
    }
}
