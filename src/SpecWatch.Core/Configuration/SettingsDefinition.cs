namespace SpecWatch.Core.Configuration;

/// <summary>Global settings block (see AGENTS.md, Section 8.2).</summary>
public sealed class SettingsDefinition
{
    public ChangeDetectionSettings? ChangeDetection { get; set; }

    public PullRequestSettings? PullRequest { get; set; }

    public ValidationSettings? Validation { get; set; }
}

/// <summary>Change-detection configuration. Sprint 1 supports <c>hash</c> mode.</summary>
public sealed class ChangeDetectionSettings
{
    /// <summary>Detection mode. Supported: <c>hash</c>. Future: <c>normalized</c>, <c>semantic</c>.</summary>
    public string Mode { get; set; } = "hash";
}

/// <summary>Pull-request metadata configuration.</summary>
public sealed class PullRequestSettings
{
    /// <summary>PR strategy. Supported: <c>single</c>. Future: <c>per-api</c>.</summary>
    public string Mode { get; set; } = "single";

    public string? BranchPrefix { get; set; }

    public string? Title { get; set; }

    public List<string> Labels { get; set; } = [];
}

/// <summary>Validation behavior configuration.</summary>
public sealed class ValidationSettings
{
    /// <summary>When true, a failed validation command fails the run.</summary>
    public bool FailOnValidationError { get; set; } = true;
}
