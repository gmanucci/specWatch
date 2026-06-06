namespace SpecWatch.Core.Configuration;

/// <summary>Local pinned snapshot of an OpenAPI document (see AGENTS.md, Section 7.3).</summary>
public sealed class SnapshotDefinition
{
    /// <summary>Repository-relative path where the snapshot is stored.</summary>
    public string? Path { get; set; }
}
