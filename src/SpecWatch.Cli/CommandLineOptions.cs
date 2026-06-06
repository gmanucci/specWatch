namespace SpecWatch.Cli;

/// <summary>
/// Minimal command-line argument helpers shared by SpecWatch CLI commands.
/// </summary>
internal static class CommandLineOptions
{
    public const string DefaultManifestPath = "specwatch.yml";

    /// <summary>
    /// Returns the value for an option flag (e.g., <c>--manifest path</c>), or
    /// <paramref name="defaultValue"/> if the flag is absent.
    /// </summary>
    public static string GetOption(IReadOnlyList<string> args, string name, string defaultValue)
    {
        for (var i = 0; i < args.Count - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.Ordinal))
            {
                return args[i + 1];
            }
        }

        return defaultValue;
    }

    /// <summary>Returns whether a boolean flag (e.g., <c>--force</c>) is present.</summary>
    public static bool HasFlag(IReadOnlyList<string> args, string name) =>
        args.Any(a => string.Equals(a, name, StringComparison.Ordinal));
}
