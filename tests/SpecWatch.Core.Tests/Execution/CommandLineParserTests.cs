using SpecWatch.Core.Execution;

namespace SpecWatch.Core.Tests.Execution;

public class CommandLineParserTests
{
    [Fact]
    public void Parse_SplitsFileNameAndArguments()
    {
        var (fileName, args) = CommandLineParser.Parse("dotnet build");
        Assert.Equal("dotnet", fileName);
        Assert.Equal(["build"], args);
    }

    [Fact]
    public void Parse_RespectsQuotedSegments()
    {
        var (fileName, args) = CommandLineParser.Parse("dotnet test \"/p:Path=a b\"");
        Assert.Equal("dotnet", fileName);
        Assert.Equal(["test", "/p:Path=a b"], args);
    }

    [Fact]
    public void Parse_EmptyString_Throws()
    {
        Assert.Throws<ArgumentException>(() => CommandLineParser.Parse("   "));
    }
}
