using System.Net;
using System.Text;
using SpecWatch.Core.Configuration;
using SpecWatch.Core.Sources;

namespace SpecWatch.Core.Tests.Sources;

public class HttpOpenApiSourceTests
{
    private static HttpClient ClientReturning(byte[] content, out StubHttpMessageHandler handler, HttpStatusCode code = HttpStatusCode.OK)
    {
        handler = new StubHttpMessageHandler(code, content);
        return new HttpClient(handler);
    }

    [Fact]
    public async Task FetchAsync_Anonymous_ReturnsContent()
    {
        var body = Encoding.UTF8.GetBytes("{\"openapi\":\"3.0.0\"}");
        var client = ClientReturning(body, out var handler);
        var source = new HttpOpenApiSource(client, "https://example.com/openapi.json", null, new DictionarySecretResolver());

        var result = await source.FetchAsync();

        Assert.Equal(body, result.Content);
        Assert.Null(handler.LastRequest!.Headers.Authorization);
    }

    [Fact]
    public async Task FetchAsync_BearerAuth_AddsAuthorizationHeader()
    {
        var client = ClientReturning(Encoding.UTF8.GetBytes("{}"), out var handler);
        var auth = new SourceAuthDefinition { Type = "bearer", TokenVariable = "TOKEN" };
        var secrets = new DictionarySecretResolver(new() { ["TOKEN"] = "secret-token" });
        var source = new HttpOpenApiSource(client, "https://example.com/openapi.json", auth, secrets);

        await source.FetchAsync();

        Assert.Equal("Bearer", handler.LastRequest!.Headers.Authorization!.Scheme);
        Assert.Equal("secret-token", handler.LastRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task FetchAsync_HeaderAuth_AddsCustomHeader()
    {
        var client = ClientReturning(Encoding.UTF8.GetBytes("{}"), out var handler);
        var auth = new SourceAuthDefinition { Type = "header", Name = "X-API-Key", ValueVariable = "KEY" };
        var secrets = new DictionarySecretResolver(new() { ["KEY"] = "abc123" });
        var source = new HttpOpenApiSource(client, "https://example.com/openapi.json", auth, secrets);

        await source.FetchAsync();

        Assert.True(handler.LastRequest!.Headers.TryGetValues("X-API-Key", out var values));
        Assert.Equal("abc123", values!.Single());
    }

    [Fact]
    public async Task FetchAsync_MissingSecret_ThrowsWithoutLeakingValue()
    {
        var client = ClientReturning(Encoding.UTF8.GetBytes("{}"), out _);
        var auth = new SourceAuthDefinition { Type = "bearer", TokenVariable = "UNSET_TOKEN" };
        var source = new HttpOpenApiSource(client, "https://example.com/openapi.json", auth, new DictionarySecretResolver());

        var ex = await Assert.ThrowsAsync<SourceFetchException>(() => source.FetchAsync());
        Assert.Contains("UNSET_TOKEN", ex.Message);
    }

    [Fact]
    public async Task FetchAsync_HttpError_Throws()
    {
        var client = ClientReturning(Encoding.UTF8.GetBytes("nope"), out _, HttpStatusCode.NotFound);
        var source = new HttpOpenApiSource(client, "https://example.com/openapi.json", null, new DictionarySecretResolver());

        var ex = await Assert.ThrowsAsync<SourceFetchException>(() => source.FetchAsync());
        Assert.Contains("404", ex.Message);
    }
}
