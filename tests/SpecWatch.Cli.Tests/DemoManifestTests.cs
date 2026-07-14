using SpecWatch.Cli;
using SpecWatch.Core;

namespace SpecWatch.Cli.Tests;

public class DemoManifestTests
{
    [Fact]
    public void Check_DemoManifest_UsesManifestRelativeSourcePath()
    {
        var manifestPath = Path.Combine(RepositoryRoot(), ".github", "specwatch-demo.yml");
        var output = new StringWriter();
        var error = new StringWriter();

        var code = CliApplication.Run(["check", "--manifest", manifestPath], output, error);

        Assert.Equal(ExitCodes.ChangesDetected, code);
        Assert.DoesNotContain("OpenAPI spec file not found", error.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "SpecWatch.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root not found from test base directory.");
    }
}
