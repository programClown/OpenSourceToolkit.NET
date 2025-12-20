# Future Features - Technical Specifications

This document outlines planned features with detailed technical specifications for future implementation.

---

## 1. AI-Powered Generative Fill / Inpainting

### Overview

Generative Fill (also known as Inpainting) allows users to select a region of an existing image using a mask, and have AI generate new content within that region based on a text prompt. Unlike standard image-to-image generation, this provides precise control over *where*changes occur.**Use Cases:**- Remove unwanted objects from photos

- Extend image backgrounds (outpainting)
- Replace specific elements while preserving the rest
- Fill in missing or damaged areas of images
- Add new objects to specific locations

### API Reference

#### OpenAI Image Edits API**Endpoint:**`POST https://api.openai.com/v1/images/edits`**Request Format:**`multipart/form-data`| Parameter | Type | Required | Description |

|-----------|------|----------|-------------|
|`image`| file | Yes | The original image (PNG, max 4MB, must be square) |
|`mask`| file | Yes | Mask image with transparent areas indicating where to generate |
|`prompt`| string | Yes | Text description of desired content (max 1000 chars) |
|`n`| integer | No | Number of images to generate (1-10, default 1) |
|`size`| string | No |`256x256`, `512x512`, or `1024x1024`(default) |
|`response_format`| string | No |`url`(default) or`b64_json`|
|`user` | string | No | Unique user identifier for abuse monitoring |**Response:**```json
{
  "created": 1589478378,
  "data": [
    {
      "url": "https://...",
      "b64_json": "iVBORw0KGgo..."
    }
  ]
}

```**Important Constraints:**- Both image and mask must be valid PNG files
- Both must be square and same dimensions
- Both must be less than 4MB
- Mask transparent areas (alpha = 0) indicate where to generate new content
- Mask opaque areas are preserved from the original

### Technical Implementation

#### 1. Mask Creation System

The mask is a PNG image where:

-**Transparent pixels (alpha = 0):**AI will generate new content here
-**Opaque pixels (any color):**Original image content is preserved**Mask Painting Tool Requirements:**- Brush tool with adjustable size (1-500px)
- Eraser to remove mask areas
- Clear all / Fill all buttons
- Opacity preview overlay on image
- Feathered edges option for smoother blending

#### 2. Image Padding for Non-Square Images

Since the API requires square images, non-square images must be padded. The TypeScript reference implementation uses**edge reflection**for natural-looking padding:

```text

Original Image (800x600):
┌────────────────┐
│                │
│    Content     │
│                │
└────────────────┘

Padded to 1024x1024 with reflection:
┌─────────────────────────┐
│ ░░░ Reflected Top ░░░   │  ← Vertical reflection
├─────────────────────────┤
│ ░│                  │░  │  ← Horizontal reflection
│ ░│    Original      │░  │
│ ░│    Content       │░  │
│ ░│                  │░  │
├─────────────────────────┤
│ ░░░ Reflected Bot ░░░   │  ← Vertical reflection
└─────────────────────────┘

```**Padding Algorithm (from reference):**```csharp
// For horizontal padding (image narrower than square)
public static void DrawHorizontalReflection(byte[] imageData, int width, int height, int xOffset)
{
    // Left side reflection
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < xOffset; x++)
        {
            int targetIdx = 4*(y*width + x);
            int sourceX = xOffset + (xOffset - x);
            int sourceIdx = 4*(y*width + sourceX);

            imageData[targetIdx] = imageData[sourceIdx];         // R
            imageData[targetIdx + 1] = imageData[sourceIdx + 1]; // G
            imageData[targetIdx + 2] = imageData[sourceIdx + 2]; // B
            imageData[targetIdx + 3] = imageData[sourceIdx + 3]; // A
        }
    }

    // Right side reflection (mirror of left logic)
    for (int y = 0; y < height; y++)
    {
        for (int x = width - 1; x >= width - xOffset; x--)
        {
            int targetIdx = 4*(y*width + x);
            int sourceX = width - 1 - xOffset - (xOffset - (width - x));
            int sourceIdx = 4*(y*width + sourceX);

            imageData[targetIdx] = imageData[sourceIdx];
            imageData[targetIdx + 1] = imageData[sourceIdx + 1];
            imageData[targetIdx + 2] = imageData[sourceIdx + 2];
            imageData[targetIdx + 3] = imageData[sourceIdx + 3];
        }
    }
}
```#### 3. Mask Compositing

