using SpecWatch.Core.Configuration;
using SpecWatch.Core.Execution;

namespace SpecWatch.Core.Generation;

/// <summary>Generates a .NET client using Refitter (see AGENTS.md, Section 12.3).</summary>
public sealed class RefitterClientGenerator : ClientGeneratorBase
{
    public RefitterClientGenerator(ICommandRunner commandRunner, string? baseDirectory = null)
        : base(commandRunner, baseDirectory)
    {
    }

    public override string Name => "refitter";

    protected override string FileName => "refitter";

    protected override IReadOnlyList<string> BuildArguments(ApiWatchDefinition api, string inputPath)
    {
        var client = api.Client!;
        var args = new List<string>
        {
            "--openapi", inputPath,
            "--output", client.Output!,
        };

        if (!string.IsNullOrWhiteSpace(client.Namespace))
        {
            args.Add("--namespace");
            args.Add(client.Namespace);
        }

        args.AddRange(client.AdditionalArguments);
        return args;
    }
}
