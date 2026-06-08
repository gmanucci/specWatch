namespace SpecWatch.Core.Configuration;

/// <summary>
/// Thrown when a manifest file cannot be read or parsed (as opposed to being
/// parsed successfully but failing semantic validation).
/// </summary>
public sealed class ManifestLoadException : Exception
{
    public ManifestLoadException(string message)
        : base(message)
    {
    }

    public ManifestLoadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
