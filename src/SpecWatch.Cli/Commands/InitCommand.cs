using SpecWatch.Core;

namespace SpecWatch.Cli.Commands;

// Placeholder; implemented in a later sprint.
internal static class InitCommand
{
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter error)
    {
        error.WriteLine("'InitCommand' is not implemented yet.");
        return ExitCodes.GeneralError;
    }
}
