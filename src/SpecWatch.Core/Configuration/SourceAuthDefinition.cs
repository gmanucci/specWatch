namespace SpecWatch.Core.Configuration;

/// <summary>
/// Source authentication used only to fetch the OpenAPI document
/// (see AGENTS.md, Section 7.4). Secret values are never stored; only the names
/// of environment variables that hold them.
/// </summary>
public sealed class SourceAuthDefinition
{
    /// <summary>Auth type: <c>anonymous</c>, <c>bearer</c>, <c>header</c>, or <c>basic</c>.</summary>
    public string? Type { get; set; }

    /// <summary>Environment variable holding the bearer token (<c>bearer</c>).</summary>
    public string? TokenVariable { get; set; }

    /// <summary>Header name (<c>header</c>).</summary>
    public string? Name { get; set; }

    /// <summary>Environment variable holding the header value (<c>header</c>).</summary>
    public string? ValueVariable { get; set; }

    /// <summary>Environment variable holding the username (<c>basic</c>).</summary>
    public string? UsernameVariable { get; set; }

    /// <summary>Environment variable holding the password (<c>basic</c>).</summary>
    public string? PasswordVariable { get; set; }
}
