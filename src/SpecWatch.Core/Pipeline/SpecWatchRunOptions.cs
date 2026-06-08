namespace SpecWatch.Core.Pipeline;

/// <summary>Options controlling a SpecWatch run (see AGENTS.md, Section 9).</summary>
public sealed class SpecWatchRunOptions
{
    /// <summary>Path to the manifest file.</summary>
    public string ManifestPath { get; init; } = "specwatch.yml";

    /// <summary>
    /// Base directory against which relative manifest paths (sources, snapshots,
    /// outputs) are resolved. Defaults to the manifest's directory.
    /// </summary>
    public string? BaseDirectory { get; init; }

    /// <summary>Path where the JSON report is written (used by <c>update</c>).</summary>
    public string ReportPath { get; init; } = "specwatch-report.json";
}
