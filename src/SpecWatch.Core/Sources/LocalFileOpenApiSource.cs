using SpecWatch.Core.Configuration;

namespace SpecWatch.Core.Sources;

/// <summary>
/// Reads an OpenAPI document from a local repository file
/// (see AGENTS.md, Section 4.1).
/// </summary>
public sealed class LocalFileOpenApiSource : IOpenApiSource
{
    private readonly string _path;
    private readonly string _baseDirectory;

    /// <summary>
    /// Creates a local file source.
    /// </summary>
    /// <param name="path">The spec file path, possibly relative to <paramref name="baseDirectory"/>.</param>
    /// <param name="baseDirectory">Directory used to resolve relative paths (defaults to the current directory).</param>
    public LocalFileOpenApiSource(string path, string? baseDirectory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = path;
        _baseDirectory = baseDirectory ?? Directory.GetCurrentDirectory();
    }

    public string Description => $"file:{_path}";

    public Task<OpenApiFetchResult> FetchAsync(CancellationToken cancellationToken = default)
    {
        var resolved = Path.IsPathRooted(_path) ? _path : Path.Combine(_baseDirectory, _path);

        if (!File.Exists(resolved))
        {
            throw new SourceFetchException($"OpenAPI spec file not found: '{_path}'.");
        }

        try
        {
            var bytes = File.ReadAllBytes(resolved);
            return Task.FromResult(new OpenApiFetchResult
            {
                Content = bytes,
                SourceDescription = Description,
            });
        }
        catch (IOException ex)
        {
            throw new SourceFetchException($"Failed to read OpenAPI spec file '{_path}': {ex.Message}", ex);
        }
    }
}
