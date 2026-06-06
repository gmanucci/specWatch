namespace SpecWatch.Core.Configuration;

/// <summary>
/// Performs semantic validation of a parsed <see cref="SpecWatchManifest"/>,
/// producing actionable error messages (see AGENTS.md, Sections 9.1 and 18.2).
/// </summary>
public sealed class ManifestValidator
{
    private const int SupportedVersion = 1;

    private static readonly HashSet<string> SupportedLanguages =
        new(StringComparer.OrdinalIgnoreCase) { "csharp" };

    private static readonly HashSet<string> SupportedGenerators =
        new(StringComparer.OrdinalIgnoreCase) { "kiota", "nswag", "refitter", "openapi-generator" };

    private static readonly HashSet<string> SupportedSourceTypes =
        new(StringComparer.OrdinalIgnoreCase) { "url", "file" };

    private static readonly HashSet<string> SupportedSourceAuthTypes =
        new(StringComparer.OrdinalIgnoreCase) { "anonymous", "bearer", "header", "basic" };

    private static readonly HashSet<string> SupportedRuntimeAuthTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "anonymous", "api-key-header", "bearer-static", "oauth2-client-credentials", "basic",
    };

    /// <summary>Validates the manifest and returns the collected errors.</summary>
    public ManifestValidationResult Validate(SpecWatchManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        var errors = new List<string>();

        if (manifest.Version != SupportedVersion)
        {
            errors.Add(
                $"Manifest version '{manifest.Version}' is not supported. Set 'version: {SupportedVersion}'.");
        }

        if (manifest.Apis.Count == 0)
        {
            errors.Add("Manifest must define at least one API under 'apis'.");
        }

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var snapshotPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var outputPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < manifest.Apis.Count; i++)
        {
            ValidateApi(manifest.Apis[i], i, errors, names, snapshotPaths, outputPaths);
        }

        return ManifestValidationResult.FromErrors(errors);
    }

    private static void ValidateApi(
        ApiWatchDefinition api,
        int index,
        List<string> errors,
        HashSet<string> names,
        Dictionary<string, string> snapshotPaths,
        Dictionary<string, string> outputPaths)
    {
        // Identify the API for error messages by name when available, else by index.
        var label = string.IsNullOrWhiteSpace(api.Name)
            ? $"API at index {index}"
            : $"API '{api.Name}'";

        if (string.IsNullOrWhiteSpace(api.Name))
        {
            errors.Add($"{label} is missing 'name'.");
        }
        else if (!names.Add(api.Name))
        {
            errors.Add($"Duplicate API name '{api.Name}'. API names must be unique.");
        }

        ValidateSource(api.Source, label, errors);
        ValidateSnapshot(api, label, errors, snapshotPaths);
        ValidateClient(api, label, errors, outputPaths);
        ValidateRuntimeAuth(api.RuntimeAuth, label, errors);
    }

    private static void ValidateSource(SourceDefinition? source, string label, List<string> errors)
    {
        if (source is null)
        {
            errors.Add($"{label} is missing 'source'.");
            return;
        }

        if (string.IsNullOrWhiteSpace(source.Type))
        {
            errors.Add($"{label} is missing 'source.type'. Expected one of: {Join(SupportedSourceTypes)}.");
        }
        else if (!SupportedSourceTypes.Contains(source.Type))
        {
            errors.Add(
                $"{label} has unsupported 'source.type' value '{source.Type}'. " +
                $"Expected one of: {Join(SupportedSourceTypes)}.");
        }
        else if (source.Type.Equals("url", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(source.Url))
            {
                errors.Add($"{label} has 'source.type: url' but is missing 'source.url'.");
            }
            else if (!Uri.TryCreate(source.Url, UriKind.Absolute, out var uri) ||
                     (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                errors.Add($"{label} has an invalid 'source.url' value '{source.Url}'. Expected an http/https URL.");
            }
        }
        else if (source.Type.Equals("file", StringComparison.OrdinalIgnoreCase) &&
                 string.IsNullOrWhiteSpace(source.Path))
        {
            errors.Add($"{label} has 'source.type: file' but is missing 'source.path'.");
        }

        ValidateSourceAuth(source.Auth, label, errors);
    }

    private static void ValidateSourceAuth(SourceAuthDefinition? auth, string label, List<string> errors)
    {
        if (auth is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(auth.Type))
        {
            errors.Add($"{label} has 'source.auth' but is missing 'source.auth.type'.");
            return;
        }

        if (!SupportedSourceAuthTypes.Contains(auth.Type))
        {
            errors.Add(
                $"{label} has unsupported 'source.auth.type' value '{auth.Type}'. " +
                $"Expected one of: {Join(SupportedSourceAuthTypes)}.");
            return;
        }

        switch (auth.Type.ToLowerInvariant())
        {
            case "bearer" when string.IsNullOrWhiteSpace(auth.TokenVariable):
                errors.Add($"{label} 'source.auth.type: bearer' requires 'source.auth.tokenVariable'.");
                break;
            case "header":
                if (string.IsNullOrWhiteSpace(auth.Name))
                {
                    errors.Add($"{label} 'source.auth.type: header' requires 'source.auth.name'.");
                }

                if (string.IsNullOrWhiteSpace(auth.ValueVariable))
                {
                    errors.Add($"{label} 'source.auth.type: header' requires 'source.auth.valueVariable'.");
                }

                break;
            case "basic":
                if (string.IsNullOrWhiteSpace(auth.UsernameVariable))
                {
                    errors.Add($"{label} 'source.auth.type: basic' requires 'source.auth.usernameVariable'.");
                }

                if (string.IsNullOrWhiteSpace(auth.PasswordVariable))
                {
                    errors.Add($"{label} 'source.auth.type: basic' requires 'source.auth.passwordVariable'.");
                }

                break;
        }
    }

    private static void ValidateSnapshot(
        ApiWatchDefinition api,
        string label,
        List<string> errors,
        Dictionary<string, string> snapshotPaths)
    {
        if (api.Snapshot is null || string.IsNullOrWhiteSpace(api.Snapshot.Path))
        {
            errors.Add($"{label} is missing 'snapshot.path'.");
            return;
        }

        var normalized = NormalizePath(api.Snapshot.Path);
        if (snapshotPaths.TryGetValue(normalized, out var other))
        {
            errors.Add(
                $"{label} has 'snapshot.path' '{api.Snapshot.Path}' that collides with {other}.");
        }
        else
        {
            snapshotPaths[normalized] = label;
        }
    }

    private static void ValidateClient(
        ApiWatchDefinition api,
        string label,
        List<string> errors,
        Dictionary<string, string> outputPaths)
    {
        var client = api.Client;
        if (client is null)
        {
            errors.Add($"{label} is missing 'client'.");
            return;
        }

        if (string.IsNullOrWhiteSpace(client.Language))
        {
            errors.Add($"{label} is missing 'client.language'. Expected one of: {Join(SupportedLanguages)}.");
        }
        else if (!SupportedLanguages.Contains(client.Language))
        {
            errors.Add(
                $"{label} has unsupported 'client.language' value '{client.Language}'. " +
                $"Expected one of: {Join(SupportedLanguages)}.");
        }

        if (string.IsNullOrWhiteSpace(client.Generator))
        {
            errors.Add($"{label} is missing 'client.generator'. Expected one of: {Join(SupportedGenerators)}.");
        }
        else if (!SupportedGenerators.Contains(client.Generator))
        {
            errors.Add(
                $"{label} has unsupported 'client.generator' value '{client.Generator}'. " +
                $"Expected one of: {Join(SupportedGenerators)}.");
        }

        if (string.IsNullOrWhiteSpace(client.Output))
        {
            errors.Add($"{label} is missing 'client.output'.");
            return;
        }

        var normalized = NormalizePath(client.Output);
        if (outputPaths.TryGetValue(normalized, out var other))
        {
            errors.Add(
                $"{label} has 'client.output' '{client.Output}' that collides with {other}.");
        }
        else
        {
            outputPaths[normalized] = label;
        }
    }

    private static void ValidateRuntimeAuth(RuntimeAuthDefinition? runtimeAuth, string label, List<string> errors)
    {
        if (runtimeAuth is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(runtimeAuth.Type))
        {
            errors.Add($"{label} has 'runtimeAuth' but is missing 'runtimeAuth.type'.");
            return;
        }

        if (!SupportedRuntimeAuthTypes.Contains(runtimeAuth.Type))
        {
            errors.Add(
                $"{label} has unsupported 'runtimeAuth.type' value '{runtimeAuth.Type}'. " +
                $"Expected one of: {Join(SupportedRuntimeAuthTypes)}.");
            return;
        }

        switch (runtimeAuth.Type.ToLowerInvariant())
        {
            case "api-key-header":
                if (string.IsNullOrWhiteSpace(runtimeAuth.HeaderName))
                {
                    errors.Add($"{label} 'runtimeAuth.type: api-key-header' requires 'runtimeAuth.headerName'.");
                }

                if (string.IsNullOrWhiteSpace(runtimeAuth.ValueVariable))
                {
                    errors.Add($"{label} 'runtimeAuth.type: api-key-header' requires 'runtimeAuth.valueVariable'.");
                }

                break;
            case "bearer-static" when string.IsNullOrWhiteSpace(runtimeAuth.TokenVariable):
                errors.Add($"{label} 'runtimeAuth.type: bearer-static' requires 'runtimeAuth.tokenVariable'.");
                break;
            case "oauth2-client-credentials":
                if (string.IsNullOrWhiteSpace(runtimeAuth.TokenUrl))
                {
                    errors.Add($"{label} 'runtimeAuth.type: oauth2-client-credentials' requires 'runtimeAuth.tokenUrl'.");
                }

                if (string.IsNullOrWhiteSpace(runtimeAuth.ClientIdVariable))
                {
                    errors.Add(
                        $"{label} 'runtimeAuth.type: oauth2-client-credentials' requires 'runtimeAuth.clientIdVariable'.");
                }

                if (string.IsNullOrWhiteSpace(runtimeAuth.ClientSecretVariable))
                {
                    errors.Add(
                        $"{label} 'runtimeAuth.type: oauth2-client-credentials' requires 'runtimeAuth.clientSecretVariable'.");
                }

                break;
            case "basic":
                if (string.IsNullOrWhiteSpace(runtimeAuth.UsernameVariable))
                {
                    errors.Add($"{label} 'runtimeAuth.type: basic' requires 'runtimeAuth.usernameVariable'.");
                }

                if (string.IsNullOrWhiteSpace(runtimeAuth.PasswordVariable))
                {
                    errors.Add($"{label} 'runtimeAuth.type: basic' requires 'runtimeAuth.passwordVariable'.");
                }

                break;
        }
    }

    private static string NormalizePath(string path) =>
        path.Replace('\\', '/').TrimEnd('/').Trim();

    private static string Join(IEnumerable<string> values) => string.Join(", ", values);
}
