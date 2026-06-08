# SpecWatch — Agent Harness and Implementation Specification

> **Agents: read this file first.** This harness is the source of truth for project
> direction, architecture, sprint planning, and implementation constraints. Before
> making any implementation changes, read this document in full and follow the
> [Agent Operating Rules](#5-agent-operating-rules).

## 1. Product Name

**SpecWatch**

## 2. Tagline

**SpecWatch keeps generated API clients in sync with OpenAPI contracts.**

## 3. Product Summary

SpecWatch is a Dependabot-like automation tool for OpenAPI-driven client generation.

It watches configured OpenAPI specifications from local files or remote URLs, detects meaningful changes, regenerates client code using configured generators, validates the result, and opens or updates pull requests with the changes.

The initial focus is **.NET client generation** and **Azure DevOps Pipelines**, with an architecture that can later support additional languages and ecosystems such as:

- TypeScript
- Java
- Python
- Go
- GitHub Actions
- GitLab CI
- Bitbucket Pipelines

SpecWatch should be modular and extensible. The core engine should understand OpenAPI sources, change detection, generation orchestration, and reporting. Platform-specific behavior such as creating Azure DevOps pull requests should be isolated behind provider abstractions.

---

# 4. Core Goals

## 4.1 Primary Goals

SpecWatch must:

1. Read a manifest file describing watched OpenAPI APIs.
2. Support OpenAPI specs from:
   - local repository files
   - remote HTTP/HTTPS URLs
3. Support per-source authentication for protected OpenAPI documents.
4. Compare the current known spec against the latest fetched spec.
5. Detect whether the spec has changed.
6. Regenerate the configured API client when needed.
7. Support .NET client generation initially.
8. Support multiple generators, starting with:
   - Kiota
   - NSwag
   - Refitter
   - OpenAPI Generator
9. Run validation commands after generation.
10. Produce a structured update report.
11. Support Azure DevOps Pipelines as the first CI/CD target.
12. Be designed to later support TypeScript and Java client generation.
13. Be designed to later support GitHub Actions.

## 4.2 Non-Goals for Initial Sprints

The initial implementation does **not** need to:

1. Host a server.
2. Provide a web dashboard.
3. Store state in a database.
4. Automatically merge PRs.
5. Perform full semantic OpenAPI breaking-change analysis.
6. Support every OpenAPI generator immediately.
7. Support every CI system immediately.
8. Generate production runtime secrets.
9. Modify application business logic.
10. Replace existing API gateway or contract testing systems.

---

# 5. Agent Operating Rules

Every agent session working on SpecWatch must follow these rules.

## 5.1 Mandatory Harness Reading

Before making any implementation changes, the agent must read this harness file.

Recommended location:

```text
AGENTS.md
```

The agent must treat this harness as the source of truth for project direction, architecture, sprint planning, and implementation constraints.

## 5.2 Sprint-Based Development

SpecWatch must be implemented incrementally through sprints.

Each new agent session must:

1. Read this harness.
2. Read the current sprint history.
3. Identify the next incomplete sprint.
4. Work only on the selected sprint unless explicitly instructed otherwise.
5. Update the harness after implementation.
6. Add a new session entry describing:
   - what was implemented
   - what files changed
   - what tests were added
   - what remains incomplete
   - suggested next sprint

## 5.3 Harness Update Requirement

After every implementation session, the agent must update this document.

The update must include:

1. A new entry under **Session History**.
2. Any completed sprint tasks checked off.
3. Any changed architectural decisions.
4. Any new commands needed to build, test, or run the project.
5. Any known limitations or follow-up work.

## 5.4 No Silent Scope Expansion

The agent must not silently expand scope.

If a sprint says to implement manifest parsing, the agent should not also implement Azure DevOps PR creation unless the sprint explicitly says so.

## 5.5 Prefer Small, Reviewable Changes

Each session should produce focused changes that can be reviewed independently.

## 5.6 Generated Code Rule

SpecWatch may generate client code in target projects, but SpecWatch’s own generated fixtures or sample output must be clearly separated from source code.

Recommended paths:

```text
/test-fixtures/generated/
.samples/generated/
```

## 5.7 Secrets Rule

No secrets may be committed.

Configuration files may reference environment variable names or secret variable names, but must not contain secret values.

Allowed:

```yaml
tokenVariable: PAYMENTS_OPENAPI_TOKEN
```

Not allowed:

```yaml
token: ******
```

---

# 6. Recommended Repository Structure

Initial repository structure:

```text
/
  AGENTS.md
  README.md
  LICENSE
  .gitignore
  SpecWatch.sln

  src/
    SpecWatch.Cli/
      SpecWatch.Cli.csproj
      Program.cs

    SpecWatch.Core/
      SpecWatch.Core.csproj

      Configuration/
        SpecWatchManifest.cs
        ApiWatchDefinition.cs
        SourceDefinition.cs
        SourceAuthDefinition.cs
        GenerationDefinition.cs
        ValidationDefinition.cs

      Sources/
        IOpenApiSource.cs
        LocalFileOpenApiSource.cs
        HttpOpenApiSource.cs
        OpenApiSourceFactory.cs

      ChangeDetection/
        ISpecChangeDetector.cs
        HashSpecChangeDetector.cs
        NormalizedHashSpecChangeDetector.cs
        SpecChangeResult.cs

      Generation/
        IClientGenerator.cs
        ClientGeneratorFactory.cs
        GeneratorResult.cs
        KiotaClientGenerator.cs
        NSwagClientGenerator.cs
        RefitterClientGenerator.cs
        OpenApiGeneratorClientGenerator.cs

      Execution/
        ICommandRunner.cs
        ProcessCommandRunner.cs
        CommandResult.cs

      Reporting/
        UpdateReport.cs
        ApiUpdateResult.cs
        ReportWriter.cs

      Pipeline/
        SpecWatchRunner.cs
        SpecWatchRunOptions.cs

    SpecWatch.AzureDevOps/
      SpecWatch.AzureDevOps.csproj
      AzureDevOpsPullRequestService.cs
      AzureDevOpsGitService.cs
      AzureDevOpsOptions.cs

  tests/
    SpecWatch.Core.Tests/
      SpecWatch.Core.Tests.csproj

    SpecWatch.Cli.Tests/
      SpecWatch.Cli.Tests.csproj

  samples/
    dotnet-kiota/
      specwatch.yml
      azure-pipelines.yml

    dotnet-nswag/
      specwatch.yml
      azure-pipelines.yml

  docs/
    manifest.md
    azure-devops.md
    generators.md
    auth.md
    roadmap.md
```

---

# 7. Core Concepts

## 7.1 Manifest

SpecWatch is driven by a manifest file.

Default manifest filename:

```text
specwatch.yml
```

Alternative supported filenames later:

```text
specwatch.yaml
.specwatch.yml
.specwatch.yaml
```

The manifest describes:

1. Which APIs to watch.
2. Where their OpenAPI specs are located.
3. How to authenticate when fetching specs.
4. Where local snapshots are stored.
5. Which generator to use.
6. Where generated clients are written.
7. Which commands to run after generation.
8. PR metadata.

## 7.2 API Watch

An API watch is one configured OpenAPI dependency.

Example:

```yaml
apis:
  - name: payments
    source:
      type: url
      url: https://payments.example.com/openapi.json
      auth:
        type: bearer
        tokenVariable: PAYMENTS_OPENAPI_TOKEN

    snapshot:
      path: openapi/payments.openapi.json

    client:
      language: csharp
      generator: kiota
      output: src/Clients/Payments
      namespace: Contoso.Clients.Payments
      className: PaymentsClient

    runtimeAuth:
      type: oauth2-client-credentials
      tokenUrl: https://login.example.com/oauth2/token
      clientIdVariable: PAYMENTS_CLIENT_ID
      clientSecretVariable: PAYMENTS_CLIENT_SECRET
      scopes:
        - payments.read
        - payments.write

    validation:
      commands:
        - dotnet build
        - dotnet test
```

## 7.3 Snapshot

A snapshot is the local pinned copy of the OpenAPI document.

SpecWatch compares the latest fetched spec against the snapshot.

If changed, the snapshot is updated and client code is regenerated.

Example snapshot path:

```text
openapi/payments.openapi.json
```

## 7.4 Source Authentication

Source authentication is used only to fetch the OpenAPI document.

Supported initial source auth types:

```yaml
auth:
  type: anonymous
```

```yaml
auth:
  type: bearer
  tokenVariable: PAYMENTS_OPENAPI_TOKEN
```

```yaml
auth:
  type: header
  name: X-API-Key
  valueVariable: INVENTORY_OPENAPI_KEY
```

```yaml
auth:
  type: basic
  usernameVariable: PARTNER_OPENAPI_USERNAME
  passwordVariable: PARTNER_OPENAPI_PASSWORD
```

## 7.5 Runtime Authentication

Runtime authentication is the authentication method expected by the generated client at application runtime.

SpecWatch should not store secret values.

Initial runtime auth types:

```yaml
runtimeAuth:
  type: anonymous
```

```yaml
runtimeAuth:
  type: api-key-header
  headerName: X-API-Key
  valueVariable: INVENTORY_API_KEY
```

```yaml
runtimeAuth:
  type: bearer-static
  tokenVariable: PARTNER_API_TOKEN
```

```yaml
runtimeAuth:
  type: oauth2-client-credentials
  tokenUrl: https://login.example.com/oauth2/token
  clientIdVariable: PAYMENTS_CLIENT_ID
  clientSecretVariable: PAYMENTS_CLIENT_SECRET
  scopes:
    - payments.read
```

```yaml
runtimeAuth:
  type: basic
  usernameVariable: PARTNER_USERNAME
  passwordVariable: PARTNER_PASSWORD
```

Runtime auth generation should initially be documented and later implemented as generated DI helpers.

---

# 8. Manifest Specification

## 8.1 Minimal Manifest

```yaml
version: 1

apis:
  - name: payments
    source:
      type: url
      url: https://payments.example.com/openapi.json

    snapshot:
      path: openapi/payments.openapi.json

    client:
      language: csharp
      generator: kiota
      output: src/Clients/Payments
      namespace: Contoso.Clients.Payments
      className: PaymentsClient
```

## 8.2 Full Manifest Example

```yaml
version: 1

settings:
  changeDetection:
    mode: hash

  pullRequest:
    mode: single
    branchPrefix: chore/specwatch
    title: "chore: update generated OpenAPI clients"
    labels:
      - dependencies
      - openapi
      - generated-client

  validation:
    failOnValidationError: true

apis:
  - name: payments
    enabled: true

    source:
      type: url
      url: https://payments.example.com/openapi.json
      auth:
        type: bearer
        tokenVariable: PAYMENTS_OPENAPI_TOKEN

    snapshot:
      path: openapi/payments.openapi.json

    client:
      language: csharp
      generator: kiota
      output: src/Clients/Payments
      namespace: Contoso.Clients.Payments
      className: PaymentsClient
      cleanOutput: true
      additionalArguments:
        - "--structured-mime-types"
        - "application/json"

    runtimeAuth:
      type: oauth2-client-credentials
      tokenUrl: https://login.example.com/oauth2/token
      clientIdVariable: PAYMENTS_CLIENT_ID
      clientSecretVariable: PAYMENTS_CLIENT_SECRET
      scopes:
        - payments.read
        - payments.write

    validation:
      commands:
        - dotnet build
        - dotnet test

  - name: inventory
    enabled: true

    source:
      type: file
      path: external-specs/inventory.openapi.json

    snapshot:
      path: openapi/inventory.openapi.json

    client:
      language: csharp
      generator: nswag
      output: src/Clients/Inventory/InventoryClient.cs
      namespace: Contoso.Clients.Inventory
      cleanOutput: false

    runtimeAuth:
      type: api-key-header
      headerName: X-API-Key
      valueVariable: INVENTORY_API_KEY
```

---

# 9. CLI Design

Initial executable:

```text
specwatch
```

## 9.1 Commands

### Validate Manifest

```bash
specwatch validate --manifest specwatch.yml
```

Responsibilities:

1. Parse manifest.
2. Validate schema.
3. Validate required fields.
4. Validate supported generator values.
5. Validate supported language values.
6. Validate path collisions.
7. Print useful errors.

### Check for Changes

```bash
specwatch check --manifest specwatch.yml
```

Responsibilities:

1. Fetch all configured specs.
2. Compare with snapshots.
3. Produce report.
4. Do not modify generated clients.
5. Return:
   - exit code `0` if no changes
   - exit code `2` if changes found
   - exit code `1` on error

### Update Clients

```bash
specwatch update --manifest specwatch.yml
```

Responsibilities:

1. Fetch specs.
2. Compare with snapshots.
3. Update changed snapshots.
4. Regenerate changed clients.
5. Run validation commands if configured.
6. Write report.

### Generate Azure Pipeline Template

```bash
specwatch init azure-devops
```

Responsibilities:

1. Create example `azure-pipelines.yml`.
2. Create example `specwatch.yml`.
3. Avoid overwriting existing files unless `--force` is used.

### Print Version

```bash
specwatch --version
```

---

# 10. Exit Codes

SpecWatch should use predictable exit codes.

```text
0 = success, no changes or completed update
1 = general error
2 = changes detected in check mode
3 = manifest validation failed
4 = source fetch failed
5 = generation failed
6 = validation command failed
```

---

# 11. Change Detection

## 11.1 Sprint 1 Mode: Raw Hash

Initial change detection should use raw file hash.

Algorithm:

```text
fetch latest spec
if snapshot does not exist:
    changed = true
else:
    compare SHA256 of latest spec with SHA256 of snapshot
```

## 11.2 Future Mode: Normalized Hash

Future normalized mode should:

1. Parse JSON or YAML.
2. Convert YAML to JSON.
3. Sort object keys.
4. Remove insignificant formatting differences.
5. Hash normalized content.

## 11.3 Future Mode: Semantic OpenAPI Diff

Future semantic mode should categorize:

1. breaking changes
2. non-breaking changes
3. documentation-only changes
4. schema additions
5. endpoint additions
6. endpoint removals

---

# 12. Client Generation Architecture

## 12.1 Interface

```csharp
public interface IClientGenerator
{
    string Name { get; }

    Task<GeneratorResult> GenerateAsync(
        ApiWatchDefinition api,
        CancellationToken cancellationToken);
}
```

## 12.2 Generator Result

```csharp
public sealed class GeneratorResult
{
    public bool Success { get; init; }

    public string GeneratorName { get; init; } = "";

    public string ApiName { get; init; } = "";

    public IReadOnlyList<string> ChangedFiles { get; init; } = [];

    public string? StandardOutput { get; init; }

    public string? StandardError { get; init; }

    public int ExitCode { get; init; }
}
```

## 12.3 Supported Initial Generators

### Kiota

Manifest:

```yaml
client:
  language: csharp
  generator: kiota
  output: src/Clients/Payments
  namespace: Contoso.Clients.Payments
  className: PaymentsClient
```

Command shape:

```bash
kiota generate \
  --openapi openapi/payments.openapi.json \
  --language CSharp \
  --class-name PaymentsClient \
  --namespace-name Contoso.Clients.Payments \
  --output src/Clients/Payments \
  --clean-output
```

### NSwag

Manifest:

```yaml
client:
  language: csharp
  generator: nswag
  output: src/Clients/Inventory/InventoryClient.cs
  namespace: Contoso.Clients.Inventory
```

Command shape:

```bash
nswag openapi2csclient \
  /input:openapi/inventory.openapi.json \
  /output:src/Clients/Inventory/InventoryClient.cs \
  /namespace:Contoso.Clients.Inventory
```

### Refitter

Manifest:

```yaml
client:
  language: csharp
  generator: refitter
  output: src/Clients/Inventory
  namespace: Contoso.Clients.Inventory
```

Command shape:

```bash
refitter \
  --openapi openapi/inventory.openapi.json \
  --output src/Clients/Inventory \
  --namespace Contoso.Clients.Inventory
```

### OpenAPI Generator

Manifest:

```yaml
client:
  language: csharp
  generator: openapi-generator
  output: src/Clients/Billing
  packageName: Contoso.Clients.Billing
```

Command shape:

```bash
openapi-generator-cli generate \
  -i openapi/billing.openapi.json \
  -g csharp \
  -o src/Clients/Billing \
  --package-name Contoso.Clients.Billing
```

---

# 13. Multi-Language Design

SpecWatch starts with .NET/C#, but must avoid hard-coding the entire system around C#.

## 13.1 Language Field

Every client definition must include:

```yaml
language: csharp
```

Future values:

```yaml
language: typescript
language: java
```

## 13.2 Generator Compatibility Matrix

SpecWatch should eventually enforce compatibility:

| Language | Kiota | NSwag | Refitter | OpenAPI Generator |
|---|---:|---:|---:|---:|
| C# | Yes | Yes | Yes | Yes |
| TypeScript | Yes | Yes | No | Yes |
| Java | Yes | No | No | Yes |

## 13.3 Future TypeScript Example

```yaml
client:
  language: typescript
  generator: openapi-generator
  output: src/generated/payments-client
  packageName: "@contoso/payments-client"
```

## 13.4 Future Java Example

```yaml
client:
  language: java
  generator: openapi-generator
  output: src/generated/payments-client-java
  packageName: com.contoso.payments
```

---

# 14. Azure DevOps Pipeline Design

SpecWatch should be easy to run inside Azure DevOps Pipelines.

## 14.1 Example Pipeline

```yaml
trigger: none
pr: none

schedules:
  - cron: "0 6 * * *"
    displayName: Daily SpecWatch run
    branches:
      include:
        - main
    always: true

pool:
  vmImage: ubuntu-latest

variables:
  TargetBranch: main
  UpdateBranch: chore/specwatch/openapi-clients

steps:
  - checkout: self
    persistCredentials: true
    fetchDepth: 0

  - task: UseDotNet@2
    displayName: Install .NET SDK
    inputs:
      packageType: sdk
      version: 9.0.x

  - pwsh: |
      dotnet tool restore
    displayName: Restore tools

  - pwsh: |
      git config user.email "specwatch-bot@local"
      git config user.name "SpecWatch Bot"

      git fetch origin $(TargetBranch)
      git checkout -B $(UpdateBranch) origin/$(TargetBranch)
    displayName: Prepare update branch

  - pwsh: |
      specwatch update --manifest specwatch.yml
    displayName: Run SpecWatch
    env:
      PAYMENTS_OPENAPI_TOKEN: $(PAYMENTS_OPENAPI_TOKEN)
      INVENTORY_OPENAPI_TOKEN: $(INVENTORY_OPENAPI_TOKEN)

  - pwsh: |
      if (-not (git status --porcelain)) {
        Write-Host "No changes detected."
        Write-Host "##vso[task.setvariable variable=HasChanges]false"
        exit 0
      }

      Write-Host "Changes detected."
      Write-Host "##vso[task.setvariable variable=HasChanges]true"

      git status --short
      git add .
      git commit -m "chore: update generated OpenAPI clients"
      git push origin HEAD:refs/heads/$(UpdateBranch) --force-with-lease
    displayName: Commit and push changes

  - pwsh: |
      az extension add --name azure-devops --yes

      az devops configure `
        --defaults `
        organization="$(System.CollectionUri)" `
        project="$(System.TeamProject)"

      $existingPr = az repos pr list `
        --repository "$(Build.Repository.Name)" `
        --source-branch "$(UpdateBranch)" `
        --target-branch "$(TargetBranch)" `
        --status active `
        --query "[0].pullRequestId" `
        --output tsv

      if ($existingPr) {
        Write-Host "An active SpecWatch PR already exists: $existingPr"
        exit 0
      }

      az repos pr create `
        --repository "$(Build.Repository.Name)" `
        --source-branch "$(UpdateBranch)" `
        --target-branch "$(TargetBranch)" `
        --title "chore: update generated OpenAPI clients" `
        --description "SpecWatch detected OpenAPI changes and regenerated API clients." `
        --delete-source-branch true
    displayName: Create pull request
    condition: eq(variables.HasChanges, 'true')
    env:
      AZURE_DEVOPS_EXT_PAT: $(System.AccessToken)
```

---

# 15. Pull Request Strategy

## 15.1 Initial Strategy

Initial implementation should support one PR for all changed APIs.

Branch:

```text
chore/specwatch/openapi-clients
```

Title:

```text
chore: update generated OpenAPI clients
```

## 15.2 Future Strategy

Future implementation should support one PR per API.

Example branches:

```text
chore/specwatch/payments
chore/specwatch/inventory
chore/specwatch/billing
```

Example PR titles:

```text
chore: update payments OpenAPI client
chore: update inventory OpenAPI client
```

---

# 16. Report Format

SpecWatch should generate a machine-readable report.

Default path:

```text
specwatch-report.json
```

Example:

```json
{
  "startedAt": "2026-06-06T10:00:00Z",
  "finishedAt": "2026-06-06T10:01:32Z",
  "manifestPath": "specwatch.yml",
  "apis": [
    {
      "name": "payments",
      "changed": true,
      "source": "https://payments.example.com/openapi.json",
      "snapshotPath": "openapi/payments.openapi.json",
      "generator": "kiota",
      "language": "csharp",
      "output": "src/Clients/Payments",
      "generationSucceeded": true,
      "validationSucceeded": true
    },
    {
      "name": "inventory",
      "changed": false,
      "source": "external-specs/inventory.openapi.json",
      "snapshotPath": "openapi/inventory.openapi.json",
      "generator": "nswag",
      "language": "csharp",
      "output": "src/Clients/Inventory/InventoryClient.cs",
      "generationSucceeded": null,
      "validationSucceeded": null
    }
  ],
  "summary": {
    "totalApis": 2,
    "changedApis": 1,
    "generatedClients": 1,
    "failedApis": 0
  }
}
```

---

# 17. Testing Strategy

## 17.1 Unit Tests

Unit tests should cover:

1. Manifest parsing.
2. Manifest validation.
3. Source factory behavior.
4. Local file spec source.
5. HTTP spec source with anonymous auth.
6. HTTP spec source with bearer auth.
7. Hash change detection.
8. Generator command construction.
9. Command runner behavior.
10. Update report creation.

## 17.2 Integration Tests

Integration tests should cover:

1. Running `specwatch validate`.
2. Running `specwatch check`.
3. Running `specwatch update` with a local test spec.
4. Ensuring snapshot files are updated.
5. Ensuring generation commands are invoked through a fake command runner.
6. Ensuring report JSON is written.

## 17.3 Test Fixtures

Recommended fixtures:

```text
tests/
  fixtures/
    specs/
      payments-v1.openapi.json
      payments-v2.openapi.json
      inventory.openapi.yaml

    manifests/
      minimal-valid.yml
      full-valid.yml
      invalid-missing-source.yml
      invalid-generator.yml
```

---

# 18. Coding Standards

## 18.1 .NET Standards

Use:

- modern C#
- nullable reference types
- dependency injection-friendly services
- async APIs for I/O
- clear result objects
- minimal hidden global state

Recommended target framework initially:

```xml
<TargetFramework>net9.0</TargetFramework>
```

If compatibility is more important, use:

```xml
<TargetFramework>net8.0</TargetFramework>
```

## 18.2 Error Handling

Errors should be actionable.

Bad:

```text
Failed.
```

Good:

```text
Manifest validation failed: API 'payments' is missing client.generator.
```

## 18.3 Logging

Initial logging can use console output.

Future logging may use:

```text
Microsoft.Extensions.Logging
```

## 18.4 Configuration

Manifest parsing should be strict enough to catch typos, but extensible enough to allow future fields.

---

# 19. Security Requirements

SpecWatch must:

1. Never log secret values.
2. Never write secret values to report files.
3. Never commit secret values.
4. Treat remote OpenAPI specs as untrusted input.
5. Allow users to pin generator versions externally.
6. Avoid executing commands from downloaded specs.
7. Only execute commands explicitly configured in the manifest.
8. Mask environment-variable-backed auth values in logs.
9. Prefer allowlisted auth types over arbitrary script hooks in early versions.

---

# 20. Roadmap and Sprint Plan

## Sprint 0 — Repository Bootstrap

Status: Completed

Goals:

- Create solution (`SpecWatch.sln`).
- Create the initial projects described in [Section 6](#6-recommended-repository-structure):
  - `src/SpecWatch.Cli`
  - `src/SpecWatch.Core`
  - `src/SpecWatch.AzureDevOps`
  - `tests/SpecWatch.Core.Tests`
  - `tests/SpecWatch.Cli.Tests`
- Add `README.md`, `LICENSE`, and `.gitignore`.
- Establish the target framework and confirm `dotnet build` / `dotnet test` succeed.

## Sprint 1 — Manifest Parsing and Validation

Status: Completed

Goals:

- Implement manifest configuration models (`SpecWatch.Core/Configuration`).
- Parse `specwatch.yml`.
- Implement `specwatch validate` with actionable errors and exit code `3` on failure.
- Add unit tests and manifest fixtures.

## Sprint 2 — OpenAPI Sources and Change Detection

Status: Completed

Goals:

- Implement `IOpenApiSource`, local file and HTTP sources, and the source factory.
- Implement source authentication (anonymous, bearer, header, basic).
- Implement raw-hash change detection ([Section 11.1](#111-sprint-1-mode-raw-hash)).
- Implement `specwatch check` with exit codes `0`/`2`/`1`/`4`.
- Add unit and integration tests.

## Sprint 3 — Generation Orchestration and Reporting

Status: Completed

Goals:

- Implement `ICommandRunner` / `ProcessCommandRunner`.
- Implement `IClientGenerator` and the initial generators (Kiota, NSwag, Refitter, OpenAPI Generator).
- Implement snapshot updates, the update pipeline (`specwatch update`), validation command execution, and JSON report writing.
- Add unit and integration tests with a fake command runner.

## Sprint 4 — Azure DevOps Integration

Status: Completed

Goals:

- Implement the `SpecWatch.AzureDevOps` provider services.
- Implement `specwatch init azure-devops` scaffolding.
- Provide sample manifests and pipelines under `samples/`.

## Future Sprints

- Normalized-hash and semantic change detection ([Sections 11.2–11.3](#112-future-mode-normalized-hash)).
- Multi-language generation (TypeScript, Java) and compatibility enforcement.
- Per-API pull request strategy ([Section 15.2](#152-future-strategy)).
- Additional CI targets (GitHub Actions, GitLab CI, Bitbucket Pipelines).
- Runtime auth DI helper generation.

---

# 21. Session History

Each implementation session must append a new entry here, per [Section 5.2](#52-sprint-based-development) and [Section 5.3](#53-harness-update-requirement).

## Session 1 — 2026-06-06 — Harness Bootstrap

- **Sprint:** Pre-Sprint 0 (harness setup).
- **Implemented:** Added this `AGENTS.md` agent harness / implementation specification at the
  repository root, establishing it as the source of truth for project direction, architecture,
  and sprint planning. Expanded the roadmap with explicit Sprint 0–4 task lists and a Future
  Sprints section, and added this Session History section.
- **Files changed:** `AGENTS.md` (new).
- **Tests added:** None (documentation-only change).
- **Remaining / incomplete:** No source code, solution, or projects exist yet. Sprint 0
  (Repository Bootstrap) has not been started.
- **Suggested next sprint:** Sprint 0 — Repository Bootstrap.

## Session 2 — 2026-06-06 — Sprint 0: Repository Bootstrap

- **Sprint:** Sprint 0 — Repository Bootstrap (Completed).
- **Implemented:** Created the `SpecWatch.sln` solution and the five projects from
  the recommended repository structure (Section 6): `src/SpecWatch.Core`,
  `src/SpecWatch.Cli` (executable assembly name `specwatch`),
  `src/SpecWatch.AzureDevOps`, `tests/SpecWatch.Core.Tests`, and
  `tests/SpecWatch.Cli.Tests`. All projects target `net9.0` with nullable
  reference types and implicit usings enabled. Wired up project references
  (Cli → Core, AzureDevOps → Core, test projects → their targets) and exposed
  CLI internals to its test project via `InternalsVisibleTo`. Added a minimal CLI
  entry point that prints the version (`--version`) and a command summary.
  Added `README.md`, `LICENSE` (MIT), and a .NET `.gitignore`.
- **Files changed:** `SpecWatch.sln` (new), `.gitignore` (new), `LICENSE` (new),
  `README.md` (updated), `src/SpecWatch.Core/SpecWatch.Core.csproj` (new),
  `src/SpecWatch.Core/AssemblyMarker.cs` (new),
  `src/SpecWatch.Cli/SpecWatch.Cli.csproj` (new),
  `src/SpecWatch.Cli/Program.cs` (new),
  `src/SpecWatch.AzureDevOps/SpecWatch.AzureDevOps.csproj` (new),
  `src/SpecWatch.AzureDevOps/AssemblyMarker.cs` (new),
  `tests/SpecWatch.Core.Tests/SpecWatch.Core.Tests.csproj` (new),
  `tests/SpecWatch.Core.Tests/BootstrapTests.cs` (new),
  `tests/SpecWatch.Cli.Tests/SpecWatch.Cli.Tests.csproj` (new),
  `tests/SpecWatch.Cli.Tests/BootstrapTests.cs` (new).
- **Tests added:** Bootstrap tests in both test projects confirming the solution
  builds and the test harness runs (`dotnet test` passes: 2 tests, 0 failures).
- **Build/test commands:** `dotnet build SpecWatch.sln` and
  `dotnet test SpecWatch.sln` both succeed on the .NET 9 / .NET 10 SDK.
- **Remaining / incomplete:** No manifest parsing, sources, change detection,
  generation, reporting, or Azure DevOps logic yet — these begin in Sprint 1.
  The CLI currently only handles `--version` and a help summary.
- **Suggested next sprint:** Sprint 1 — Manifest Parsing and Validation.

## Session 3 — 2026-06-06 — Sprints 1–4: Core, Pipeline, and Azure DevOps

- **Sprints:** Sprint 1, Sprint 2, Sprint 3, Sprint 4 (all Completed).
- **Implemented:**
  - **Sprint 1 — Manifest Parsing and Validation:** Added YamlDotNet-backed
    configuration models (`SpecWatch.Core/Configuration`), `ManifestLoader`,
    `ManifestValidator`/`ManifestValidationResult`, `ExitCodes`, and the
    `specwatch validate` command. Validation covers required fields, supported
    generators/languages, and snapshot/output path collisions (exit code `3`).
  - **Sprint 2 — OpenAPI Sources and Change Detection:** Added `IOpenApiSource`
    with local-file and HTTP sources, `OpenApiSourceFactory`, secret resolution
    by environment-variable name (`ISecretResolver`), raw-hash change detection
    (`HashSpecChangeDetector`), the `SpecWatchRunner.CheckAsync` pipeline, and the
    `specwatch check` command (exit codes `0`/`2`/`1`/`4`).
  - **Sprint 3 — Generation Orchestration and Reporting:** Added
    `ICommandRunner`/`ProcessCommandRunner` (+ `CommandLineParser`), the
    `IClientGenerator` abstraction with Kiota, NSwag, Refitter, and OpenAPI
    Generator implementations plus `ClientGeneratorFactory`, snapshot updates, the
    `SpecWatchRunner.UpdateAsync` pipeline (fetch → detect → write snapshot →
    generate → run validation → write report), JSON report writing
    (`ReportWriter`/`UpdateReport`), and the `specwatch update` command.
  - **Sprint 4 — Azure DevOps Integration:** Implemented `AzureDevOpsOptions`,
    `AzureDevOpsGitService` (prepare branch, commit, push), and
    `AzureDevOpsPullRequestService` (single PR for all changes via `az repos pr`,
    skipping when an active PR exists). Implemented `specwatch init azure-devops`
    scaffolding (writes `specwatch.yml` + `azure-pipelines.yml`, refuses to
    overwrite without `--force`). Added `samples/dotnet-kiota` and
    `samples/dotnet-nswag` with example manifests and pipelines.
- **Files changed:** New code under `src/SpecWatch.Core/{Configuration,Sources,
  ChangeDetection,Generation,Execution,Reporting,Pipeline}`, `src/SpecWatch.Core/
  ExitCodes.cs`, CLI commands under `src/SpecWatch.Cli/Commands`
  (`Validate`/`Check`/`Update`/`Init` + `InitTemplates`) and `CliApplication`,
  Azure DevOps services under `src/SpecWatch.AzureDevOps` (replacing the
  placeholder `AssemblyMarker`), `samples/**`, and tests under
  `tests/SpecWatch.Core.Tests/**` and `tests/SpecWatch.Cli.Tests/**`.
- **Tests added:** 73 tests total (63 in Core.Tests, 10 in Cli.Tests) covering
  manifest load/validation, sources/auth, change detection, generators, the
  update pipeline (with a fake command runner), report writing, the Azure DevOps
  git and PR services, and the `validate`/`init` CLI commands. `dotnet test
  SpecWatch.sln` passes with 0 failures.
- **Build/test commands:** `dotnet build SpecWatch.sln`, `dotnet test SpecWatch.sln`.
- **Security:** Secrets are resolved only by environment-variable name and never
  logged or stored; process execution uses argument lists (no shell) to avoid
  injection (Sections 5.7 and 19).
- **Remaining / incomplete:** Change detection is raw-hash only (normalized-hash
  and semantic diff are future work); generation is C#-only; PR strategy is a
  single PR for all changes; only Azure DevOps CI is scaffolded. The optional
  `docs/` set (manifest/generators/auth/azure-devops/roadmap) was not added.
- **Suggested next sprint:** A "Future Sprints" item — normalized-hash/semantic
  change detection, or multi-language generation with the compatibility matrix.
