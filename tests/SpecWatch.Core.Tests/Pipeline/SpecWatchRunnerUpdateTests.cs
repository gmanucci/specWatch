using SpecWatch.Core.ChangeDetection;
using SpecWatch.Core.Configuration;
using SpecWatch.Core.Generation;
using SpecWatch.Core.Pipeline;
using SpecWatch.Core.Sources;
using SpecWatch.Core.Tests.Execution;

namespace SpecWatch.Core.Tests.Pipeline;

public class SpecWatchRunnerUpdateTests : IDisposable
{
    private readonly string _tempDir;
    private readonly HttpClient _httpClient = new();

    public SpecWatchRunnerUpdateTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "specwatch-update-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    private SpecWatchRunner CreateRunner(FakeCommandRunner runner) => new(
        new OpenApiSourceFactory(_httpClient, baseDirectory: _tempDir),
        new HashSpecChangeDetector(),
        new ClientGeneratorFactory(runner, _tempDir),
        runner);

    private ApiWatchDefinition FileApi(string name, string[]? validationCommands = null) => new()
    {
        Name = name,
        Source = new SourceDefinition { Type = "file", Path = "spec.json" },
        Snapshot = new SnapshotDefinition { Path = $"openapi/{name}.json" },
        Client = new GenerationDefinition
        {
            Language = "csharp",
            Generator = "kiota",
            Output = $"src/Clients/{name}",
        },
        Validation = validationCommands is null ? null : new ValidationDefinition { Commands = [.. validationCommands] },
    };

    private SpecWatchManifest Manifest(ApiWatchDefinition api) => new() { Version = 1, Apis = [api] };

    private SpecWatchRunOptions Options() => new()
    {
        ManifestPath = "specwatch.yml",
        BaseDirectory = _tempDir,
        ReportPath = "specwatch-report.json",
    };

    [Fact]
    public async Task UpdateAsync_ChangedSpec_WritesSnapshotAndGenerates()
    {
        File.WriteAllText(Path.Combine(_tempDir, "spec.json"), "{\"openapi\":\"3.0.0\"}");
        var runner = new FakeCommandRunner();

        var result = await CreateRunner(runner).UpdateAsync(Manifest(FileApi("payments")), Options());

        Assert.True(result.HasChanges);
        Assert.False(result.HasGenerationError);

        // Snapshot was written.
        Assert.True(File.Exists(Path.Combine(_tempDir, "openapi", "payments.json")));
        // Generator was invoked.
        Assert.Single(runner.Invocations);
        Assert.Equal("kiota", runner.Invocations[0].FileName);
        // Report was written.
        Assert.True(File.Exists(Path.Combine(_tempDir, "specwatch-report.json")));

        var api = result.Report.Apis.Single();
        Assert.True(api.Changed);
        Assert.True(api.GenerationSucceeded);
    }

    [Fact]
    public async Task UpdateAsync_UnchangedSpec_DoesNotGenerate()
    {
        var content = "{\"openapi\":\"3.0.0\"}";
        File.WriteAllText(Path.Combine(_tempDir, "spec.json"), content);
        Directory.CreateDirectory(Path.Combine(_tempDir, "openapi"));
        File.WriteAllText(Path.Combine(_tempDir, "openapi", "payments.json"), content);
        var runner = new FakeCommandRunner();

        var result = await CreateRunner(runner).UpdateAsync(Manifest(FileApi("payments")), Options());

        Assert.False(result.HasChanges);
        Assert.Empty(runner.Invocations);
        Assert.Null(result.Report.Apis.Single().GenerationSucceeded);
    }

    [Fact]
    public async Task UpdateAsync_RunsValidationCommands_OnSuccess()
    {
        File.WriteAllText(Path.Combine(_tempDir, "spec.json"), "{\"v\":1}");
        var runner = new FakeCommandRunner();

        var result = await CreateRunner(runner)
            .UpdateAsync(Manifest(FileApi("payments", ["dotnet build", "dotnet test"])), Options());

        Assert.True(result.Report.Apis.Single().ValidationSucceeded);
        // kiota + 2 dotnet invocations
        Assert.Equal(3, runner.Invocations.Count);
        Assert.Contains(runner.Invocations, i => i.FileName == "dotnet");
    }

    [Fact]
    public async Task UpdateAsync_GenerationFailure_SetsGenerationError()
    {
        File.WriteAllText(Path.Combine(_tempDir, "spec.json"), "{\"v\":1}");
        var runner = new FakeCommandRunner();
        runner.ExitCodeByFileName["kiota"] = 1;

        var result = await CreateRunner(runner).UpdateAsync(Manifest(FileApi("payments")), Options());

        Assert.True(result.HasGenerationError);
        Assert.False(result.Report.Apis.Single().GenerationSucceeded);
    }

    [Fact]
    public async Task UpdateAsync_ValidationFailure_SetsValidationError()
    {
        File.WriteAllText(Path.Combine(_tempDir, "spec.json"), "{\"v\":1}");
        var runner = new FakeCommandRunner();
        runner.ExitCodeByFileName["dotnet"] = 1;

        var result = await CreateRunner(runner)
            .UpdateAsync(Manifest(FileApi("payments", ["dotnet test"])), Options());

        Assert.True(result.HasValidationError);
        Assert.False(result.Report.Apis.Single().ValidationSucceeded);
    }
}
