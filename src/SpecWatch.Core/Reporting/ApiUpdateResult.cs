namespace SpecWatch.Core.Reporting;

/// <summary>
/// Per-API result entry in a SpecWatch report (see AGENTS.md, Section 16).
/// Nullable success flags indicate "not attempted" (e.g., unchanged APIs).
/// </summary>
public sealed class ApiUpdateResult
{
    public required string Name { get; init; }

    public bool Changed { get; set; }

    public string? Source { get; init; }

    public string? SnapshotPath { get; init; }

    public string? Generator { get; init; }

    public string? Language { get; init; }

    public string? Output { get; init; }

    /// <summary>Null when generation was not attempted (e.g., unchanged or check mode).</summary>
    public bool? GenerationSucceeded { get; set; }

    /// <summary>Null when validation was not attempted.</summary>
    public bool? ValidationSucceeded { get; set; }

    /// <summary>Non-sensitive error message when processing this API failed.</summary>
    public string? Error { get; set; }
}
