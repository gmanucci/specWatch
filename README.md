# SpecWatch

**SpecWatch keeps generated API clients in sync with OpenAPI contracts.**

SpecWatch is a Dependabot-like automation tool for OpenAPI-driven client
generation. It watches configured OpenAPI specifications from local files or
remote URLs, detects meaningful changes, regenerates client code using
configured generators, validates the result, and opens or updates pull requests
with the changes.

The initial focus is **.NET client generation** and **Azure DevOps Pipelines**,
with an architecture designed to later support additional languages (TypeScript,
Java, Python, Go) and CI systems (GitHub Actions, GitLab CI, Bitbucket
Pipelines).

> The full project specification, architecture, sprint plan, and agent operating
> rules live in [`AGENTS.md`](AGENTS.md), which is the source of truth for this
> repository.

## Repository layout

```text
src/
  SpecWatch.Cli/          # `specwatch` command-line executable
  SpecWatch.Core/         # Core engine (sources, change detection, generation)
  SpecWatch.AzureDevOps/  # Azure DevOps provider integration
tests/
  SpecWatch.Core.Tests/   # Unit tests for the core engine
  SpecWatch.Cli.Tests/    # Tests for the CLI
```

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/) 9.0 or later

## Build and test

```bash
dotnet build SpecWatch.sln
dotnet test SpecWatch.sln
```

## Run

```bash
dotnet run --project src/SpecWatch.Cli -- --version
```

## Status

This project is being implemented incrementally through sprints. See the
**Roadmap and Sprint Plan** and **Session History** sections in
[`AGENTS.md`](AGENTS.md) for current progress. Sprint 0 (Repository Bootstrap)
establishes the solution, projects, and tooling.

## License

Licensed under the [MIT License](LICENSE).
