using System.Text;
using SpecWatch.Core.ChangeDetection;

namespace SpecWatch.Core.Tests.ChangeDetection;

public class HashSpecChangeDetectorTests : IDisposable
{
    private readonly HashSpecChangeDetector _detector = new();
    private readonly string _tempDir;

    public HashSpecChangeDetectorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "specwatch-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public void Detect_NoSnapshot_IsChanged()
    {
        var snapshot = Path.Combine(_tempDir, "missing.json");
        var result = _detector.Detect(Encoding.UTF8.GetBytes("{}"), snapshot);

        Assert.True(result.Changed);
        Assert.False(result.SnapshotExisted);
        Assert.Null(result.PreviousHash);
        Assert.NotEmpty(result.LatestHash);
    }

    [Fact]
    public void Detect_IdenticalSnapshot_IsUnchanged()
    {
        var snapshot = Path.Combine(_tempDir, "spec.json");
        var content = Encoding.UTF8.GetBytes("{\"a\":1}");
        File.WriteAllBytes(snapshot, content);

        var result = _detector.Detect(content, snapshot);

        Assert.False(result.Changed);
        Assert.True(result.SnapshotExisted);
        Assert.Equal(result.PreviousHash, result.LatestHash);
    }

    [Fact]
    public void Detect_DifferentContent_IsChanged()
    {
        var snapshot = Path.Combine(_tempDir, "spec.json");
        File.WriteAllBytes(snapshot, Encoding.UTF8.GetBytes("{\"a\":1}"));

        var result = _detector.Detect(Encoding.UTF8.GetBytes("{\"a\":2}"), snapshot);

        Assert.True(result.Changed);
        Assert.NotEqual(result.PreviousHash, result.LatestHash);
    }

    [Fact]
    public void ComputeHash_IsStableAndLowercaseHex()
    {
        var hash = HashSpecChangeDetector.ComputeHash(Encoding.UTF8.GetBytes("hello"));
        Assert.Equal("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824", hash);
    }
}
