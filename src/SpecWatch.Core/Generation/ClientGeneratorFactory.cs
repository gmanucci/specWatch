using SpecWatch.Core.Execution;

namespace SpecWatch.Core.Generation;

/// <summary>Creates <see cref="IClientGenerator"/> instances by generator name.</summary>
public interface IClientGeneratorFactory
{
    /// <summary>Returns whether a generator with the given name is supported.</summary>
    bool IsSupported(string generatorName);

    /// <summary>Creates the generator for the given name.</summary>
    /// <exception cref="ArgumentException">Thrown when the generator is unknown.</exception>
    IClientGenerator Create(string generatorName);
}

/// <summary>
/// Default factory mapping <c>client.generator</c> values to concrete generators
/// (see AGENTS.md, Sections 6 and 12.3).
/// </summary>
public sealed class ClientGeneratorFactory : IClientGeneratorFactory
{
    private readonly ICommandRunner _commandRunner;
    private readonly string? _baseDirectory;

    public ClientGeneratorFactory(ICommandRunner commandRunner, string? baseDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(commandRunner);
        _commandRunner = commandRunner;
        _baseDirectory = baseDirectory;
    }

    public bool IsSupported(string generatorName) => Normalize(generatorName) is not null;

    public IClientGenerator Create(string generatorName)
    {
        return Normalize(generatorName) switch
        {
            "kiota" => new KiotaClientGenerator(_commandRunner, _baseDirectory),
            "nswag" => new NSwagClientGenerator(_commandRunner, _baseDirectory),
            "refitter" => new RefitterClientGenerator(_commandRunner, _baseDirectory),
            "openapi-generator" => new OpenApiGeneratorClientGenerator(_commandRunner, _baseDirectory),
            _ => throw new ArgumentException($"Unsupported generator '{generatorName}'.", nameof(generatorName)),
        };
    }

    private static string? Normalize(string? generatorName) =>
        generatorName?.Trim().ToLowerInvariant() switch
        {
            "kiota" => "kiota",
            "nswag" => "nswag",
            "refitter" => "refitter",
            "openapi-generator" => "openapi-generator",
            _ => null,
        };
}
