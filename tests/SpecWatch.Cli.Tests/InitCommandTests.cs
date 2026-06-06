using SpecWatch.Cli;
using SpecWatch.Core;

namespace SpecWatch.Cli.Tests;

public class InitCommandTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _originalDir;

    public InitCommandTests()
    {
        _originalDir = Directory.GetCurrentDirectory();
        _tempDir = Path.Combine(Path.GetTempPath(), "specwatch-init-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        Directory.SetCurrentDirectory(_tempDir);
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDir);
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    private static (int code, string output, string error) RunCli(params string[] args)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var code = CliApplication.Run(args, output, error);
        return (code, output.ToString(), error.ToString());
    }

    [Fact]
    public void Init_AzureDevOps_ScaffoldsFiles()
    {
        var (code, output, _) = RunCli("init", "azure-devops");

        Assert.Equal(ExitCodes.Success, code);
        Assert.True(File.Exists(Path.Combine(_tempDir, "specwatch.yml")));
        Assert.True(File.Exists(Path.Combine(_tempDir, "azure-pipelines.yml")));
        Assert.Contains("Wrote specwatch.yml", output);
    }

    [Fact]
    public void Init_DoesNotOverwriteWithoutForce()
    {
        File.WriteAllText(Path.Combine(_tempDir, "specwatch.yml"), "existing");

        var (code, _, error) = RunCli("init", "azure-devops");

        Assert.Equal(ExitCodes.GeneralError, code);
        Assert.Contains("already exists", error);
        Assert.Equal("existing", File.ReadAllText(Path.Combine(_tempDir, "specwatch.yml")));
    }

    [Fact]
    public void Init_ForceOverwritesExistingFiles()
    {
        File.WriteAllText(Path.Combine(_tempDir, "specwatch.yml"), "existing");

        var (code, _, _) = RunCli("init", "azure-devops", "--force");

        Assert.Equal(ExitCodes.Success, code);
        Assert.Contains("version: 1", File.ReadAllText(Path.Combine(_tempDir, "specwatch.yml")));
    }

    [Fact]
    public void Init_UnknownTarget_ReturnsGeneralError()
    {
        var (code, _, error) = RunCli("init", "github-actions");

        Assert.Equal(ExitCodes.GeneralError, code);
        Assert.Contains("Unknown init target", error);
    }
}
