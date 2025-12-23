# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.1] - 2025-12-23

### Changed

- Updated Avalonia packages from 11.3.9 to 11.3.10 (required by Flowery.NET).
- Updated System.Text.Json from 10.0.0 to 10.0.1.
- Updated App_Title in all localization files to include ".NET" suffix.

### Fixed

- Fixed deprecation warning in `AsciiArtGenerator.cs` by replacing `SKFilterQuality` with `SKSamplingOptions`.
- Fixed CA1416 platform compatibility warnings in `ImageConverter.cs` by adding `[SupportedOSPlatform("windows")]` attribute.
- Fixed Scientific Calculator: Enter key now always triggers calculation instead of re-activating the last clicked button.
- Fixed Scientific Calculator: Unary minus was being ignored, causing negative results to become positive in subsequent calculations (e.g., `-1522 + 2000` incorrectly returned `3517` instead of `478`).
- App title in all localizations now state ".NET" at the end

## [0.1.0] - 2025-12-20

### Added

- Initial public release.
