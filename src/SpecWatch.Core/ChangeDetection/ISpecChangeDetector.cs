namespace SpecWatch.Core.ChangeDetection;

/// <summary>
/// Detects whether a freshly fetched OpenAPI spec differs from its stored
/// snapshot (see AGENTS.md, Section 11).
/// </summary>
public interface ISpecChangeDetector
{
    /// <summary>The detection mode name (e.g., <c>hash</c>).</summary>
    string Mode { get; }

    /// <summary>
    /// Compares the latest fetched spec bytes against the snapshot file at
    /// <paramref name="snapshotPath"/>.
    /// </summary>
    SpecChangeResult Detect(byte[] latestSpec, string snapshotPath);
}
