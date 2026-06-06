using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SpecWatch.Core.Configuration;

/// <summary>
/// Loads and deserializes <c>specwatch.yml</c> manifests (see AGENTS.md, Section 8).
/// </summary>
public sealed class ManifestLoader
{
    /// <summary>Manifest filenames recognized by SpecWatch, in priority order.</summary>
    public static readonly IReadOnlyList<string> SupportedFileNames =
    [
        "specwatch.yml",
        "specwatch.yaml",
        ".specwatch.yml",
        ".specwatch.yaml",
    ];

    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>
    /// Loads a manifest from the given file path.
    /// </summary>
    /// <exception cref="ManifestLoadException">
    /// Thrown when the file is missing or contains invalid YAML.
    /// </exception>
    public SpecWatchManifest LoadFromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ManifestLoadException("Manifest path must be provided.");
        }

        if (!File.Exists(path))
        {
            throw new ManifestLoadException($"Manifest file not found: '{path}'.");
        }

        string content;
        try
        {
            content = File.ReadAllText(path);
        }
        catch (IOException ex)
        {
            throw new ManifestLoadException($"Failed to read manifest file '{path}': {ex.Message}", ex);
        }

        return LoadFromString(content, path);
    }

    /// <summary>
    /// Deserializes a manifest from raw YAML content.
    /// </summary>
    /// <param name="content">The YAML document text.</param>
    /// <param name="sourceName">A label used in error messages (e.g., the file path).</param>
    /// <exception cref="ManifestLoadException">Thrown when the YAML is invalid.</exception>
    public SpecWatchManifest LoadFromString(string content, string sourceName = "<manifest>")
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ManifestLoadException($"Manifest '{sourceName}' is empty.");
        }

        try
        {
            var manifest = _deserializer.Deserialize<SpecWatchManifest>(content);
            if (manifest is null)
            {
                throw new ManifestLoadException($"Manifest '{sourceName}' did not contain a document.");
            }

            return manifest;
        }
        catch (YamlException ex)
        {
            throw new ManifestLoadException(
                $"Failed to parse manifest '{sourceName}' as YAML: {ex.Message}", ex);
        }
    }
}
