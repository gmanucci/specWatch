namespace SpecWatch.Core.Sources;

/// <summary>Thrown when an OpenAPI document cannot be fetched from its source.</summary>
public sealed class SourceFetchException : Exception
{
    public SourceFetchException(string message)
        : base(message)
    {
    }

    public SourceFetchException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
