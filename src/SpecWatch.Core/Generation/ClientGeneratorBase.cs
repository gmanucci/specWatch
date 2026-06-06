using SpecWatch.Core.Configuration;
using SpecWatch.Core.Execution;

namespace SpecWatch.Core.Generation;

/// <summary>
/// Base class for command-line client generators. Resolves the snapshot input
/// path, delegates command construction to subclasses, runs the command via the
/// injected <see cref="ICommandRunner"/>, and maps the result.
/// </summary>
public abstract class ClientGeneratorBase : IClientGenerator
{
    private readonly ICommandRunner _commandRunner;
    private readonly string _baseDirectory;

    protected ClientGeneratorBase(ICommandRunner commandRunner, string? baseDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(commandRunner);
        _commandRunner = commandRunner;
        _baseDirectory = baseDirectory ?? Directory.GetCurrentDirectory();
    }

    public abstract string Name { get; }

    /// <summary>The executable name for this generator.</summary>
    protected abstract string FileName { get; }

    /// <summary>Builds the generator arguments for the given API and resolved input path.</summary>
    protected abstract IReadOnlyList<string> BuildArguments(ApiWatchDefinition api, string inputPath);

    public async Task<GeneratorResult> GenerateAsync(
        ApiWatchDefinition api,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(api);

        var snapshotPath = api.Snapshot?.Path;
        if (string.IsNullOrWhiteSpace(snapshotPath))
        {
            return Failure(api, "Cannot generate client: 'snapshot.path' is not set.");
        }

        if (string.IsNullOrWhiteSpace(api.Client?.Output))
        {
            return Failure(api, "Cannot generate client: 'client.output' is not set.");
        }

        var input = ResolvePath(snapshotPath);
        var arguments = BuildArguments(api, input);

        var result = await _commandRunner
            .RunAsync(FileName, arguments, _baseDirectory, cancellationToken)
            .ConfigureAwait(false);

        return new GeneratorResult
        {
            Success = result.Success,
            GeneratorName = Name,
            ApiName = api.Name ?? "<unnamed>",
            ChangedFiles = result.Success ? [api.Client!.Output!] : [],
            StandardOutput = result.StandardOutput,
            StandardError = result.StandardError,
            ExitCode = result.ExitCode,
        };
    }

    private string ResolvePath(string path) =>
        Path.IsPathRooted(path) ? path : Path.Combine(_baseDirectory, path);

    private GeneratorResult Failure(ApiWatchDefinition api, string message) => new()
    {
        Success = false,
        GeneratorName = Name,
        ApiName = api.Name ?? "<unnamed>",
        StandardError = message,
        ExitCode = 1,
    };
}
