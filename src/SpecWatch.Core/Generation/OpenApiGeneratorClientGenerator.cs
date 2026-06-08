using SpecWatch.Core.Configuration;
using SpecWatch.Core.Execution;

namespace SpecWatch.Core.Generation;

/// <summary>
/// Generates a client using the OpenAPI Generator CLI (see AGENTS.md, Section 12.3).
/// </summary>
public sealed class OpenApiGeneratorClientGenerator : ClientGeneratorBase
{
    public OpenApiGeneratorClientGenerator(ICommandRunner commandRunner, string? baseDirectory = null)
        : base(commandRunner, baseDirectory)
    {
    }

    public override string Name => "openapi-generator";

    protected override string FileName => "openapi-generator-cli";

    protected override IReadOnlyList<string> BuildArguments(ApiWatchDefinition api, string inputPath)
    {
        var client = api.Client!;

        // OpenAPI Generator uses a generator name per target language.
        var generatorName = (client.Language ?? "csharp").ToLowerInvariant() switch
        {
            "typescript" => "typescript",
            "java" => "java",
            _ => "csharp",
        };

        var args = new List<string>
        {
            "generate",
            "-i", inputPath,
            "-g", generatorName,
            "-o", client.Output!,
        };

        if (!string.IsNullOrWhiteSpace(client.PackageName))
        {
            args.Add("--package-name");
            args.Add(client.PackageName);
        }

        args.AddRange(client.AdditionalArguments);
        return args;
    }
}
