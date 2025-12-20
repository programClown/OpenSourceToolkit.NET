# OpenSourceToolkit.Net

A comprehensive C# .NET Framework 4.7.2 port of the utilities and tools found in the main **[OpenSourceToolkit](https://github.com/truethari/OpensourceToolkit/)** project. **Special thanks to the original author [truethari](https://github.com/truethari) for the incredible work.**

This project is a **pure AI-driven port** executed using **Gemini 3 Pro** and **GPT-5.1**.

This suite provides reusable, modular components for text manipulation, security, networking, hardware testing, and more. Crucially, it includes a full **Avalonia UI application** that allows users to interactively test and verify all functions of the **25+ separate tools** and libraries in a modern desktop interface.

## 🚀 Overview

- **Framework**: .NET Framework 4.7.2
- **Language**: C# 7.3
- **Output**: Reusable DLLs (Class Libraries) and Console Applications.
- **Architecture**: Modular design with a core library and specialized domain libraries.

## 📁 Project Structure

The solution `OpenSourceToolkit.Net.sln` is organized into the following projects:

### **OpenSourceToolkit.NET**

- **Full GUI Application** to test all 25+ tools interactively.
- Dependencies: All of the below, `Avalonia`

| Project | Description | Dependencies |
|---------|-------------|--------------|
| **OpenSourceToolkit.Core** | Shared primitives and base types. | None |
| **OpenSourceToolkit.TextData** | Generators for UUIDs, Lorem Ipsum, Mock Data, QR Codes, Privacy Policies, VCards, Regex Testing. | `Bogus`, `QRCoder` |
| **OpenSourceToolkit.Converters** | Utilities for Timestamp, Image Format, Text Case, Base64, and Ethereum conversions. | `System.Drawing.Common` |
| **OpenSourceToolkit.Security** | Security tools for JWT, HMAC, Hashing (MD5/SHA), and Password Generation. | `System.IdentityModel.Tokens.Jwt` |
| **OpenSourceToolkit.Networking** | Tools for IP Geolocation, Speed Testing, DNS Lookups, Uptime Monitoring, and IP Subnet Calculations. | `DnsClient` |
| **OpenSourceToolkit.Scheduling** | Cron job expression parsing and scheduling. | `NCrontab` |
| **OpenSourceToolkit.ApiTesting** | HTTP API testing framework with request/response assertions. | None |
| **OpenSourceToolkit.IO** | File system tools, including folder structure analysis. | None |
| **OpenSourceToolkit.Documents** | PDF manipulation tools (Merge, Split, Watermark). | `PdfSharp` |
| **OpenSourceToolkit.Media** | ASCII Art generation, Next.js Image URL parsing, and media utilities. | `FIGLet` |
| **OpenSourceToolkit.Colors** | Color format converters (HEX, RGB, HSL). | `System.Drawing.Common` |
| **OpenSourceToolkit.Hardware** | Hardware testing abstractions for Keyboard, Speaker, Mic, and Camera. | `NAudio` |
| **OpenSourceToolkit.Calculators** | Financial calculators (Compound Interest, Loan Payments, ROI). | None |
| **OpenSourceToolkit.Demo** | Console application demonstrating usage of the libraries. | All of the above |
| **OpenSourceToolkit.Tests** | MSTest unit tests for ensuring parity and correctness. | MSTest |

## 🛠️ Building & Running

### Prerequisites

- .NET SDK (capable of targeting .NET Framework 4.7.2)
- Windows OS (recommended for Hardware/Audio/System.Drawing dependencies)

### Build

To build the entire solution, run the following command in this directory:

```bash
dotnet build
```

**Note**: The build is configured via `Directory.Build.props` to output all artifacts to a common directory:

- **Debug**: `bin\Debug\net472\`
- **Release**: `bin\Release\net472\`

### Run Avalonia App (GUI)

The best way to explore the toolkit is via the Avalonia UI app, which provides a dedicated interface for every tool:

```bash
.\bin\Debug\net472\OpenSourceToolkit.NET.exe
```

### Run Demo (Console)

You can also run the console demo application:

```bash
.\bin\Debug\net472\OpenSourceToolkit.Demo.exe
```

### Run Tests

To execute the 48 test cases in the unit tests:

```bash
dotnet test
```

## 📦 Usage Examples

### Text & Data

```csharp
using OpenSourceToolkit.TextData;

// Generate UUID V4
string uuid = UuidGenerator.GenerateV4();

// Generate Lorem Ipsum
var generator = new LoremIpsumGenerator();
string text = generator.GenerateSentences(3);
```

### Security

```csharp
using OpenSourceToolkit.Security;

