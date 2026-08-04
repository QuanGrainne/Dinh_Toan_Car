param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$EfArgs
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not $EfArgs -or $EfArgs.Count -eq 0) {
    Write-Host "Usage: .\ef.ps1 <dotnet-ef-args>"
    Write-Host "Example: .\ef.ps1 migrations list"
    Write-Host "Example: .\ef.ps1 database update"
    exit 1
}

$solutionPath = Join-Path $PSScriptRoot "CarSalesManagementSystem.sln"
$projectPath = Join-Path $PSScriptRoot "DataAccessObjects\DataAccessObjects.csproj"
$startupProjectPath = Join-Path $PSScriptRoot "CarSalesManagementSystemAPI\CarSalesManagementSystemAPI.csproj"
$nugetConfigPath = Join-Path $PSScriptRoot "NuGet.Config"

$env:NUGET_CONFIG_FILE = $nugetConfigPath

Write-Host "Building backend solution with repo NuGet.Config..."
& dotnet build $solutionPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$finalArgs = @($EfArgs)
if ($finalArgs -notcontains "--no-build") {
    $finalArgs += "--no-build"
}

$finalArgs += @(
    "--project", $projectPath,
    "--startup-project", $startupProjectPath
)

Write-Host "Running dotnet ef $($EfArgs -join ' ')..."
& dotnet ef @finalArgs
exit $LASTEXITCODE
