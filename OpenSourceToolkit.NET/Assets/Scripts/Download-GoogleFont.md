# Download-GoogleFont.ps1

A PowerShell script to download Google Fonts (TTF files) from the official GitHub repository.

## Usage

```powershell
.\Download-GoogleFont.ps1 -FontFamily "<FontName>" [-OutputPath "<path>"]
```

## Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `-FontFamily` | Yes | Font name exactly as shown on [fonts.google.com](https://fonts.google.com) |
| `-OutputPath` | No | Destination folder. Defaults to `..\Fonts` |

## Examples

### Geist Mono (Monospace font for code)

From: <https://fonts.google.com/specimen/Geist+Mono>

```powershell
.\Download-GoogleFont.ps1 -FontFamily "Geist Mono"
```

Output: `GeistMono[wght].ttf` (variable font containing all weights)

### Other fonts

```powershell
# Roboto
.\Download-GoogleFont.ps1 -FontFamily "Roboto"

# Inter to custom folder
.\Download-GoogleFont.ps1 -FontFamily "Inter" -OutputPath "C:\Fonts\Inter"
```

## How it works

1. Converts font name to GitHub folder format (e.g., "Geist Mono" → `geistmono`)
2. Queries the [google/fonts](https://github.com/google/fonts) repository via GitHub API
3. Prefers static TTF files (individual weights) if available in `static/` subfolder
4. Falls back to variable font (`[wght].ttf`) if no static files exist

## Variable Fonts

Some fonts (like Geist Mono) only provide a **variable font** file with `[wght]` in the name.
This single file contains all font weights and is fully installable on Windows.

## Installing Fonts on Windows

To install a downloaded TTF file:

1. Right-click the `.ttf` file
2. Select **Install**
3. Follow the instructions in the dialog

## Notes

- Downloads TTF format (installable on Windows, macOS, Linux)
- Font family names must match Google Fonts exactly (case-insensitive)
- Creates the output directory if it doesn't exist
