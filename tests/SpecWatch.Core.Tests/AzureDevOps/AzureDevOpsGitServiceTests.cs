using SpecWatch.AzureDevOps;
using SpecWatch.Core.Execution;
using SpecWatch.Core.Tests.Execution;

namespace SpecWatch.Core.Tests.AzureDevOps;

public class AzureDevOpsGitServiceTests
{
    private static AzureDevOpsOptions Options() => new()
    {
        TargetBranch = "main",
        UpdateBranch = "chore/specwatch/openapi-clients",
        CommitUserName = "SpecWatch Bot",
        CommitUserEmail = "specwatch-bot@local",
        CommitMessage = "chore: update generated OpenAPI clients",
    };

    [Fact]
    public async Task PrepareBranchAsync_FetchesAndChecksOutUpdateBranch()
    {
        var runner = new FakeCommandRunner();
        var service = new AzureDevOpsGitService(runner);

        await service.PrepareBranchAsync(Options());

        Assert.Equal(2, runner.Invocations.Count);
        Assert.All(runner.Invocations, i => Assert.Equal("git", i.FileName));
        Assert.Equal(["fetch", "origin", "main"], runner.Invocations[0].Arguments);
        Assert.Equal(["checkout", "-B", "chore/specwatch/openapi-clients", "origin/main"], runner.Invocations[1].Arguments);
    }

    [Fact]
    public async Task HasChangesAsync_TrueWhenStatusHasOutput()
    {
        var runner = new FakeCommandRunner(); // StandardOutput = "ok"
        Assert.True(await new AzureDevOpsGitService(runner).HasChangesAsync());
    }

    [Fact]
    public async Task HasChangesAsync_FalseWhenStatusEmpty()
    {
        var runner = new EmptyOutputCommandRunner();
        Assert.False(await new AzureDevOpsGitService(runner).HasChangesAsync());
    }

    [Fact]
    public async Task CommitAllAsync_StagesAndCommits()
    {
        var runner = new FakeCommandRunner();
        await new AzureDevOpsGitService(runner).CommitAllAsync(Options());

        Assert.Equal(["add", "."], runner.Invocations[0].Arguments);
        Assert.Equal(["commit", "-m", "chore: update generated OpenAPI clients"], runner.Invocations[1].Arguments);
    }

    [Fact]
    public async Task PushAsync_UsesForceWithLease()
    {
        var runner = new FakeCommandRunner();
        await new AzureDevOpsGitService(runner).PushAsync(Options());

        Assert.Equal(
            ["push", "origin", "HEAD:refs/heads/chore/specwatch/openapi-clients", "--force-with-lease"],
            runner.Invocations.Single().Arguments);
    }

    [Fact]
    public async Task FailedGitCommand_Throws()
    {
        var runner = new FakeCommandRunner();
        runner.ExitCodeByFileName["git"] = 1;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => new AzureDevOpsGitService(runner).PushAsync(Options()));
    }

    private sealed class EmptyOutputCommandRunner : ICommandRunner
    {
        public Task<CommandResult> RunAsync(
            string fileName,
            IReadOnlyList<string> arguments,
            string? workingDirectory = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new CommandResult { ExitCode = 0, StandardOutput = "" });
    }
}
