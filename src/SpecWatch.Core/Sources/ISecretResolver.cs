namespace SpecWatch.Core.Sources;

/// <summary>
/// Resolves secret values (tokens, passwords) by environment-variable name.
/// SpecWatch never stores secret values; it only references variable names
/// (see AGENTS.md, Sections 5.7 and 19).
/// </summary>
public interface ISecretResolver
{
    /// <summary>Returns the secret value for the given variable name, or null if unset.</summary>
    string? GetSecret(string variableName);
}

/// <summary>Resolves secrets from process environment variables.</summary>
public sealed class EnvironmentSecretResolver : ISecretResolver
{
    public string? GetSecret(string variableName)
    {
        if (string.IsNullOrWhiteSpace(variableName))
        {
            return null;
        }

        var value = Environment.GetEnvironmentVariable(variableName);
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
