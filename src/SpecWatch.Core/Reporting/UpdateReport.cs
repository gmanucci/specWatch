namespace SpecWatch.Core.Reporting;

/// <summary>Aggregate counts for a SpecWatch report (see AGENTS.md, Section 16).</summary>
public sealed class ReportSummary
{
    public int TotalApis { get; set; }

    public int ChangedApis { get; set; }

    public int GeneratedClients { get; set; }

    public int FailedApis { get; set; }
}

/// <summary>
/// Machine-readable report describing a SpecWatch <c>check</c> or <c>update</c>
/// run (see AGENTS.md, Section 16).
/// </summary>
public sealed class UpdateReport
{
    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset FinishedAt { get; set; }

    public string ManifestPath { get; set; } = "";

    public List<ApiUpdateResult> Apis { get; set; } = [];

    public ReportSummary Summary { get; set; } = new();

    /// <summary>Recomputes the <see cref="Summary"/> counts from <see cref="Apis"/>.</summary>
    public void RecomputeSummary()
    {
        Summary = new ReportSummary
        {
            TotalApis = Apis.Count,
            ChangedApis = Apis.Count(a => a.Changed),
            GeneratedClients = Apis.Count(a => a.GenerationSucceeded == true),
            FailedApis = Apis.Count(a => a.Error is not null
                || a.GenerationSucceeded == false
                || a.ValidationSucceeded == false),
        };
    }
}
