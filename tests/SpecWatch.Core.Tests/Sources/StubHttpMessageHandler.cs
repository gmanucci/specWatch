using System.Net;

namespace SpecWatch.Core.Tests.Sources;

/// <summary>A stub <see cref="HttpMessageHandler"/> that captures the request and returns a fixed response.</summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly byte[] _content;

    public StubHttpMessageHandler(HttpStatusCode statusCode, byte[] content)
    {
        _statusCode = statusCode;
        _content = content;
    }

    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new ByteArrayContent(_content),
        };
        return Task.FromResult(response);
    }
}
