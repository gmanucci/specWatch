using SpecWatch.Core.Configuration;
using SpecWatch.Core.Generation;
using SpecWatch.Core.Tests.Execution;

namespace SpecWatch.Core.Tests.Generation;

public class ClientGeneratorTests
{
    private static ApiWatchDefinition Api(GenerationDefinition client) => new()
    {
        Name = "payments",
        Snapshot = new SnapshotDefinition { Path = "openapi/payments.json" },
        Client = client,
    };

    [Fact]
    public async Task Kiota_BuildsExpectedCommand()
    {
        var runner = new FakeCommandRunner();
        var generator = new KiotaClientGenerator(runner, "/repo");
        var api = Api(new GenerationDefinition
        {
            Language = "csharp",
            Generator = "kiota",
            Output = "src/Clients/Payments",
            Namespace = "Contoso.Clients.Payments",
            ClassName = "PaymentsClient",
            CleanOutput = true,
        });

        var result = await generator.GenerateAsync(api);

        Assert.True(result.Success);
        var (fileName, args, workingDir) = Assert.Single(runner.Invocations);
        Assert.Equal("kiota", fileName);
        Assert.Equal("/repo", workingDir);
        Assert.Contains("generate", args);
        Assert.Contains("--language", args);
        Assert.Contains("CSharp", args);
        Assert.Contains("PaymentsClient", args);
        Assert.Contains("Contoso.Clients.Payments", args);
        Assert.Contains("--clean-output", args);
        Assert.Equal(Path.Combine("/repo", "openapi/payments.json"), args[args.ToList().IndexOf("--openapi") + 1]);
    }

    [Fact]
    public async Task NSwag_BuildsSlashStyleArguments()
    {
        var runner = new FakeCommandRunner();
        var generator = new NSwagClientGenerator(runner, "/repo");
        var api = Api(new GenerationDefinition
        {
            Language = "csharp",
            Generator = "nswag",
            Output = "src/Clients/Inventory/InventoryClient.cs",
            Namespace = "Contoso.Clients.Inventory",
        });

        var result = await generator.GenerateAsync(api);

        Assert.True(result.Success);
        var (fileName, args, _) = Assert.Single(runner.Invocations);
        Assert.Equal("nswag", fileName);
        Assert.Contains("openapi2csclient", args);
        Assert.Contains(args, a => a.StartsWith("/output:"));
        Assert.Contains(args, a => a == "/namespace:Contoso.Clients.Inventory");
    }

    [Fact]
    public async Task Refitter_BuildsExpectedCommand()
    {
        var runner = new FakeCommandRunner();
        var generator = new RefitterClientGenerator(runner, "/repo");
        var api = Api(new GenerationDefinition
        {
            Language = "csharp",
            Generator = "refitter",
            Output = "src/Clients/Inventory",
            Namespace = "Contoso.Clients.Inventory",
        });

        await generator.GenerateAsync(api);

        var (fileName, args, _) = Assert.Single(runner.Invocations);
        Assert.Equal("refitter", fileName);
        Assert.Contains("--namespace", args);
    }

    [Fact]
    public async Task OpenApiGenerator_BuildsExpectedCommand()
    {
        var runner = new FakeCommandRunner();
        var generator = new OpenApiGeneratorClientGenerator(runner, "/repo");
        var api = Api(new GenerationDefinition
        {
            Language = "csharp",
            Generator = "openapi-generator",
            Output = "src/Clients/Billing",
            PackageName = "Contoso.Clients.Billing",
        });

        await generator.GenerateAsync(api);

        var (fileName, args, _) = Assert.Single(runner.Invocations);
        Assert.Equal("openapi-generator-cli", fileName);
        Assert.Contains("-g", args);
        Assert.Contains("csharp", args);
        Assert.Contains("--package-name", args);
        Assert.Contains("Contoso.Clients.Billing", args);
    }

    [Fact]
    public async Task Generator_CommandFailure_ReportsFailure()
    {
        var runner = new FakeCommandRunner(exitCode: 1);
        var generator = new KiotaClientGenerator(runner, "/repo");
        var api = Api(new GenerationDefinition
        {
            Language = "csharp",
            Generator = "kiota",
            Output = "out",
        });

        var result = await generator.GenerateAsync(api);

        Assert.False(result.Success);
        Assert.Equal(1, result.ExitCode);
    }

    [Theory]
    [InlineData("kiota", typeof(KiotaClientGenerator))]
    [InlineData("nswag", typeof(NSwagClientGenerator))]
    [InlineData("refitter", typeof(RefitterClientGenerator))]
    [InlineData("openapi-generator", typeof(OpenApiGeneratorClientGenerator))]
    public void Factory_CreatesCorrectGenerator(string name, Type expected)
    {
        var factory = new ClientGeneratorFactory(new FakeCommandRunner());
        Assert.True(factory.IsSupported(name));
        Assert.IsType(expected, factory.Create(name));
    }

    [Fact]
    public void Factory_UnknownGenerator_Throws()
    {
        var factory = new ClientGeneratorFactory(new FakeCommandRunner());
        Assert.False(factory.IsSupported("bogus"));
        Assert.Throws<ArgumentException>(() => factory.Create("bogus"));
    }
}
