using SpecWatch.Core.Configuration;

namespace SpecWatch.Core.Tests.Configuration;

public class ManifestLoaderTests
{
    private readonly ManifestLoader _loader = new();

    [Fact]
    public void LoadFromFile_MinimalManifest_ParsesCoreFields()
    {
        var manifest = _loader.LoadFromFile(Fixtures.Manifest("minimal-valid.yml"));

        Assert.Equal(1, manifest.Version);
        var api = Assert.Single(manifest.Apis);
        Assert.Equal("payments", api.Name);
        Assert.True(api.Enabled);
        Assert.Equal("url", api.Source!.Type);
        Assert.Equal("https://payments.example.com/openapi.json", api.Source.Url);
        Assert.Equal("openapi/payments.openapi.json", api.Snapshot!.Path);
        Assert.Equal("csharp", api.Client!.Language);
        Assert.Equal("kiota", api.Client.Generator);
        Assert.Equal("PaymentsClient", api.Client.ClassName);
    }

    [Fact]
    public void LoadFromFile_FullManifest_ParsesSettingsAndAllApis()
    {
        var manifest = _loader.LoadFromFile(Fixtures.Manifest("full-valid.yml"));

        Assert.Equal("hash", manifest.Settings!.ChangeDetection!.Mode);
        Assert.Equal("single", manifest.Settings.PullRequest!.Mode);
        Assert.Contains("openapi", manifest.Settings.PullRequest.Labels);
        Assert.True(manifest.Settings.Validation!.FailOnValidationError);

        Assert.Equal(2, manifest.Apis.Count);

        var payments = manifest.Apis[0];
        Assert.Equal("bearer", payments.Source!.Auth!.Type);
        Assert.Equal("PAYMENTS_OPENAPI_TOKEN", payments.Source.Auth.TokenVariable);
        Assert.True(payments.Client!.CleanOutput);
        Assert.Equal(2, payments.Client.AdditionalArguments.Count);
        Assert.Equal("oauth2-client-credentials", payments.RuntimeAuth!.Type);
        Assert.Contains("payments.read", payments.RuntimeAuth.Scopes);
        Assert.Contains("dotnet build", payments.Validation!.Commands);

        var inventory = manifest.Apis[1];
        Assert.Equal("file", inventory.Source!.Type);
        Assert.Equal("external-specs/inventory.openapi.json", inventory.Source.Path);
        Assert.Equal("api-key-header", inventory.RuntimeAuth!.Type);
        Assert.Equal("X-API-Key", inventory.RuntimeAuth.HeaderName);
    }

    [Fact]
    public void LoadFromFile_MissingFile_Throws()
    {
        var ex = Assert.Throws<ManifestLoadException>(() => _loader.LoadFromFile("does-not-exist.yml"));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void LoadFromString_InvalidYaml_Throws()
    {
        var ex = Assert.Throws<ManifestLoadException>(
            () => _loader.LoadFromString("version: 1\n\tbad: : : indentation", "inline"));
        Assert.Contains("inline", ex.Message);
    }

    [Fact]
    public void LoadFromString_UnknownFields_AreIgnored()
    {
        const string yaml = """
            version: 1
            futureSetting: somethingNew
            apis:
              - name: payments
                source:
                  type: file
                  path: spec.json
                snapshot:
                  path: openapi/payments.json
                client:
                  language: csharp
                  generator: kiota
                  output: out
            """;

        var manifest = _loader.LoadFromString(yaml);
        Assert.Single(manifest.Apis);
    }
}
