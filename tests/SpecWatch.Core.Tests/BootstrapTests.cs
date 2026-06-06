using Xunit;

namespace SpecWatch.Core.Tests;

/// <summary>
/// Sprint 0 bootstrap tests. These confirm the solution, projects, and test
/// harness build and execute. Real Core behavior is covered from Sprint 1 onward.
/// </summary>
public class BootstrapTests
{
    [Fact]
    public void CoreAssembly_IsReferencedAndLoadable()
    {
        var assembly = typeof(SpecWatch.Core.AssemblyMarker).Assembly;
        Assert.Equal("SpecWatch.Core", assembly.GetName().Name);
    }
}
