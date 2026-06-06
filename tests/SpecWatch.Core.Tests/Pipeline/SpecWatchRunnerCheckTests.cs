using SpecWatch.Core.ChangeDetection;
using SpecWatch.Core.Configuration;
using SpecWatch.Core.Pipeline;
using SpecWatch.Core.Sources;

namespace SpecWatch.Core.Tests.Pipeline;

public class SpecWatchRunnerCheckTests : IDisposable
{
    private readonly string _tempDir;
    private readonly HttpClient _httpClient = new();

    public SpecWatchRunnerCheckTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "specwatch-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    private SpecWatchRunner CreateRunner() => new(
        new OpenApiSourceFactory(_httpClient, baseDirectory: _tempDir),
        new HashSpecChangeDetector());

    private static ApiWatchDefinition FileApi(string name, string sourcePath, string snapshotPath) => new()
    {
        Name = name,
        Source = new SourceDefinition { Type = "file", Path = sourcePath },
        Snapshot = new SnapshotDefinition { Path = snapshotPath },
        Client = new GenerationDefinition { Language = "csharp", Generator = "kiota", Output = "out/" + name },
    };

    [Fact]
    public async Task CheckAsync_NoSnapshot_ReportsChanged()
    {
        var specPath = Path.Combine(_tempDir, "spec.json");
        File.WriteAllText(specPath, "{\"openapi\":\"3.0.0\"}");

        var manifest = new SpecWatchManifest
        {
            Version = 1,
            Apis = [FileApi("payments", "spec.json", "snapshots/payments.json")],
        };

        var result = await CreateRunner().CheckAsync(manifest, new SpecWatchRunOptions { BaseDirectory = _tempDir });

        Assert.True(result.HasChanges);
        Assert.False(result.HasFetchError);
        Assert.True(result.Report.Apis.Single().Changed);
        Assert.Equal(1, result.Report.Summary.ChangedApis);
    }

    [Fact]
    public async Task CheckAsync_MatchingSnapshot_ReportsUnchanged()
    {
        var content = "{\"openapi\":\"3.0.0\"}";
        File.WriteAllText(Path.Combine(_tempDir, "spec.json"), content);
        Directory.CreateDirectory(Path.Combine(_tempDir, "snapshots"));
        File.WriteAllText(Path.Combine(_tempDir, "snapshots", "payments.json"), content);

        var manifest = new SpecWatchManifest
        {
            Version = 1,
            Apis = [FileApi("payments", "spec.json", "snapshots/payments.json")],
        };

        var result = await CreateRunner().CheckAsync(manifest, new SpecWatchRunOptions { BaseDirectory = _tempDir });

        Assert.False(result.HasChanges);
        Assert.False(result.Report.Apis.Single().Changed);
    }

    [Fact]
    public async Task CheckAsync_MissingSource_ReportsFetchError()
    {
        var manifest = new SpecWatchManifest
        {
            Version = 1,
            Apis = [FileApi("payments", "nonexistent.json", "snapshots/payments.json")],
        };

        var result = await CreateRunner().CheckAsync(manifest, new SpecWatchRunOptions { BaseDirectory = _tempDir });

        Assert.True(result.HasFetchError);
        Assert.NotNull(result.Report.Apis.Single().Error);
    }

    [Fact]
    public async Task CheckAsync_DisabledApi_IsSkipped()
    {
        File.WriteAllText(Path.Combine(_tempDir, "spec.json"), "{}");
        var api = FileApi("payments", "spec.json", "snapshots/payments.json");
        api.Enabled = false;

        var manifest = new SpecWatchManifest { Version = 1, Apis = [api] };
        var result = await CreateRunner().CheckAsync(manifest, new SpecWatchRunOptions { BaseDirectory = _tempDir });

        Assert.Empty(result.Report.Apis);
    }
}
