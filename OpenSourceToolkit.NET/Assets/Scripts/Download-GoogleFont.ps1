<#
.SYNOPSIS
    Downloads Google Fonts (TTF files) to a local folder.

.DESCRIPTION
    This script downloads font files from the Google Fonts GitHub repository.
    Downloads TTF files that can be installed on Windows.

.PARAMETER FontFamily
    The name of the font family to download (e.g., "Geist Mono", "Roboto", "Open Sans").
    Use the exact name as shown on fonts.google.com.

.PARAMETER OutputPath
    The destination folder for downloaded fonts. Defaults to "..\Fonts" relative to script location.

.EXAMPLE
    .\Download-GoogleFont.ps1 -FontFamily "Geist Mono"

.EXAMPLE
    .\Download-GoogleFont.ps1 -FontFamily "Roboto" -OutputPath "C:\Fonts"

.EXAMPLE
    .\Download-GoogleFont.ps1 -FontFamily "Open Sans"
#>

param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$FontFamily,

    [Parameter(Mandatory = $false)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

# Default output path to Fonts folder relative to script
if (-not $OutputPath) {
    $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $OutputPath = Join-Path $ScriptDir "..\Fonts"
}

# Ensure output directory exists
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    Write-Host "Created output directory: $OutputPath" -ForegroundColor Green
}

$OutputPath = Resolve-Path $OutputPath

Write-Host "Downloading font family: $FontFamily" -ForegroundColor Cyan

# Convert font name to GitHub folder format (lowercase, no spaces)
# e.g., "Geist Mono" -> "geistmono", "Open Sans" -> "opensans"
$FolderName = ($FontFamily -replace ' ', '').ToLower()

# GitHub API to list files in the font folder
$GithubApiUrl = "https://api.github.com/repos/google/fonts/contents/ofl/$FolderName"

Write-Host "  Checking GitHub: $GithubApiUrl" -ForegroundColor DarkGray

try {
    $Headers = @{
        "User-Agent" = "PowerShell-GoogleFonts-Downloader"
        "Accept" = "application/vnd.github.v3+json"
    }

    $Response = Invoke-RestMethod -Uri $GithubApiUrl -Headers $Headers -Method Get

    # Filter for TTF files, exclude variable fonts (contain [wght] or [axis] in name)
    $TtfFiles = $Response | Where-Object { $_.name -match '\.ttf$' -and $_.name -notmatch '\[' }

    if ($TtfFiles.Count -eq 0) {
        Write-Host "  No static TTF files found, checking static folder..." -ForegroundColor Yellow
        # Most fonts have static TTFs in a "static" subfolder
        $StaticApiUrl = "https://api.github.com/repos/google/fonts/contents/ofl/$FolderName/static"
        try {
            $StaticResponse = Invoke-RestMethod -Uri $StaticApiUrl -Headers $Headers -Method Get
            $TtfFiles = $StaticResponse | Where-Object { $_.name -match '\.ttf$' -and $_.name -notmatch '\[' }
        } catch {
            # No static folder, fall back to variable font if available
            $TtfFiles = $Response | Where-Object { $_.name -match '\.ttf$' }
            if ($TtfFiles.Count -gt 0) {
                Write-Host "  Only variable font available (contains all weights in one file)" -ForegroundColor Yellow
            }
        }
    }

    if ($TtfFiles.Count -eq 0) {
        Write-Host "Error: No TTF files found for '$FontFamily'" -ForegroundColor Red
        Write-Host "The font may use a different folder name or not be available." -ForegroundColor Yellow
        exit 1
    }

    Write-Host "  Found $($TtfFiles.Count) TTF file(s)" -ForegroundColor Green

    $DownloadCount = 0
    foreach ($File in $TtfFiles) {
        $FileName = $File.name
        $DownloadUrl = $File.download_url
        $DestPath = Join-Path $OutputPath $FileName

        Write-Host "  Downloading: $FileName" -ForegroundColor White

        $WebClient = New-Object System.Net.WebClient
        $WebClient.Headers.Add("User-Agent", "PowerShell-GoogleFonts-Downloader")
        $WebClient.DownloadFile($DownloadUrl, $DestPath)
        $WebClient.Dispose()

        $DownloadCount++
    }

    Write-Host ""
    Write-Host "Download complete! $DownloadCount font file(s) saved to: $OutputPath" -ForegroundColor Green

} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "Error: Font '$FontFamily' not found in Google Fonts repository." -ForegroundColor Red
        Write-Host "Tried folder: ofl/$FolderName" -ForegroundColor Yellow
        Write-Host "Check the exact font name on fonts.google.com" -ForegroundColor Yellow
    } else {
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
    exit 1
}
