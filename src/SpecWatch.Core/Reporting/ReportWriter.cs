using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpecWatch.Core.Reporting;

/// <summary>Serializes a <see cref="UpdateReport"/> to JSON (see AGENTS.md, Section 16).</summary>
public sealed class ReportWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Serializes the report to a JSON string.</summary>
    public string Serialize(UpdateReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return JsonSerializer.Serialize(report, Options);
    }

    /// <summary>Writes the report as JSON to the given path, creating directories as needed.</summary>
    public void WriteToFile(UpdateReport report, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, Serialize(report));
    }
}
