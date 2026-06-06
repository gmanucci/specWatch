using System.Reflection;
using SpecWatch.Cli.Commands;
using SpecWatch.Core;

namespace SpecWatch.Cli;

/// <summary>
/// SpecWatch CLI command dispatcher. Kept separate from <c>Program</c> so the
/// command surface can be unit-tested with injected output writers.
/// </summary>
internal static class CliApplication
{
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter error)
    {
        if (args.Count == 0)
        {
            PrintHelp(output);
            return ExitCodes.Success;
        }

        var command = args[0];
        var rest = args.Skip(1).ToArray();

        switch (command)
        {
            case "--version":
            case "-v":
                output.WriteLine(GetVersion());
                return ExitCodes.Success;

            case "--help":
            case "-h":
            case "help":
                PrintHelp(output);
                return ExitCodes.Success;

            case "validate":
                return ValidateCommand.Run(rest, output, error);

            case "check":
                return CheckCommand.Run(rest, output, error);

            case "update":
                return UpdateCommand.Run(rest, output, error);

            case "init":
                return InitCommand.Run(rest, output, error);

            default:
                error.WriteLine($"Unknown command '{command}'. Run 'specwatch --help' for usage.");
                return ExitCodes.GeneralError;
        }
    }

    private static void PrintHelp(TextWriter output)
    {
        output.WriteLine($"SpecWatch {GetVersion()}");
        output.WriteLine("SpecWatch keeps generated API clients in sync with OpenAPI contracts.");
        output.WriteLine();
        output.WriteLine("Usage: specwatch <command> [options]");
        output.WriteLine();
        output.WriteLine("Commands:");
        output.WriteLine("  validate   Validate a manifest file.");
        output.WriteLine("  check      Check watched specs for changes.");
        output.WriteLine("  update     Regenerate clients for changed specs.");
        output.WriteLine("  init       Scaffold CI/CD templates (e.g. 'init azure-devops').");
        output.WriteLine();
        output.WriteLine("Common options:");
        output.WriteLine("  --manifest <path>   Manifest file path (default: specwatch.yml).");
        output.WriteLine("  --version           Print the SpecWatch version.");
    }

    public static string GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "0.0.0";
    }
}
