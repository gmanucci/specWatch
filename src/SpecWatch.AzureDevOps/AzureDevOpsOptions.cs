namespace SpecWatch.AzureDevOps;

/// <summary>
/// Configuration for Azure DevOps git and pull-request operations
/// (see AGENTS.md, Sections 14 and 15). Secret values are referenced by
/// environment-variable name and never stored here.
/// </summary>
public sealed class AzureDevOpsOptions
{
    /// <summary>Azure DevOps organization/collection URL (e.g., System.CollectionUri).</summary>
    public string? OrganizationUrl { get; set; }

    /// <summary>Project name (e.g., System.TeamProject).</summary>
    public string? Project { get; set; }

    /// <summary>Repository name (e.g., Build.Repository.Name).</summary>
    public string? Repository { get; set; }

    /// <summary>Branch the PR targets.</summary>
    public string TargetBranch { get; set; } = "main";

    /// <summary>Branch SpecWatch pushes changes to.</summary>
    public string UpdateBranch { get; set; } = "chore/specwatch/openapi-clients";

    /// <summary>Pull request title.</summary>
    public string Title { get; set; } = "chore: update generated OpenAPI clients";

    /// <summary>Pull request description.</summary>
    public string Description { get; set; } =
        "SpecWatch detected OpenAPI changes and regenerated API clients.";

    /// <summary>Whether to delete the source branch after the PR completes.</summary>
    public bool DeleteSourceBranch { get; set; } = true;

    /// <summary>Git commit author name.</summary>
    public string CommitUserName { get; set; } = "SpecWatch Bot";

    /// <summary>Git commit author email.</summary>
    public string CommitUserEmail { get; set; } = "specwatch-bot@local";

    /// <summary>Commit message used when pushing changes.</summary>
    public string CommitMessage { get; set; } = "chore: update generated OpenAPI clients";
}
