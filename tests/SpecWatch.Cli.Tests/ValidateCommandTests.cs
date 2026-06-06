using SpecWatch.Cli;
using SpecWatch.Core;

namespace SpecWatch.Cli.Tests;

public class ValidateCommandTests
{
    private static string FixtureManifest(string name) =>
        Path.Combine(AppContext.BaseDirectory, "fixtures", "manifests", name);

    private static (int code, string output, string error) RunCli(params string[] args)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var code = CliApplication.Run(args, output, error);
        return (code, output.ToString(), error.ToString());
    }

    [Fact]
    public void Validate_ValidManifest_ReturnsSuccess()
    {
        var (code, output, _) = RunCli("validate", "--manifest", FixtureManifest("minimal-valid.yml"));

        Assert.Equal(ExitCodes.Success, code);
        Assert.Contains("is valid", output);
    }

    [Fact]
    public void Validate_InvalidManifest_ReturnsManifestValidationFailed()
    {
        var (code, _, error) = RunCli("validate", "--manifest", FixtureManifest("invalid-generator.yml"));

        Assert.Equal(ExitCodes.ManifestValidationFailed, code);
        Assert.Contains("validation failed", error);
    }

    [Fact]
    public void Validate_MissingManifest_ReturnsManifestValidationFailed()
    {
        var (code, _, error) = RunCli("validate", "--manifest", "no-such-file.yml");

        Assert.Equal(ExitCodes.ManifestValidationFailed, code);
        Assert.Contains("not found", error);
    }

    [Fact]
    public void Version_ReturnsSuccessAndPrintsVersion()
    {
        var (code, output, _) = RunCli("--version");

        Assert.Equal(ExitCodes.Success, code);
        Assert.False(string.IsNullOrWhiteSpace(output));
    }

    [Fact]
    public void UnknownCommand_ReturnsGeneralError()
    {
        var (code, _, error) = RunCli("frobnicate");

        Assert.Equal(ExitCodes.GeneralError, code);
        Assert.Contains("Unknown command", error);
    }
}
