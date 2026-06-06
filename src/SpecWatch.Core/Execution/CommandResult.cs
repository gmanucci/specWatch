namespace SpecWatch.Core.Execution;

/// <summary>The result of running an external command (see AGENTS.md, Section 6).</summary>
public sealed class CommandResult
{
    public required int ExitCode { get; init; }

    public string StandardOutput { get; init; } = "";

    public string StandardError { get; init; } = "";

    /// <summary>The command line that was executed (non-sensitive, for reporting).</summary>
    public string CommandLine { get; init; } = "";

    public bool Success => ExitCode == 0;
}
