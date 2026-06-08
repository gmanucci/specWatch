namespace SpecWatch.Core.Execution;

/// <summary>
/// Parses a validation command string (e.g., <c>dotnet test</c>) into a file
/// name and argument list. Supports double-quoted segments so paths with spaces
/// can be expressed. No shell metacharacters are interpreted (see AGENTS.md,
/// Section 19).
/// </summary>
public static class CommandLineParser
{
    public static (string FileName, List<string> Arguments) Parse(string commandLine)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandLine);

        var tokens = Tokenize(commandLine);
        if (tokens.Count == 0)
        {
            throw new ArgumentException("Command line did not contain an executable.", nameof(commandLine));
        }

        var fileName = tokens[0];
        var arguments = tokens.Skip(1).ToList();
        return (fileName, arguments);
    }

    private static List<string> Tokenize(string input)
    {
        var tokens = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach (var ch in input)
        {
            switch (ch)
            {
                case '"':
                    inQuotes = !inQuotes;
                    break;
                case ' ' or '\t' when !inQuotes:
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }

                    break;
                default:
                    current.Append(ch);
                    break;
            }
        }

        if (current.Length > 0)
        {
            tokens.Add(current.ToString());
        }

        return tokens;
    }
}
