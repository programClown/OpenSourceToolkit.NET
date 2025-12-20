# OpenSourceToolkit.NET Tool Manual

Feature-level overview for tools under `Views/Tools`. Listed alphabetically.

## Api Tester

- Choose HTTP method and target URL, add headers/body, and send requests with a single click.
- Supports Bearer tokens, Basic auth, or API key (header/query) authentication.
- Shows response status code and duration with tabs for body and headers.
- Handles loading and empty states to clarify when no response is available.

## Ascii Art

- Load an image from disk and convert it to ASCII art.
- Adjust font size (zoom) for readability, copy the output, or clear the result.

## Audio Noise Reduction

- Windows-only audio workflow with device selection, refresh, and live peak meter for monitoring input.
- Record microphone input or import an audio file, with status and recording time display.
- Processing tabs cover gain and high/low-pass filters, compressor (threshold/ratio/attack/release), and EQ (bass/mid/treble).
- Playback original vs processed audio, stop playback, and export in multiple formats with MP3 bitrate selection; reset settings when needed.

## Base64

- Encode or decode text to and from Base64 in a simple two-panel view.

## Clipboard Image Saver

- Paste images from the clipboard, view dimensions/format/size, and keep a history of captures with delete support.
- Select output format and quality when applicable.
- Optional resize controls with width/height inputs and aspect-ratio lock.
- Download the processed image or clear the workspace; notifications confirm actions.

## Color Tool

- Pick a color via palette or hex input (with randomize) and preview it immediately.
- Copy-ready values for Hex, RGB, and HSL plus conversions to HSV, CMYK, and LAB; fine-tune with RGB sliders.
- Generate palettes by harmony type and explore shade strips.
- Build linear gradients and copy CSS snippets.
- Accessibility tab checks contrast for two foreground choices against the background.

## Cron Tool

- Quick presets for common cron expressions.
- Manual builder for minute, hour, day, month, and weekday fields with dropdown presets.
- Validates cron input, showing friendly schedule descriptions.
- Lists upcoming occurrences with adjustable count controls.

## Diff Checker

- Compare two text blocks and render side-by-side diffs.
- Color cues highlight inserted, deleted, and modified lines for the old and new text.

## DNS Tool

- Run DNS lookups for a domain and inspect results in a copyable output area.

## Eth Converter

- Enter an ETH amount and convert it to other Ethereum denominations with a single action.

## Financial Calculator

- Compound Interest tab for growth over time.
- Loan Payment tab to compute installments from principal, rate, and term.
- Investment tab models recurring contributions over years.
- Deposits & Savings tab includes fixed-deposit maturity (with compounding options), CD ladder planning, and APR-to-APY conversion.
- ROI calculator for basic return math.
- German calculators for Baufinanzierung (annuity loan) and Festgeld deposits.

## Folder Analyzer

- Browse to a folder and trigger analysis with live progress.
- Supports canceling the scan and shows a textual summary of findings.

## Fonts Viewer

- Browse Google Fonts with search, category filter, and tag chips; refresh the catalog and view status.
- Inspect a family’s details (variants, version, category) and copy the font name or open its Google Fonts page.
- Preview text with adjustable size, optional live font download, and dark/light background toggle; shows sample sizes.
- Manage downloaded cache (open folder/clear) and download selected or all font files with progress indicators.
- List available file assets with sizes and remind about OFL/Apache licensing.

## Hardware Tool

- Run a simple speaker test that plays a 440 Hz sine tone, with a platform warning if audio is unsupported.

## Hash Tool

- Generate MD5 and SHA-256 hashes for provided text, with copyable outputs.

## HMAC Tool

- Create HMACs (SHA-256 and SHA-512) from a message and secret key (masked input) and copy the results.

## Image Converter

- Single Image workspace supports opening, saving, copying, undo, reset, zoom/pan, histogram display, and before/after comparison slider.
- Session manager stores thumbnail strips and AI chat history; thumbnail strip aids quick navigation.
- Tool categories: output format/quality (ICO multi-size presets), rotate/flip, resize with aspect lock, brightness/contrast/saturation, grayscale/sepia/invert filters, blur and sharpen sliders, effects (auto-enhance, vignette radius/softness, posterize levels, edge detection), crop with ratios or manual values, watermark text or image (position/opacity/size/color/padding), background removal with replacement color and tolerance, and metadata stripping.
- AI assistant tab selects a connection (with image-gen size/quality when available), keeps chat history with save/copy/clear, adjusts font size, and lets you send or abort prompts about the image.
- Batch Convert tab imports multiple images, shows thumbnails/status/errors, and converts them with chosen format/quality, resize rules, metadata stripping, ICO multi-size presets, and custom rename patterns ({name},{ext},{width},{height},{date},{time},{index},{format}).
- Batch extras: create animated GIFs (frame delay, loop, optimize) and PDF operations (build a PDF from images or extract PDF pages to images with selectable DPI).

