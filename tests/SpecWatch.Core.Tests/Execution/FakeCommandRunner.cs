using SpecWatch.Core.Execution;

namespace SpecWatch.Core.Tests.Execution;

/// <summary>
/// A fake <see cref="ICommandRunner"/> that records invocations and returns
/// configured results, used to test generation and the update pipeline without
/// invoking real external tools (see AGENTS.md, Section 17.2).
/// </summary>
internal sealed class FakeCommandRunner : ICommandRunner
{
    private readonly int _exitCode;

    public FakeCommandRunner(int exitCode = 0)
    {
        _exitCode = exitCode;
    }

    public List<(string FileName, IReadOnlyList<string> Arguments, string? WorkingDirectory)> Invocations { get; } = [];

    /// <summary>Optional per-file-name exit code overrides (e.g., make "dotnet" fail).</summary>
    public Dictionary<string, int> ExitCodeByFileName { get; } = new(StringComparer.Ordinal);

    public Task<CommandResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory = null,
        CancellationToken cancellationToken = default)
    {
        Invocations.Add((fileName, arguments, workingDirectory));
        var code = ExitCodeByFileName.TryGetValue(fileName, out var c) ? c : _exitCode;
        return Task.FromResult(new CommandResult
        {
            ExitCode = code,
            StandardOutput = "ok",
            StandardError = code == 0 ? "" : "boom",
            CommandLine = $"{fileName} {string.Join(' ', arguments)}",
        });
    }
}