When the user paints a mask on a non-square image, the mask must be composited onto the padded canvas:```csharp
public static byte[] CreateCompositeMask(
    byte[] userMask,           // User's painted mask (original dimensions)
    int originalWidth,
    int originalHeight,
    int paddedSize = 1024)     // Target square size
{
    var compositeMask = new byte[paddedSize*paddedSize*4];

    // Fill with opaque (preserve original)
    for (int i = 0; i < compositeMask.Length; i += 4)
    {
        compositeMask[i] = 0;       // R
        compositeMask[i + 1] = 0;   // G
        compositeMask[i + 2] = 0;   // B
        compositeMask[i + 3] = 255; // A = opaque (preserve)
    }

    // Calculate offset for centering
    int xOffset = (paddedSize - originalWidth) / 2;
    int yOffset = (paddedSize - originalHeight) / 2;

    // Copy user mask to correct position
    for (int y = 0; y < originalHeight; y++)
    {
        for (int x = 0; x < originalWidth; x++)
        {
            int srcIdx = 4*(y*originalWidth + x);
            int dstIdx = 4*((y + yOffset)*paddedSize + (x + xOffset));

            compositeMask[dstIdx] = userMask[srcIdx];
            compositeMask[dstIdx + 1] = userMask[srcIdx + 1];
            compositeMask[dstIdx + 2] = userMask[srcIdx + 2];
            compositeMask[dstIdx + 3] = userMask[srcIdx + 3];
        }
    }

    return compositeMask;
}

```#### 4. Result Cropping

After receiving the AI-generated square image, crop back to original dimensions:```csharp

public static byte[] CropToOriginal(
    byte[] paddedImage,
    int paddedSize,
    int originalWidth,
    int originalHeight)
{
    var result = new byte[originalWidth*originalHeight*4];

    int xOffset = (paddedSize - originalWidth) / 2;
    int yOffset = (paddedSize - originalHeight) / 2;

    for (int y = 0; y < originalHeight; y++)
    {
        for (int x = 0; x < originalWidth; x++)
        {
            int srcIdx = 4*((y + yOffset)*paddedSize + (x + xOffset));
            int dstIdx = 4*(y*originalWidth + x);

            result[dstIdx] = paddedImage[srcIdx];
            result[dstIdx + 1] = paddedImage[srcIdx + 1];
            result[dstIdx + 2] = paddedImage[srcIdx + 2];
            result[dstIdx + 3] = paddedImage[srcIdx + 3];
        }
    }

    return result;
}

```### UI/UX Design

#### New Tool Category: "Generative Fill"```text

┌─────────────────────────────────────────────────────────────────┐
│ Tool Categories                                                 │
├─────────────────────────────────────────────────────────────────┤
│ [Output] [Transform] [Resize] [Adjust] [Filters] [Effects]      │
│ [Crop] [Watermark] [Background] [Metadata] [AI] [Gen Fill]      │
└─────────────────────────────────────────────────────────────────┘

```#### Generative Fill Panel```text
┌─────────────────────────────────┐
│ Generative Fill                 │
├─────────────────────────────────┤
│ Brush Size                      │
│ [─────●───────────────] 50px    │
│                                 │
│ Brush Hardness                  │
│ [───────────●─────────] 80%     │
│                                 │
│ [🖌️ Paint] [🧹 Erase] [🗑️ Clear]│
│                                 │
│ ☑ Show mask overlay            │
│ ☐ Feather edges (5px)           │
├─────────────────────────────────┤
│ Prompt                          │
│ ┌─────────────────────────────┐ │
│ │ A beautiful sunset sky      │ │
│ │ with orange clouds          │ │
│ └─────────────────────────────┘ │
│                                 │
│ Variations: [1▼] Size: [1024▼]  │
│                                 │
│ [      Generate Fill      ]     │
├─────────────────────────────────┤
│ Results                         │
│ ┌───┐ ┌───┐ ┌───┐ ┌───┐         │
│ │ 1 │ │ 2 │ │ 3 │ │ 4 │         │
│ └───┘ └───┘ └───┘ └───┘         │
│ [Apply Selected] [Regenerate]   │
└─────────────────────────────────┘
```#### Mask Overlay Visualization

When painting the mask, show a semi-transparent colored overlay:```text
┌─────────────────────────────────────────┐
│                                         │
│     ┌─────────────────────┐             │
│     │ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓ │ ← Red/pink  │
│     │ ▓▓▓ MASK AREA ▓▓▓▓▓ │   overlay   │
│     │ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓ │   (50%      │
│     └─────────────────────┘   opacity)  │
│                                         │
│        Original image visible           │
│        underneath                       │
│                                         │
└─────────────────────────────────────────┘

