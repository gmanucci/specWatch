using SpecWatch.Core.Execution;

namespace SpecWatch.AzureDevOps;

/// <summary>The outcome of ensuring a SpecWatch pull request exists.</summary>
public sealed class PullRequestResult
{
    /// <summary>A new pull request was created.</summary>
    public bool Created { get; init; }

    /// <summary>An active pull request already existed; nothing was created.</summary>
    public bool AlreadyExisted { get; init; }

    /// <summary>The pull request id, when known.</summary>
    public string? PullRequestId { get; init; }
}

/// <summary>
/// Creates a single pull request for all changed APIs via the Azure DevOps CLI
/// (<c>az repos pr</c>), matching the pipeline in AGENTS.md, Section 14.1, and
/// the initial PR strategy in Section 15.1. Process execution is delegated to an
/// injected <see cref="ICommandRunner"/> for testability.
/// </summary>
public sealed class AzureDevOpsPullRequestService
{
    private readonly ICommandRunner _commandRunner;
    private readonly string? _workingDirectory;

    public AzureDevOpsPullRequestService(ICommandRunner commandRunner, string? workingDirectory = null)
    {
        _commandRunner = commandRunner ?? throw new ArgumentNullException(nameof(commandRunner));
        _workingDirectory = workingDirectory;
    }

    /// <summary>
    /// Creates a pull request from the update branch to the target branch unless
    /// an active one already exists.
    /// </summary>
    public async Task<PullRequestResult> EnsurePullRequestAsync(
        AzureDevOpsOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Repository))
        {
            throw new InvalidOperationException("AzureDevOpsOptions.Repository is required to create a pull request.");
        }

        var existingId = await FindActivePullRequestIdAsync(options, cancellationToken).ConfigureAwait(false);
        if (existingId is not null)
        {
            return new PullRequestResult { AlreadyExisted = true, PullRequestId = existingId };
        }

        var createArgs = new List<string>
        {
            "repos", "pr", "create",
            "--repository", options.Repository,
            "--source-branch", options.UpdateBranch,
            "--target-branch", options.TargetBranch,
            "--title", options.Title,
            "--description", options.Description,
            "--delete-source-branch", options.DeleteSourceBranch ? "true" : "false",
        };

        var result = await RunAsync(createArgs, cancellationToken).ConfigureAwait(false);
        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"'az repos pr create' failed with exit code {result.ExitCode}.");
        }

        return new PullRequestResult { Created = true };
    }

    /// <summary>
    /// Returns the id of an active pull request for the update/target branch pair,
    /// or <c>null</c> when none exists.
    /// </summary>
    public async Task<string?> FindActivePullRequestIdAsync(
        AzureDevOpsOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var listArgs = new List<string>
        {
            "repos", "pr", "list",
            "--repository", options.Repository ?? "",
            "--source-branch", options.UpdateBranch,
            "--target-branch", options.TargetBranch,
            "--status", "active",
            "--query", "[0].pullRequestId",
            "--output", "tsv",
        };

        var result = await RunAsync(listArgs, cancellationToken).ConfigureAwait(false);
        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"'az repos pr list' failed with exit code {result.ExitCode}.");
        }

        var id = result.StandardOutput.Trim();
        return long.TryParse(id, out _) ? id : null;
    }

    private Task<CommandResult> RunAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken) =>
        _commandRunner.RunAsync("az", arguments, _workingDirectory, cancellationToken);
}