## Image Fullscreen Viewer

- Borderless, topmost image viewer with scroll-to-zoom, drag-to-pan, and on-screen hints; close with Esc or Enter.

## IP Calculator

- Calculate network details from an IP address and subnet mask and display them in a copyable output.

## IP Location

- Look up geolocation details for an IP address and present them in a wrapped, copyable view.

## JSON Formatter

- Accepts JSON, XML, or YAML input (paste or load from file) and shows size readouts.
- Format/Beautify tab pretty-prints in the chosen format; Minify tab compresses payloads.
- Convert tab translates between JSON/XML/YAML with selectable input/output formats.
- Shows errors inline and includes a short format guide.

## JWT Tool

- Generate JWTs using a secret and issuer.
- Paste or create encoded tokens, decode them, and view header and payload panes with copyable text.

## Keyboard Tester

- Key Events tab starts/stops monitoring keystrokes, showing current key/code/modifiers, totals, unique count, most-pressed key, and a logged table of events.
- Typing Test tab offers preset or continuous text sessions with live WPM, accuracy, wrong-word counts, and session aggregates; includes reset and clear controls.

## Lorem Ipsum

- Generate placeholder text as words, sentences, or paragraphs for a chosen count and copy the result.

## Markdown Editor

- Editor with undo/redo, open/save, clear, adjustable font size, and HTML preview toggle (tables/task-list options).
- Quick actions to insert formatting (bold, italic, code, links, headings, lists, quotes) and load templates.
- Optional linting with run/auto-fix, violation list, and navigation to issues.
- Built-in Markdown guide covering headers, emphasis, lists, quotes, code, tables, and links.

## Mock Data

- Produce JSON mock data for selected data types/locales with count control up to 100 items.
- Outputs ready-to-copy structured results.

## Next.js Image Decoder

- Decode a Next.js image URL (with an example loader) and display the decoded output.

## Password Generator

- Configure length, character sets (uppercase/lowercase/numbers/symbols), and exclusions for similar or ambiguous characters.
- Generate and copy secure passwords instantly.

## PDF Tool

- Merge: add multiple PDFs and create a combined output file.
- Split: select a PDF and output directory to break it into individual pages.
- Watermark: apply text watermarks to every page and save to a chosen file.
- Status bar surfaces operation progress or messages.

## Privacy Policy

- Capture company name, website, and contact email plus checkboxes for personal data, cookies, analytics, third-party services, and user accounts.
- Toggle GDPR and CCPA sections and choose Markdown output if desired.
- Generate a templated privacy policy with a built-in non-legal-advice disclaimer.

## QR Code

- Generate QR codes for text/URLs, email, or Wi-Fi payloads with selectable error correction and output format.
- Shows PNG preview and SVG source; copy either format to the clipboard.

## Regex Tester

- Enter a regex with ignore-case, multiline, or single-line options and test against sample text.
- Displays match values with index/length and captured groups.
- Replacement field applies substitutions and shows the result.
- Load sample patterns from an examples list and surface regex errors inline.

## Speed Test

- Run or stop a bandwidth test that transfers data to/from httpbin.org and tracks current stage and progress.
- Report ping, download, and upload speeds in result cards.
- Maintain a history of past runs with clear control and contextual info about methodology and accuracy.
- Surface errors in a dedicated banner.

## SQL Formatter

- Choose SQL dialect and indent size; toggles for uppercasing keywords and placing newlines before keywords.
- Actions to format, minify, clear, and load template queries.
- Copyable input/output panes with error and warning banners plus optional optimization tips.
- Collapsible SQL reference for common statements, joins, and functions.

## Stopwatch & Timer

- Stopwatch: start, pause, lap, reset, and clear laps with large time display and lap duration/total tracking.
- Timer: set hours/minutes/seconds or use quick presets, with progress bar, start/pause/stop controls, finished indicator, and settings for sound and auto-restart.

## Text Case

- Convert text into upper case, lower case, title case, and sentence case with copyable outputs.

## Timestamp

- Pick a local date/time and view it; convert Unix seconds to human-readable date/time through the converter.

## Uptime

- Check a URL’s availability and display status text, HTTP status code, and response time in milliseconds.

## UUID

- Generate one or many GUIDs/UUIDs in selectable formats and copy the generated list.

## VCard Generator

- Enter personal, work, and address fields (or use test data) to generate a vCard payload.
- Outputs copy-ready vCard text containing contact details, organization info, and address.
