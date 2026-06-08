namespace SpecWatch.Cli.Commands;

/// <summary>
/// Embedded scaffolding content for <c>specwatch init azure-devops</c>
/// (see AGENTS.md, Sections 8 and 14.1).
/// </summary>
internal static class InitTemplates
{
    public const string SpecWatchManifest =
"""
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

    validation:
      commands:
        - dotnet build
        - dotnet test

""";

    public const string AzurePipeline =
"""
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

""";
}
