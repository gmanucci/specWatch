using SpecWatch.Core.Execution;

namespace SpecWatch.AzureDevOps;

/// <summary>
/// Prepares an update branch and commits/pushes generated changes using git
/// (see AGENTS.md, Section 14.1). Process execution is delegated to an injected
/// <see cref="ICommandRunner"/> so the service can be tested without git.
/// </summary>
public sealed class AzureDevOpsGitService
{
    private readonly ICommandRunner _commandRunner;
    private readonly string? _workingDirectory;

    public AzureDevOpsGitService(ICommandRunner commandRunner, string? workingDirectory = null)
    {
        _commandRunner = commandRunner ?? throw new ArgumentNullException(nameof(commandRunner));
        _workingDirectory = workingDirectory;
    }

    /// <summary>Configures the git author identity used for commits.</summary>
    public async Task ConfigureIdentityAsync(AzureDevOpsOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        await RunAsync(["config", "user.email", options.CommitUserEmail], cancellationToken).ConfigureAwait(false);
        await RunAsync(["config", "user.name", options.CommitUserName], cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Fetches the target branch and checks out a fresh update branch from it.</summary>
    public async Task PrepareBranchAsync(AzureDevOpsOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        await RunAsync(["fetch", "origin", options.TargetBranch], cancellationToken).ConfigureAwait(false);
        await RunAsync(
            ["checkout", "-B", options.UpdateBranch, $"origin/{options.TargetBranch}"],
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Returns <c>true</c> if the working tree has uncommitted changes.</summary>
    public async Task<bool> HasChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await RunAsync(["status", "--porcelain"], cancellationToken).ConfigureAwait(false);
        return !string.IsNullOrWhiteSpace(result.StandardOutput);
    }

    /// <summary>Stages all changes and commits them with the configured message.</summary>
    public async Task CommitAllAsync(AzureDevOpsOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        await RunAsync(["add", "."], cancellationToken).ConfigureAwait(false);
        await RunAsync(["commit", "-m", options.CommitMessage], cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Pushes the update branch to origin using <c>--force-with-lease</c>.</summary>
    public async Task PushAsync(AzureDevOpsOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        await RunAsync(
            ["push", "origin", $"HEAD:refs/heads/{options.UpdateBranch}", "--force-with-lease"],
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<CommandResult> RunAsync(string[] arguments, CancellationToken cancellationToken)
    {
        var result = await _commandRunner.RunAsync("git", arguments, _workingDirectory, cancellationToken)
            .ConfigureAwait(false);
        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"git {arguments[0]} failed with exit code {result.ExitCode}.");
        }

        return result;
    }
}
