using System.Reflection;

// SpecWatch CLI entry point.
//
// Sprint 0 establishes the executable and a minimal command surface. Commands
// such as `validate`, `check`, `update`, and `init` are defined by the harness
// (AGENTS.md, Section 9) and are implemented in later sprints.

if (args.Length == 1 && (args[0] == "--version" || args[0] == "-v"))
{
    Console.WriteLine(GetVersion());
    return 0;
}

Console.WriteLine($"SpecWatch {GetVersion()}");
Console.WriteLine("SpecWatch keeps generated API clients in sync with OpenAPI contracts.");
Console.WriteLine();
Console.WriteLine("Commands (implemented in later sprints):");
Console.WriteLine("  validate   Validate a manifest file.");
Console.WriteLine("  check      Check watched specs for changes.");
Console.WriteLine("  update     Regenerate clients for changed specs.");
Console.WriteLine("  init       Scaffold CI/CD templates.");
return 0;

static string GetVersion()
{
    var version = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
        ?? "0.0.0";
    return version;
}
