using SpecWatch.AzureDevOps;
using SpecWatch.Core.Execution;
using SpecWatch.Core.Tests.Execution;

namespace SpecWatch.Core.Tests.AzureDevOps;

public class AzureDevOpsPullRequestServiceTests
{
    private static AzureDevOpsOptions Options() => new()
    {
        Repository = "Contoso.Api",
        TargetBranch = "main",
        UpdateBranch = "chore/specwatch/openapi-clients",
        Title = "chore: update generated OpenAPI clients",
        Description = "SpecWatch detected OpenAPI changes and regenerated API clients.",
        DeleteSourceBranch = true,
    };

    [Fact]
    public async Task EnsurePullRequestAsync_CreatesWhenNoneExists()
    {
        // FakeCommandRunner returns "ok" stdout which is not a PR id => none active.
        var runner = new FakeCommandRunner();
        var service = new AzureDevOpsPullRequestService(runner);

        var result = await service.EnsurePullRequestAsync(Options());

        Assert.True(result.Created);
        Assert.False(result.AlreadyExisted);
        // First az call lists, second creates.
        Assert.Equal("az", runner.Invocations[0].FileName);
        Assert.Contains("list", runner.Invocations[0].Arguments);
        Assert.Contains("create", runner.Invocations[1].Arguments);
        Assert.Contains("--delete-source-branch", runner.Invocations[1].Arguments);
    }

    [Fact]
    public async Task EnsurePullRequestAsync_SkipsWhenActivePrExists()
    {
        var runner = new ListReturningCommandRunner("4242");
        var service = new AzureDevOpsPullRequestService(runner);

        var result = await service.EnsurePullRequestAsync(Options());

        Assert.True(result.AlreadyExisted);
        Assert.Equal("4242", result.PullRequestId);
        Assert.False(result.Created);
        // Only the list call should have run; no create.
        Assert.Single(runner.Invocations);
    }

    [Fact]
    public async Task EnsurePullRequestAsync_RequiresRepository()
    {
        var options = Options();
        options.Repository = null;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => new AzureDevOpsPullRequestService(new FakeCommandRunner()).EnsurePullRequestAsync(options));
    }

    private sealed class ListReturningCommandRunner : ICommandRunner
    {
        private readonly string _listOutput;

        public ListReturningCommandRunner(string listOutput) => _listOutput = listOutput;

        public List<(string FileName, IReadOnlyList<string> Arguments, string? WorkingDirectory)> Invocations { get; } = [];

        public Task<CommandResult> RunAsync(
            string fileName,
            IReadOnlyList<string> arguments,
            string? workingDirectory = null,
            CancellationToken cancellationToken = default)
        {
            Invocations.Add((fileName, arguments, workingDirectory));
            var output = arguments.Contains("list") ? _listOutput : "";
            return Task.FromResult(new CommandResult { ExitCode = 0, StandardOutput = output });
        }
    }
}
