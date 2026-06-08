namespace SpecWatch.Core.Configuration;

/// <summary>
/// Runtime authentication expected by the generated client (see AGENTS.md, Section 7.5).
/// Only variable names are stored, never secret values.
/// </summary>
public sealed class RuntimeAuthDefinition
{
    /// <summary>
    /// Runtime auth type: <c>anonymous</c>, <c>api-key-header</c>, <c>bearer-static</c>,
    /// <c>oauth2-client-credentials</c>, or <c>basic</c>.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>Header name (<c>api-key-header</c>).</summary>
    public string? HeaderName { get; set; }

    /// <summary>Environment variable holding the header value (<c>api-key-header</c>).</summary>
    public string? ValueVariable { get; set; }

    /// <summary>Environment variable holding the static token (<c>bearer-static</c>).</summary>
    public string? TokenVariable { get; set; }

    /// <summary>Token endpoint URL (<c>oauth2-client-credentials</c>).</summary>
    public string? TokenUrl { get; set; }

    /// <summary>Environment variable holding the client id (<c>oauth2-client-credentials</c>).</summary>
    public string? ClientIdVariable { get; set; }

    /// <summary>Environment variable holding the client secret (<c>oauth2-client-credentials</c>).</summary>
    public string? ClientSecretVariable { get; set; }

    /// <summary>OAuth2 scopes (<c>oauth2-client-credentials</c>).</summary>
    public List<string> Scopes { get; set; } = [];

    /// <summary>Environment variable holding the username (<c>basic</c>).</summary>
    public string? UsernameVariable { get; set; }

    /// <summary>Environment variable holding the password (<c>basic</c>).</summary>
    public string? PasswordVariable { get; set; }
}
