using SpecWatch.Core.Configuration;
using SpecWatch.Core.Sources;

namespace SpecWatch.Core.Tests.Sources;

public class OpenApiSourceFactoryAndLocalFileTests
{
    private readonly HttpClient _httpClient = new();

    [Fact]
    public async Task LocalFileSource_ReadsExistingFile()
    {
        var path = Fixtures.Spec("payments-v1.openapi.json");
        var source = new LocalFileOpenApiSource(path);

        var result = await source.FetchAsync();

        Assert.NotEmpty(result.Content);
        Assert.StartsWith("file:", result.SourceDescription);
    }

    [Fact]
    public async Task LocalFileSource_MissingFile_Throws()
    {
        var source = new LocalFileOpenApiSource("/does/not/exist.json");
        await Assert.ThrowsAsync<SourceFetchException>(() => source.FetchAsync());
    }

    [Fact]
    public void Factory_FileType_CreatesLocalFileSource()
    {
        var factory = new OpenApiSourceFactory(_httpClient);
        var source = factory.Create(new SourceDefinition { Type = "file", Path = "spec.json" });
        Assert.IsType<LocalFileOpenApiSource>(source);
    }

    [Fact]
    public void Factory_UrlType_CreatesHttpSource()
    {
        var factory = new OpenApiSourceFactory(_httpClient);
        var source = factory.Create(new SourceDefinition { Type = "url", Url = "https://example.com/o.json" });
        Assert.IsType<HttpOpenApiSource>(source);
    }

    [Fact]
    public void Factory_UnknownType_Throws()
    {
        var factory = new OpenApiSourceFactory(_httpClient);
        Assert.Throws<SourceFetchException>(() => factory.Create(new SourceDefinition { Type = "ftp" }));
    }
}
