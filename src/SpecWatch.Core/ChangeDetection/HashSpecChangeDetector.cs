using System.Security.Cryptography;

namespace SpecWatch.Core.ChangeDetection;

/// <summary>
/// Sprint 1/2 change detection using a raw SHA-256 hash of the spec bytes
/// (see AGENTS.md, Section 11.1). If the snapshot does not exist, the spec is
/// considered changed.
/// </summary>
public sealed class HashSpecChangeDetector : ISpecChangeDetector
{
    public string Mode => "hash";

    public SpecChangeResult Detect(byte[] latestSpec, string snapshotPath)
    {
        ArgumentNullException.ThrowIfNull(latestSpec);
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshotPath);

        var latestHash = ComputeHash(latestSpec);

        if (!File.Exists(snapshotPath))
        {
            return new SpecChangeResult
            {
                Changed = true,
                SnapshotExisted = false,
                PreviousHash = null,
                LatestHash = latestHash,
            };
        }

        var snapshotBytes = File.ReadAllBytes(snapshotPath);
        var previousHash = ComputeHash(snapshotBytes);

        return new SpecChangeResult
        {
            Changed = !string.Equals(previousHash, latestHash, StringComparison.Ordinal),
            SnapshotExisted = true,
            PreviousHash = previousHash,
            LatestHash = latestHash,
        };
    }

    /// <summary>Computes the lowercase hex SHA-256 hash of the given bytes.</summary>
    public static string ComputeHash(byte[] content)
    {
        var hash = SHA256.HashData(content);
        return Convert.ToHexStringLower(hash);
    }
}