```### Data Models

#### MaskPaintingState```csharp

public class MaskPaintingState
{
    /// <summary>
    /// Mask bitmap matching workspace image dimensions.
    /// Alpha channel indicates mask: 0 = generate, 255 = preserve
    /// </summary>
    public byte[] MaskData { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    /// <summary>
    /// Current brush size in pixels
    /// </summary>
    public int BrushSize { get; set; } = 50;

    /// <summary>
    /// Brush hardness 0.0-1.0 (affects edge falloff)
    /// </summary>
    public double BrushHardness { get; set; } = 0.8;

    /// <summary>
    /// Whether to feather mask edges before sending to API
    /// </summary>
    public bool FeatherEdges { get; set; } = false;

    /// <summary>
    /// Feather radius in pixels
    /// </summary>
    public int FeatherRadius { get; set; } = 5;
}

```#### GenerativeFillRequest```csharp
public class GenerativeFillRequest
{
    /// <summary>
    /// Original image bytes (will be padded if non-square)
    /// </summary>
    public byte[] ImageData { get; set; }

    /// <summary>
    /// Mask data (transparent = generate, opaque = preserve)
    /// </summary>
    public byte[] MaskData { get; set; }

    /// <summary>
    /// Text prompt describing what to generate
    /// </summary>
    public string Prompt { get; set; }

    /// <summary>
    /// Number of variations to generate (1-10)
    /// </summary>
    public int Count { get; set; } = 1;

    /// <summary>
    /// Output size: 256, 512, or 1024
    /// </summary>
    public int Size { get; set; } = 1024;

    /// <summary>
    /// Original image dimensions (for cropping result)
    /// </summary>
    public int OriginalWidth { get; set; }
    public int OriginalHeight { get; set; }
}
```#### GenerativeFillResult```csharp

public class GenerativeFillResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Generated image variations (already cropped to original dimensions)
    /// </summary>
    public List<byte[]> Images { get; set; }
}

