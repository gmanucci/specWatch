namespace SpecWatch.Core.Configuration;

/// <summary>Validation commands run after client generation (see AGENTS.md, Section 7.2).</summary>
public sealed class ValidationDefinition
{
    /// <summary>Shell commands to run, in order, after generation.</summary>
    public List<string> Commands { get; set; } = [];
}
