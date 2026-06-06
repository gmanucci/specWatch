namespace SpecWatch.Core;

/// <summary>
/// Process exit codes used by the SpecWatch CLI (see AGENTS.md, Section 10).
/// </summary>
public static class ExitCodes
{
    /// <summary>Success: no changes, or a completed update.</summary>
    public const int Success = 0;

    /// <summary>General error.</summary>
    public const int GeneralError = 1;

    /// <summary>Changes detected in check mode.</summary>
    public const int ChangesDetected = 2;

    /// <summary>Manifest validation failed.</summary>
    public const int ManifestValidationFailed = 3;

    /// <summary>Source fetch failed.</summary>
    public const int SourceFetchFailed = 4;

    /// <summary>Generation failed.</summary>
    public const int GenerationFailed = 5;

    /// <summary>Validation command failed.</summary>
    public const int ValidationCommandFailed = 6;
}
