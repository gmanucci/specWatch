namespace SpecWatch.Core.Execution;

/// <summary>
/// Abstraction over running external processes (generators, validation
/// commands). Injectable so the pipeline can be tested with a fake runner
/// (see AGENTS.md, Sections 6 and 17.2).
/// </summary>
public interface ICommandRunner
{
    /// <summary>
    /// Runs <paramref name="fileName"/> with the given arguments. Arguments are
    /// passed individually (no shell), avoiding shell-injection.
    /// </summary>
    Task<CommandResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory = null,
        CancellationToken cancellationToken = default);
}
