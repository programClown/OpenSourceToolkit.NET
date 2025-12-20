<#
.SYNOPSIS
    Builds and starts the OpenSourceToolkit.NET Avalonia desktop application.

.DESCRIPTION
    Builds OpenSourceToolkit.NET and, if the resulting .exe exists, starts it in the background.

.PARAMETER Configuration
    Build configuration to use (default: Debug).

.EXAMPLE
    pwsh ./scripts/run_desktop.ps1
    pwsh ./scripts/run_desktop.ps1 -Configuration Release
#>
param(
    [string]$Configuration = "Debug"
)

Clear-Host

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$desktopProject = Join-Path $repoRoot "OpenSourceToolkit.NET/OpenSourceToolkit.NET.csproj"

if (-not (Test-Path $desktopProject)) {
    Write-Host "ERROR: Desktop project not found at $desktopProject" -ForegroundColor Red
    exit 1
}

Write-Host "Building: OpenSourceToolkit.NET" -ForegroundColor Cyan
Write-Host "  Configuration: $Configuration" -ForegroundColor Gray

dotnet build "$desktopProject" -c $Configuration

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

[xml]$desktopProjectXml = Get-Content -Raw $desktopProject
$tfm = ($desktopProjectXml.Project.PropertyGroup | Where-Object { $_.TargetFramework } | Select-Object -First 1).TargetFramework

$projectName = [System.IO.Path]::GetFileNameWithoutExtension($desktopProject)

$configurationFolder = if ($Configuration -eq "Debug") { "debug" } elseif ($Configuration -eq "Release") { "release" } else { $Configuration.ToLowerInvariant() }

# This repo overrides Debug output path to a common root-level bin folder:
#   ..\bin\debug\<tfm>\OpenSourceToolkit.NET.exe
$candidateOutputDirs = @(
    (Join-Path $repoRoot "bin/$configurationFolder/$tfm"),
    (Join-Path $repoRoot "OpenSourceToolkit.NET/bin/$Configuration/$tfm")
) | Select-Object -Unique

$exePath = $null
$workingDir = $null
foreach ($dir in $candidateOutputDirs) {
    $candidateExe = Join-Path $dir "$projectName.exe"
    if (Test-Path $candidateExe) {
        $exePath = $candidateExe
        $workingDir = $dir
        break
    }
}

if (-not $exePath) {
    Write-Host "Desktop executable not found. Checked:" -ForegroundColor Yellow
    foreach ($dir in $candidateOutputDirs) {
        Write-Host "  - $(Join-Path $dir "$projectName.exe")" -ForegroundColor Yellow
    }
    exit 0
}

Write-Host "Starting Desktop app in background..." -ForegroundColor Cyan
Start-Process -FilePath $exePath -WorkingDirectory $workingDir | Out-Null
Write-Host "Started: $exePath" -ForegroundColor Green


