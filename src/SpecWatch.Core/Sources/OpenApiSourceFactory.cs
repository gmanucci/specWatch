using SpecWatch.Core.Configuration;

namespace SpecWatch.Core.Sources;

/// <summary>Creates <see cref="IOpenApiSource"/> instances from manifest source definitions.</summary>
public interface IOpenApiSourceFactory
{
    IOpenApiSource Create(SourceDefinition source);
}

/// <summary>
/// Default factory that maps <c>source.type</c> to a concrete source
/// (see AGENTS.md, Section 6 and 7).
/// </summary>
public sealed class OpenApiSourceFactory : IOpenApiSourceFactory
{
    private readonly HttpClient _httpClient;
    private readonly ISecretResolver _secretResolver;
    private readonly string _baseDirectory;

    public OpenApiSourceFactory(
        HttpClient httpClient,
        ISecretResolver? secretResolver = null,
        string? baseDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        _secretResolver = secretResolver ?? new EnvironmentSecretResolver();
        _baseDirectory = baseDirectory ?? Directory.GetCurrentDirectory();
    }

    public IOpenApiSource Create(SourceDefinition source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var type = source.Type ?? string.Empty;
        switch (type.ToLowerInvariant())
        {
            case "file":
                if (string.IsNullOrWhiteSpace(source.Path))
                {
                    throw new SourceFetchException("File source is missing 'source.path'.");
                }

                return new LocalFileOpenApiSource(source.Path, _baseDirectory);

            case "url":
                if (string.IsNullOrWhiteSpace(source.Url))
                {
                    throw new SourceFetchException("URL source is missing 'source.url'.");
                }

                return new HttpOpenApiSource(_httpClient, source.Url, source.Auth, _secretResolver);

            default:
                throw new SourceFetchException($"Unsupported source type '{source.Type}'.");
        }
    }
}
