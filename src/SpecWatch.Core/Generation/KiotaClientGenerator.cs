using SpecWatch.Core.Configuration;
using SpecWatch.Core.Execution;

namespace SpecWatch.Core.Generation;

/// <summary>Generates a .NET client using Kiota (see AGENTS.md, Section 12.3).</summary>
public sealed class KiotaClientGenerator : ClientGeneratorBase
{
    public KiotaClientGenerator(ICommandRunner commandRunner, string? baseDirectory = null)
        : base(commandRunner, baseDirectory)
    {
    }

    public override string Name => "kiota";

    protected override string FileName => "kiota";

    protected override IReadOnlyList<string> BuildArguments(ApiWatchDefinition api, string inputPath)
    {
        var client = api.Client!;
        var args = new List<string>
        {
            "generate",
            "--openapi", inputPath,
            "--language", "CSharp",
            "--output", client.Output!,
        };

        if (!string.IsNullOrWhiteSpace(client.ClassName))
        {
            args.Add("--class-name");
            args.Add(client.ClassName);
        }

        if (!string.IsNullOrWhiteSpace(client.Namespace))
        {
            args.Add("--namespace-name");
            args.Add(client.Namespace);
        }

        if (client.CleanOutput)
        {
            args.Add("--clean-output");
        }

        args.AddRange(client.AdditionalArguments);
        return args;
    }
}
