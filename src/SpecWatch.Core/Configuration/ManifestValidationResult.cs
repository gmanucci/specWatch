namespace SpecWatch.Core.Configuration;

/// <summary>The outcome of validating a <see cref="SpecWatchManifest"/>.</summary>
public sealed class ManifestValidationResult
{
    private readonly List<string> _errors;

    private ManifestValidationResult(List<string> errors)
    {
        _errors = errors;
    }

    /// <summary>Whether the manifest passed validation with no errors.</summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>The list of actionable validation error messages.</summary>
    public IReadOnlyList<string> Errors => _errors;

    public static ManifestValidationResult FromErrors(List<string> errors) => new(errors);
}
