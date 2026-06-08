using SpecWatch.Core;
using Xunit;

namespace SpecWatch.Core.Tests;

/// <summary>
/// Bootstrap tests confirming the Core assembly is referenced and loadable.
/// </summary>
public class BootstrapTests
{
    [Fact]
    public void CoreAssembly_IsReferencedAndLoadable()
    {
        var assembly = typeof(ExitCodes).Assembly;
        Assert.Equal("SpecWatch.Core", assembly.GetName().Name);
    }
}
