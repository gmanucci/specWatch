namespace SpecWatch.Core.ChangeDetection;

/// <summary>The result of comparing a freshly fetched spec against its snapshot.</summary>
public sealed class SpecChangeResult
{
    /// <summary>Whether the latest spec differs from the snapshot (or no snapshot exists).</summary>
    public required bool Changed { get; init; }

    /// <summary>Whether a snapshot already existed prior to detection.</summary>
    public required bool SnapshotExisted { get; init; }

    /// <summary>Hash of the existing snapshot, if any.</summary>
    public string? PreviousHash { get; init; }

    /// <summary>Hash of the latest fetched spec.</summary>
    public required string LatestHash { get; init; }
}