```### Implementation Phases

#### Phase 1: Basic Mask Painting

- [ ] Add`MaskPaintingState`to ViewModel
- [ ] Create mask canvas overlay control
- [ ] Implement brush painting with mouse/touch
- [ ] Add brush size slider
- [ ] Add clear/fill buttons
- [ ] Toggle mask visibility

#### Phase 2: API Integration

- [ ] Add`GenerativeFillRequest`to`ImageGenerationRequest`- [ ] Implement padding algorithm in`ImageProcessor`- [ ] Add`/v1/images/edits`endpoint support to`OpenAiCompatibleProvider`- [ ] Implement result cropping
- [ ] Handle multiple variations

#### Phase 3: Advanced Features

- [ ] Brush hardness (soft edges)
- [ ] Feathered mask edges
- [ ] Undo/redo for mask painting
- [ ] Selection tools (rectangle, lasso)
- [ ] Auto-detect edges option
- [ ] Outpainting (extend canvas beyond original bounds)

#### Phase 4: Provider Support

- [ ] OpenAI (native support)
- [ ] Stability AI (similar API)
- [ ] Google Imagen (if supported)
- [ ] Local models via ComfyUI/Automatic1111

### Error Handling

| Error | Cause | User Message |
|-------|-------|--------------|
| Image too large | File > 4MB | "Image must be under 4MB. Try reducing quality or dimensions." |
| Invalid mask | Mask dimensions mismatch | "Mask must match image dimensions." |
| No mask area | Entire mask is opaque | "Please paint an area to fill." |
| API rate limit | Too many requests | "Please wait a moment before generating again." |
| Content policy | Prompt/image rejected | "Content was flagged. Please modify your prompt." |

### Performance Considerations

1.**Mask rendering:**Use GPU-accelerated canvas for smooth brush strokes
2.**Large images:**Consider downscaling for mask editing, upscale result
3.**Memory:**Dispose intermediate bitmaps promptly
4.**Caching:**Cache padded image if user regenerates with same base

### Security Notes

- Validate mask dimensions match image exactly
- Sanitize prompt text (already handled by existing prompt injection protection)
- Don't expose raw API errors to users (may contain sensitive info)

---*Document Version: 1.0*
*Last Updated: November 2025*
*Status: Planned*## 2. AI image generation options

Ah, my bad! Let me look at what you currently have for AI image gen and create a plan for the missing options.

[6 tools called]

Now I see the full picture. Your`ImageGenerationRequest`already has all the parameters, but**the UI only exposes`Size`and`Quality`**. Here's the detailed plan:

---

## AI Image Generation Options - Implementation Plan

### Current State**API Model (`ImageGenerationRequest`)**- Already has:

- ✅ `Prompt`, `Size`, `Quality`- ✅`Count`(1-10)
- ✅`Style`(vivid/natural)
- ✅`Model`(gpt-image-1, gpt-image-1-mini)
- ✅`AspectRatio`(for OpenRouter/Gemini)
- ✅`Background`(transparent/opaque/auto)
- ✅`OutputFormat`(png/jpeg/webp)
- ✅`OutputCompression`(0-100)
- ✅`Moderation` (low/auto)**UI (`ImageConverterToolViewModel`)**- Only exposes:

- ✅ `ImageGenSize`- ✅`ImageGenQuality`### Missing UI Controls

| Parameter | UI Control | Visibility Condition |
|-----------|------------|---------------------|
|`Count`| NumericUpDown (1-10) | Always |
|`Background`| ComboBox: transparent/opaque/auto | gpt-image-1 only |
|`OutputFormat`| ComboBox: png/jpeg/webp | gpt-image-1 only |
|`OutputCompression`| Slider 0-100 | OutputFormat = jpeg/webp |
|`Style`| ComboBox: vivid/natural | OpenAI DALL-E models |
|`Model`| ComboBox (provider-specific) | When connection has multiple models |
|`AspectRatio`| ComboBox: 1:1, 16:9, etc. | OpenRouter/Gemini |
|`Moderation`| ComboBox: auto/low | gpt-image-1 only |

### Implementation Tasks

#### 1. ViewModel Properties (ImageConverterToolViewModel.cs)```csharp

#region Image Generation Settings

// Existing
public static readonly string[] ImageGenSizeOptions = ...
public static readonly string[] ImageGenQualityOptions = ...
public string ImageGenSize { get; set; }
public string ImageGenQuality { get; set; }

// NEW: Count
private int_imageGenCount = 1;
public int ImageGenCount { get; set; } // 1-10, NotifyCanExecuteChanged if affects button

// NEW: Background transparency
public static readonly string[] ImageGenBackgroundOptions = { "auto", "opaque", "transparent" };
private string_imageGenBackground = "auto";
public string ImageGenBackground { get; set; }

// NEW: Output format
public static readonly string[] ImageGenOutputFormatOptions = { "png", "jpeg", "webp" };
private string_imageGenOutputFormat = "png";
public string ImageGenOutputFormat { get; set; }

// NEW: Compression (only for jpeg/webp)
private int_imageGenCompression = 100;
public int ImageGenCompression { get; set; } // 0-100
public bool IsCompressionEnabled => ImageGenOutputFormat == "jpeg" || ImageGenOutputFormat == "webp";

// NEW: Style (DALL-E specific)
public static readonly string[] ImageGenStyleOptions = { "vivid", "natural" };
private string_imageGenStyle = "vivid";
public string ImageGenStyle { get; set; }

// NEW: Moderation
public static readonly string[] ImageGenModerationOptions = { "auto", "low" };
private string_imageGenModeration = "auto";
public string ImageGenModeration { get; set; }

// NEW: Model selection (populated from connection)
public ObservableCollection<string> ImageGenModelOptions { get; } = new();
private string_imageGenModel;
public string ImageGenModel { get; set; }

