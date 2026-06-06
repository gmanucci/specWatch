using SpecWatch.Core.Configuration;
using SpecWatch.Core.Execution;

namespace SpecWatch.Core.Generation;

/// <summary>Generates a .NET client using NSwag (see AGENTS.md, Section 12.3).</summary>
public sealed class NSwagClientGenerator : ClientGeneratorBase
{
    public NSwagClientGenerator(ICommandRunner commandRunner, string? baseDirectory = null)
        : base(commandRunner, baseDirectory)
    {
    }

    public override string Name => "nswag";

    protected override string FileName => "nswag";

    protected override IReadOnlyList<string> BuildArguments(ApiWatchDefinition api, string inputPath)
    {
        var client = api.Client!;
        var args = new List<string>
        {
            "openapi2csclient",
            $"/input:{inputPath}",
            $"/output:{client.Output}",
        };

        if (!string.IsNullOrWhiteSpace(client.Namespace))
        {
            args.Add($"/namespace:{client.Namespace}");
        }

        if (!string.IsNullOrWhiteSpace(client.ClassName))
        {
            args.Add($"/className:{client.ClassName}");
        }

        args.AddRange(client.AdditionalArguments);
        return args;
    }
}
