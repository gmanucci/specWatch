namespace SpecWatch.Core.Configuration;

/// <summary>
/// Describes where an OpenAPI document is fetched from (see AGENTS.md, Section 7).
/// </summary>
public sealed class SourceDefinition
{
    /// <summary>Source type: <c>url</c> or <c>file</c>.</summary>
    public string? Type { get; set; }

    /// <summary>HTTP/HTTPS URL of the spec (used when <see cref="Type"/> is <c>url</c>).</summary>
    public string? Url { get; set; }

    /// <summary>Local file path of the spec (used when <see cref="Type"/> is <c>file</c>).</summary>
    public string? Path { get; set; }

    /// <summary>Optional authentication used only to fetch the spec.</summary>
    public SourceAuthDefinition? Auth { get; set; }
}
