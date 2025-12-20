<#
Builds the OpenSourceToolkit.NET Avalonia desktop app (and optionally the test project).

Usage:
  pwsh ./scripts/build_desktop.ps1
  pwsh ./scripts/build_desktop.ps1 -Configuration Release
  pwsh ./scripts/build_desktop.ps1 -IncludeTests
  pwsh ./scripts/build_desktop.ps1 -NoRestore
#>
param(
    [string]$Configuration = "Debug",
    [switch]$IncludeTests,
    [switch]$NoRestore
)

Clear-Host

$ErrorActionPreference = "Stop"
$script:buildResults = @()
$script:startTime = Get-Date

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)][string]$Title,
        [Parameter(Mandatory = $true)][string]$Command
    )

    Write-Host $Title -ForegroundColor Cyan
    Write-Host "  $Command"
    $stepStart = Get-Date
    Invoke-Expression $Command
    $stepDuration = (Get-Date) - $stepStart

    if ($LASTEXITCODE -ne 0) {
        $script:buildResults += [PSCustomObject]@{ Project = $Title; Status = "FAILED"; Duration = $stepDuration }
        Write-Host "FAILED: $Title" -ForegroundColor Red
        exit 1
    }
    $script:buildResults += [PSCustomObject]@{ Project = $Title; Status = "OK"; Duration = $stepDuration }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

$desktopProject = Join-Path $repoRoot "OpenSourceToolkit.NET/OpenSourceToolkit.NET.csproj"
$testsProject = Join-Path $repoRoot "OpenSourceToolkit.Tests/OpenSourceToolkit.Tests.csproj"

if (-not (Test-Path $desktopProject)) {
    Write-Host "ERROR: Desktop project not found at $desktopProject" -ForegroundColor Red
    exit 1
}

$noRestoreArg = if ($NoRestore) { " --no-restore" } else { "" }

Invoke-Step "Build: OpenSourceToolkit.NET (desktop)" "dotnet build `"$desktopProject`" -c $Configuration$noRestoreArg"

if ($IncludeTests) {
    if (-not (Test-Path $testsProject)) {
        Write-Host "ERROR: Tests project not found at $testsProject" -ForegroundColor Red
        exit 1
    }
    Invoke-Step "Build: OpenSourceToolkit.Tests" "dotnet build `"$testsProject`" -c $Configuration$noRestoreArg"
}

$totalDuration = (Get-Date) - $script:startTime

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host " BUILD SUMMARY" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
foreach ($result in $script:buildResults) {
    $statusColor = if ($result.Status -eq "OK") { "Green" } else { "Red" }
    $duration = $result.Duration.ToString("mm\:ss\.ff")
    Write-Host ("  [{0}] {1,-40} {2}" -f $result.Status, $result.Project, $duration) -ForegroundColor $statusColor
}
Write-Host ""
Write-Host ("  Total time: {0:mm\:ss\.ff}" -f $totalDuration) -ForegroundColor Cyan
Write-Host ("  Projects built: {0}" -f $script:buildResults.Count) -ForegroundColor Cyan
Write-Host ""
Write-Host "All builds completed successfully." -ForegroundColor Green


