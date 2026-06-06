namespace SpecWatch.Core.Configuration;

/// <summary>
/// A single configured OpenAPI dependency to watch (see AGENTS.md, Section 7.2).
/// </summary>
public sealed class ApiWatchDefinition
{
    /// <summary>Unique, human-readable name for the API watch.</summary>
    public string? Name { get; set; }

    /// <summary>Whether this watch is processed. Defaults to true.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Where the OpenAPI document is fetched from.</summary>
    public SourceDefinition? Source { get; set; }

    /// <summary>Where the local pinned snapshot of the spec is stored.</summary>
    public SnapshotDefinition? Snapshot { get; set; }

    /// <summary>How the client is generated.</summary>
    public GenerationDefinition? Client { get; set; }

    /// <summary>Runtime authentication expected by the generated client.</summary>
    public RuntimeAuthDefinition? RuntimeAuth { get; set; }

    /// <summary>Validation commands to run after generation.</summary>
    public ValidationDefinition? Validation { get; set; }
}
