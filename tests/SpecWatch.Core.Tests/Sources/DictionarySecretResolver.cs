using SpecWatch.Core.Sources;

namespace SpecWatch.Core.Tests.Sources;

/// <summary>An in-memory <see cref="ISecretResolver"/> for tests.</summary>
internal sealed class DictionarySecretResolver : ISecretResolver
{
    private readonly Dictionary<string, string> _secrets;

    public DictionarySecretResolver(Dictionary<string, string>? secrets = null)
    {
        _secrets = secrets ?? new Dictionary<string, string>(StringComparer.Ordinal);
    }

    public string? GetSecret(string variableName) =>
        _secrets.TryGetValue(variableName, out var value) ? value : null;
}