// Compute Hash
string md5 = HashGenerator.ComputeMd5("OpenSourceToolkit");

// Generate JWT
string token = JwtHelper.GenerateToken("secret_key", "issuer", "audience");
```

### Networking

```csharp
using OpenSourceToolkit.Networking;

// Check Uptime
var monitor = new UptimeMonitor();
var result = await monitor.CheckAsync("https://google.com");
Console.WriteLine($"Is Up: {result.IsUp}");
```

### Hardware (Windows)

```csharp
using OpenSourceToolkit.Hardware;

// Play Tone
using (var speaker = new SpeakerTester())
{
    speaker.PlayTone(440, 1.0f); // 440Hz for 1 second
}
```

## 🧩 Dependencies

The project relies on high-quality open-source packages:

- **[Bogus](https://github.com/bchavez/Bogus)**: Fake data generation.
- **[QRCoder](https://github.com/codebude/QRCoder)**: QR Code creation.
- **[PdfSharp](http://www.pdfsharp.net/)**: PDF processing.
- **[DnsClient.NET](https://github.com/MichaCo/DnsClient.NET)**: DNS lookups.
- **[NCrontab](https://github.com/atifaziz/NCrontab)**: Cron parsing.
- **[NAudio](https://github.com/naudio/NAudio)**: Audio playback and capture.
- **[Avalonia](https://github.com/AvaloniaUI/Avalonia)**: Cross-platform UI framework.
- **[Flowery.NET](https://github.com/tobitege/Flowery.NET)**: DaisyUI component library for Avalonia. **Requires v1.0.9 or later** (for `CustomThemeApplicator` support).

## 🌍 Localization

The application supports multiple languages with runtime switching. Currently supported:

- **English (en-US)** - Default
- **German (de-DE)** - Full translation

### For Users

Change the application language through the settings panel. All UI elements will update immediately without restart.

### For Developers

#### Using Localization in XAML

Add the localization namespace to your view:

```xml
xmlns:loc="clr-namespace:OpenSourceToolkit.NET.Localization"
```

Use the `Localize` markup extension:

```xml
<TextBlock Text="{loc:Localize Button_Save}"/>
<Button Content="{loc:Localize Button_Generate}"/>
<TextBox Watermark="{loc:Localize Input_Placeholder}"/>
```

#### Adding New Translations

1. Add resource key to `ToolkitStrings.resx` (English)
2. Add translation to `ToolkitStrings.de.resx` (German)
3. Use the key in XAML via `{loc:Localize YourKey}`

#### Implementation Notes

**Critical**: The `ToolkitLocalization.SetCulture()` method sets culture at multiple levels to ensure `ResourceManager` respects culture changes:

```csharp
Thread.CurrentThread.CurrentUICulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
CultureInfo.DefaultThreadCurrentCulture = culture;
_resourceManager.ReleaseAllResources();
```

This pattern is **essential** for reliable runtime culture switching. Without `DefaultThreadCurrentUICulture`, the ResourceManager may cache strings from the initial system culture even after calling `ReleaseAllResources()`.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🧪 Tests

The solution includes a comprehensive test suite in `OpenSourceToolkit.Tests` using **MSTest**. These tests verify the functionality of the ported libraries and ensure parity with the original tools.

### Coverage Areas

- **Text & Data**:
  - UUID V4 generation (format, uniqueness).
  - Lorem Ipsum generation (word/sentence counts).
  - Mock Data (User/Address schema validation).
  - QR Code (PNG and SVG generation).
  - Privacy Policy (template substitution).
- **Security**:
  - Hashing (MD5, SHA256, SHA512 correctness).
  - HMAC generation (RFC compliance).
  - JWT (Token generation, signing, and validation).
- **Converters**:
  - Timestamp (Unix epoch round-trips).
  - Base64 (Standard and URL-safe variants).
  - Text Case (Title Case, Sentence Case).
  - Color (Hex/RGB/HSL conversions).
- **IO & Documents**:
  - Folder Analyzer (Recursive structure and size calculation).
  - PDF Toolkit (Merge, Split, Watermark functionality).
- **Media**:
  - ASCII Art (Bitmap to ASCII conversion logic).
- **Scheduling**:
  - Cron Scheduler (Expression parsing and execution).
- **Hardware**:
  - Keyboard Tester (Typing speed algorithms).
  - Audio Device Manager (Device enumeration safety).
- **Networking**:
  - DNS lookup tools (direct and batched queries for common record types).
  - IP geolocation (dummy provider default mapping).

### Running Tests

You can execute the full test suite via the command line:

```bash
dotnet test
```

Tests are designed to be safe and isolated, using temporary files for IO operations and simulating hardware interactions where necessary to run on CI/CD environments.
