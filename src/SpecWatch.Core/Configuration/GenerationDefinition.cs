namespace SpecWatch.Core.Configuration;

/// <summary>
/// Describes how a client is generated for an API watch (see AGENTS.md, Section 12).
/// </summary>
public sealed class GenerationDefinition
{
    /// <summary>Target language. Sprint 1 supports <c>csharp</c>.</summary>
    public string? Language { get; set; }

    /// <summary>
    /// Generator name: <c>kiota</c>, <c>nswag</c>, <c>refitter</c>, or <c>openapi-generator</c>.
    /// </summary>
    public string? Generator { get; set; }

    /// <summary>Output path (directory or file, depending on the generator).</summary>
    public string? Output { get; set; }

    /// <summary>Namespace for the generated client (Kiota, NSwag, Refitter).</summary>
    public string? Namespace { get; set; }

    /// <summary>Generated client class name (Kiota, NSwag).</summary>
    public string? ClassName { get; set; }

    /// <summary>Package name (OpenAPI Generator).</summary>
    public string? PackageName { get; set; }

    /// <summary>Whether to clean the output directory before generation.</summary>
    public bool CleanOutput { get; set; }

    /// <summary>Extra arguments passed verbatim to the generator command.</summary>
    public List<string> AdditionalArguments { get; set; } = [];
}
