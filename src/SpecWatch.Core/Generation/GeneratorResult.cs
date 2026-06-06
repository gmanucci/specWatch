namespace SpecWatch.Core.Generation;

/// <summary>The outcome of running a client generator (see AGENTS.md, Section 12.2).</summary>
public sealed class GeneratorResult
{
    public bool Success { get; init; }

    public string GeneratorName { get; init; } = "";

    public string ApiName { get; init; } = "";

    public IReadOnlyList<string> ChangedFiles { get; init; } = [];

    public string? StandardOutput { get; init; }

    public string? StandardError { get; init; }

    public int ExitCode { get; init; }
}
