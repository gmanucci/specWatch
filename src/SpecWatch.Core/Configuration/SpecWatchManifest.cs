namespace SpecWatch.Core.Configuration;

/// <summary>
/// Root manifest model describing the SpecWatch configuration, parsed from
/// <c>specwatch.yml</c> (see AGENTS.md, Section 8).
/// </summary>
public sealed class SpecWatchManifest
{
    /// <summary>Manifest schema version. Currently only version 1 is supported.</summary>
    public int Version { get; set; }

    /// <summary>Optional global settings shared across all API watches.</summary>
    public SettingsDefinition? Settings { get; set; }

    /// <summary>The list of configured API watches.</summary>
    public List<ApiWatchDefinition> Apis { get; set; } = [];
}
