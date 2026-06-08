using Xunit;

namespace SpecWatch.Cli.Tests;

/// <summary>
/// Sprint 0 bootstrap tests. These confirm the CLI project builds and is
/// referenced by the test project. CLI command behavior is covered in later
/// sprints (see AGENTS.md, Section 9).
/// </summary>
public class BootstrapTests
{
    [Fact]
    public void CliAssembly_IsReferencedAndLoadable()
    {
        var assembly = typeof(Program).Assembly;
        Assert.Equal("specwatch", assembly.GetName().Name);
    }
}
