using System.Net;
using System.Net.Http.Headers;
using System.Text;
using SpecWatch.Core.Configuration;

namespace SpecWatch.Core.Sources;

/// <summary>
/// Fetches an OpenAPI document over HTTP/HTTPS, applying configured source
/// authentication (see AGENTS.md, Sections 4.1 and 7.4). Secret values are
/// resolved from environment variables and never logged.
/// </summary>
public sealed class HttpOpenApiSource : IOpenApiSource
{
    private readonly HttpClient _httpClient;
    private readonly string _url;
    private readonly SourceAuthDefinition? _auth;
    private readonly ISecretResolver _secretResolver;

    public HttpOpenApiSource(
        HttpClient httpClient,
        string url,
        SourceAuthDefinition? auth,
        ISecretResolver secretResolver)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentNullException.ThrowIfNull(secretResolver);

        _httpClient = httpClient;
        _url = url;
        _auth = auth;
        _secretResolver = secretResolver;
    }

    public string Description => $"url:{_url}";

    public async Task<OpenApiFetchResult> FetchAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, _url);
        ApplyAuth(request);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new SourceFetchException($"Failed to fetch OpenAPI spec from '{_url}': {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new SourceFetchException($"Timed out fetching OpenAPI spec from '{_url}'.", ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new SourceFetchException(
                    $"Failed to fetch OpenAPI spec from '{_url}': HTTP {(int)response.StatusCode} {response.ReasonPhrase}.");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            return new OpenApiFetchResult
            {
                Content = bytes,
                SourceDescription = Description,
            };
        }
    }

    private void ApplyAuth(HttpRequestMessage request)
    {
        var type = _auth?.Type;
        if (string.IsNullOrWhiteSpace(type) ||
            type.Equals("anonymous", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        switch (type.ToLowerInvariant())
        {
            case "bearer":
            {
                var token = RequireSecret(_auth!.TokenVariable, "source.auth.tokenVariable");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                break;
            }

            case "header":
            {
                var value = RequireSecret(_auth!.ValueVariable, "source.auth.valueVariable");
                request.Headers.TryAddWithoutValidation(_auth.Name!, value);
                break;
            }

            case "basic":
            {
                var username = RequireSecret(_auth!.UsernameVariable, "source.auth.usernameVariable");
                var password = RequireSecret(_auth.PasswordVariable, "source.auth.passwordVariable");
                var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encoded);
                break;
            }

            default:
                throw new SourceFetchException($"Unsupported source auth type '{type}' for '{_url}'.");
        }
    }

    private string RequireSecret(string? variableName, string field)
    {
        if (string.IsNullOrWhiteSpace(variableName))
        {
            throw new SourceFetchException($"Missing '{field}' for authenticated source '{_url}'.");
        }

        var secret = _secretResolver.GetSecret(variableName);
        if (string.IsNullOrEmpty(secret))
        {
            // Report only the variable name, never the value.
            throw new SourceFetchException(
                $"Environment variable '{variableName}' (referenced by '{field}') is not set for source '{_url}'.");
        }

        return secret;
    }
}
