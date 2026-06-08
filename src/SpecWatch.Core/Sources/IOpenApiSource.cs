namespace SpecWatch.Core.Sources;

/// <summary>The result of fetching an OpenAPI document from a source.</summary>
public sealed class OpenApiFetchResult
{
    public required byte[] Content { get; init; }

    /// <summary>A non-sensitive description of where the spec was fetched from.</summary>
    public required string SourceDescription { get; init; }
}

/// <summary>
/// Abstraction over a location from which an OpenAPI document can be fetched
/// (see AGENTS.md, Section 7). Implementations must treat fetched content as
/// untrusted input (Section 19).
/// </summary>
public interface IOpenApiSource
{
    /// <summary>A non-sensitive, human-readable description of the source.</summary>
    string Description { get; }

    /// <summary>Fetches the latest OpenAPI document bytes.</summary>
    /// <exception cref="SourceFetchException">Thrown when the document cannot be fetched.</exception>
    Task<OpenApiFetchResult> FetchAsync(CancellationToken cancellationToken = default);
}
