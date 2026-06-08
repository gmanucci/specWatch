namespace SpecWatch.Core.Tests;

/// <summary>Helpers for locating test fixture files copied to the test output.</summary>
internal static class Fixtures
{
    private static readonly string Root =
        Path.Combine(AppContext.BaseDirectory, "fixtures");

    public static string Manifest(string fileName) =>
        Path.Combine(Root, "manifests", fileName);

    public static string Spec(string fileName) =>
        Path.Combine(Root, "specs", fileName);
}