// NEW: Aspect Ratio (OpenRouter/Gemini)
public static readonly string[] ImageGenAspectRatioOptions = { "1:1", "16:9", "9:16", "4:3", "3:4", "3:2", "2:3" };
private string_imageGenAspectRatio = "1:1";
public string ImageGenAspectRatio { get; set; }

#endregion

```#### 2. Visibility Helpers (ViewModel)```csharp
// Provider-specific feature detection
public bool ShowBackgroundOption => IsGptImage1Model;
public bool ShowOutputFormatOption => IsGptImage1Model;
public bool ShowCompressionOption => IsGptImage1Model && (ImageGenOutputFormat == "jpeg" || ImageGenOutputFormat == "webp");
public bool ShowStyleOption => IsDallEModel;
public bool ShowModerationOption => IsGptImage1Model;
public bool ShowAspectRatioOption => IsOpenRouterOrGemini;
public bool ShowModelSelection => ImageGenModelOptions.Count > 1;

private bool IsGptImage1Model => ImageGenModel?.StartsWith("gpt-image") ?? false;
private bool IsDallEModel => ImageGenModel?.StartsWith("dall-e") ?? false;
private bool IsOpenRouterOrGemini =>_currentAiConnection?.Provider == "openrouter" ||_currentAiConnection?.Provider == "google";
```#### 3. Update Request Builder```csharp

var request = new ImageGenerationRequest(prompt)
{
    Size = ImageGenSize,
    Quality = ImageGenQuality,
    Count = ImageGenCount,                    // NEW
    Background = ImageGenBackground,          // NEW
    OutputFormat = ImageGenOutputFormat,      // NEW
    OutputCompression = ImageGenCompression,  // NEW
    Style = ImageGenStyle,                    // NEW
    Moderation = ImageGenModeration,          // NEW
    Model = ImageGenModel,                    // NEW
    AspectRatio = ImageGenAspectRatio         // NEW
};

```#### 4. AXAML UI (AI Panel Section)

Add collapsible "Generation Options" expander below the existing Size/Quality dropdowns:```text

┌─────────────────────────────────────┐
│ Generation Options              [▼] │
├─────────────────────────────────────┤
│ Count        [1    ▾]               │
│ Background   [auto ▾]  (gpt-image)  │
│ Format       [png  ▾]  (gpt-image)  │
│ Compression  [====|====] 80%        │
│ Style        [vivid▾]  (DALL-E)     │
│ Moderation   [auto ▾]  (gpt-image)  │
│ Aspect Ratio [1:1 ▾]   (Gemini)     │
└─────────────────────────────────────┘

```#### 5. Cross-Field Validation```csharp
// In property setter for ImageGenBackground
if (value == "transparent" && ImageGenOutputFormat == "jpeg")
{
    ImageGenOutputFormat = "png"; // Auto-switch, JPEG doesn't support transparency
}

// In property setter for ImageGenOutputFormat
if (value == "jpeg" && ImageGenBackground == "transparent")
{
    ImageGenBackground = "opaque"; // Can't have transparent JPEG
}
OnPropertyChanged(nameof(IsCompressionEnabled));
```#### 6. Session Persistence

Add to`ImageEditorSession`:

```csharp
public string ImageGenBackground { get; set; }
public string ImageGenOutputFormat { get; set; }
public int ImageGenCompression { get; set; } = 100;
public string ImageGenStyle { get; set; }
public string ImageGenModeration { get; set; }
public string ImageGenModel { get; set; }
public string ImageGenAspectRatio { get; set; }
public int ImageGenCount { get; set; } = 1;
```#### 7. Handle Multiple Generated Images

When`Count > 1`, add all generated images to thumbnail strip:

```csharp
foreach (var img in response.Images)
{
    AddToThumbnailStrip(img, $"Gen_{thumbnailIndex++}");
}
// Optionally: load first one to workspace, rest go to strip only
```---

### Files to Modify

| File | Changes |
|------|---------|
|`ImageConverterToolViewModel.cs`| Add 8 new properties, visibility helpers, update request builder |
|`ImageConverterToolView.axaml`| Add Generation Options expander with controls |
|`ImageEditorSession.cs`| Add session persistence for new options |
|`ImageEditorSessionManagement.md` | Document new session fields |

### Estimated Scope

-**ViewModel**: ~150 lines
-**AXAML**: ~80 lines
-**Session**: ~20 lines
