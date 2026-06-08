using SpecWatch.Core.Configuration;

namespace SpecWatch.Core.Generation;

/// <summary>
/// Generates a client for a single API watch (see AGENTS.md, Section 12.1).
/// The OpenAPI input is the API's snapshot; the destination is the API's
/// <c>client.output</c>.
/// </summary>
public interface IClientGenerator
{
    /// <summary>The generator name (e.g., <c>kiota</c>).</summary>
    string Name { get; }

    /// <summary>Runs generation for the given API watch.</summary>
    Task<GeneratorResult> GenerateAsync(ApiWatchDefinition api, CancellationToken cancellationToken = default);
}
