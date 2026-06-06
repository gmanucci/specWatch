using SpecWatch.Core.Configuration;

namespace SpecWatch.Core.Tests.Configuration;

public class ManifestValidatorTests
{
    private readonly ManifestLoader _loader = new();
    private readonly ManifestValidator _validator = new();

    private ManifestValidationResult ValidateFixture(string fileName) =>
        _validator.Validate(_loader.LoadFromFile(Fixtures.Manifest(fileName)));

    [Fact]
    public void Validate_MinimalManifest_IsValid()
    {
        var result = ValidateFixture("minimal-valid.yml");
        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact]
    public void Validate_FullManifest_IsValid()
    {
        var result = ValidateFixture("full-valid.yml");
        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact]
    public void Validate_MissingSource_ReportsActionableError()
    {
        var result = ValidateFixture("invalid-missing-source.yml");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("missing 'source'"));
    }

    [Fact]
    public void Validate_UnsupportedGenerator_ReportsActionableError()
    {
        var result = ValidateFixture("invalid-generator.yml");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("client.generator") && e.Contains("not-a-real-generator"));
    }

    [Fact]
    public void Validate_WrongVersion_ReportsError()
    {
        var manifest = new SpecWatchManifest { Version = 2 };
        var result = _validator.Validate(manifest);
        Assert.Contains(result.Errors, e => e.Contains("version"));
    }

    [Fact]
    public void Validate_DuplicateNames_ReportsError()
    {
        var manifest = _loader.LoadFromString("""
            version: 1
            apis:
              - name: dup
                source: { type: file, path: a.json }
                snapshot: { path: openapi/a.json }
                client: { language: csharp, generator: kiota, output: out/a }
              - name: dup
                source: { type: file, path: b.json }
                snapshot: { path: openapi/b.json }
                client: { language: csharp, generator: kiota, output: out/b }
            """);

        var result = _validator.Validate(manifest);
        Assert.Contains(result.Errors, e => e.Contains("Duplicate API name 'dup'"));
    }

    [Fact]
    public void Validate_SnapshotPathCollision_ReportsError()
    {
        var manifest = _loader.LoadFromString("""
            version: 1
            apis:
              - name: a
                source: { type: file, path: a.json }
                snapshot: { path: openapi/shared.json }
                client: { language: csharp, generator: kiota, output: out/a }
              - name: b
                source: { type: file, path: b.json }
                snapshot: { path: openapi/shared.json }
                client: { language: csharp, generator: kiota, output: out/b }
            """);

        var result = _validator.Validate(manifest);
        Assert.Contains(result.Errors, e => e.Contains("snapshot.path") && e.Contains("collides"));
    }

    [Fact]
    public void Validate_OutputPathCollision_ReportsError()
    {
        var manifest = _loader.LoadFromString("""
            version: 1
            apis:
              - name: a
                source: { type: file, path: a.json }
                snapshot: { path: openapi/a.json }
                client: { language: csharp, generator: kiota, output: out/shared }
              - name: b
                source: { type: file, path: b.json }
                snapshot: { path: openapi/b.json }
                client: { language: csharp, generator: kiota, output: out/shared }
            """);

        var result = _validator.Validate(manifest);
        Assert.Contains(result.Errors, e => e.Contains("client.output") && e.Contains("collides"));
    }

    [Fact]
    public void Validate_BearerAuthMissingTokenVariable_ReportsError()
    {
        var manifest = _loader.LoadFromString("""
            version: 1
            apis:
              - name: a
                source:
                  type: url
                  url: https://example.com/openapi.json
                  auth:
                    type: bearer
                snapshot: { path: openapi/a.json }
                client: { language: csharp, generator: kiota, output: out/a }
            """);

        var result = _validator.Validate(manifest);
        Assert.Contains(result.Errors, e => e.Contains("tokenVariable"));
    }

    [Fact]
    public void Validate_UrlSourceWithInvalidUrl_ReportsError()
    {
        var manifest = _loader.LoadFromString("""
            version: 1
            apis:
              - name: a
                source: { type: url, url: not-a-url }
                snapshot: { path: openapi/a.json }
                client: { language: csharp, generator: kiota, output: out/a }
            """);

        var result = _validator.Validate(manifest);
        Assert.Contains(result.Errors, e => e.Contains("source.url"));
    }
}
