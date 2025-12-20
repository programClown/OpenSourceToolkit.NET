# Refactoring Complete: `Semi.Daisy.Theming`

This theme functionality has been extracted into its own reusable assembly.

## Rationale

1. **Separation of Concerns**: Theme conversion, color space math (OKLCH→RGB), and Semi.Avalonia resource management are now distinct from the main app's business logic.

2. **Reusability**: Other Avalonia projects can reference this library to integrate DaisyUI themes without copying code.

3. **Testability**: Isolated color conversion and CSS parsing can be unit tested independently.

4. **Documentation Co-location**: The detailed theme documentation now lives with the code it describes.

---

### Project Structure

```directory
dotnet/
├── Semi.Daisy.Theming/
│   ├── Semi.Daisy.Theming.csproj
│   │
│   ├── DaisyUI/
│   │   ├── DaisyUiTheme.cs              # Theme data model
│   │   ├── DaisyUiCssParser.cs          # CSS parsing logic
│   │   ├── DaisyUiAxamlGenerator.cs     # AXAML ResourceDictionary generation
│   │   └── EmbeddedThemes/              # Embedded .css resources (36 themes)
│   │       ├── corporate.css
│   │       ├── dracula.css
│   │       └── ... (all themes)
│   │
│   ├── ColorSpaces/
│   │   ├── OklchColor.cs                # OKLCH color struct with parsing
│   │   └── ColorConverter.cs            # OKLCH↔RGB↔Hex conversions
│   │
│   ├── Semi/
│   │   ├── SemiResourceKeys.cs          # Constants for all Semi resource key names (~180 keys)
│   │   ├── SemiThemeMapping.cs          # DaisyUI→Semi mapping dictionary
│   │   └── SemiThemeApplicator.cs       # Runtime theme application to Avalonia.Application
│   │
│   ├── Services/
│   │   ├── IThemeService.cs             # Interface for theme operations
│   │   └── ThemeService.cs              # Implementation (load embedded themes, apply)
│   │
│   └── docs/
│       ├── daisyUI_themes.md            # DaisyUI documentation
│       └── semi.avalonia_themes.md      # Semi architecture documentation
│
├── OpenSourceToolkit.NET/
│   ├── ViewModels/Tools/
│   │   └── ThemeSelectionToolViewModel.cs   # Uses Semi.Daisy.Theming.Services.IThemeService
│   ├── ViewModels/
│   │   └── SettingsViewModel.cs         # Uses IThemeService for startup theme restoration
│   └── Views/Tools/
│       └── ThemeSelectionToolView.axaml     # UI unchanged
```

---

### IThemeService Interface

The `IThemeService` interface provides a clean abstraction for all DaisyUI theme operations:

| Member | Purpose |
|--------|---------|
| `AvailableThemes` | Returns all 36 embedded DaisyUI themes (corporate, dracula, nord, etc.) pre-parsed and ready to use. No need to manually load CSS files or deal with assembly resources. |
| `CurrentTheme` | Tracks which theme is currently applied—useful for UI state (e.g., highlighting the selected theme in a picker). |
| `ParseCss(css, name)` | Parses raw DaisyUI CSS (with OKLCH colors) into a `DaisyUiTheme` object. Useful for importing custom themes pasted by users. |
| `GenerateAxaml(theme)` | Generates Avalonia ResourceDictionary AXAML from a theme. Users can copy this to use the theme statically without runtime conversion. |
| `ApplyTheme(theme, app)` | The main feature—applies a DaisyUI theme to Semi.Avalonia at runtime by injecting ~180 color resources into the app's resource dictionaries. |
| `GetThemeByName(name)` | Retrieves a theme by name. Used by `SettingsViewModel` to restore the saved theme on app startup. |
| `GetThemeCss(name)` | Returns the raw CSS content for an embedded theme. |

**Why an interface?**

- **Testability**: You can mock `IThemeService` in unit tests
- **Decoupling**: ViewModels don't depend on concrete implementation details
- **Future flexibility**: Could swap implementations (e.g., one that fetches themes from a server)

**Shared Instance via AppSettings:**

The theme service is exposed as a singleton through `AppSettings.ThemeService`:

```csharp
// In AppSettings.cs
public static IThemeService ThemeService { get; }

// Usage in ViewModels
var themes = AppSettings.ThemeService.AvailableThemes;
AppSettings.ThemeService.ApplyTheme(theme, Application.Current);
```

This ensures a single `ThemeService` instance is shared across the app, avoiding duplicate theme loading.

---

### Public API Surface

```csharp
namespace Semi.Daisy.Theming.Services
{
    public interface IThemeService
    {
        IReadOnlyList<DaisyUiTheme> AvailableThemes { get; }
        DaisyUiTheme CurrentTheme { get; }

        DaisyUiTheme ParseCss(string cssContent, string name = "custom");
        string GenerateAxaml(DaisyUiTheme theme);
        void ApplyTheme(DaisyUiTheme theme, Application app);
        DaisyUiTheme GetThemeByName(string name);
        string GetThemeCss(string name);
    }

    public class ThemeService : IThemeService { ... }
}

namespace Semi.Daisy.Theming.DaisyUI
{
    public class DaisyUiTheme { ... }
    public static class DaisyUiCssParser { ... }
    public static class DaisyUiAxamlGenerator { ... }
}

namespace Semi.Daisy.Theming.ColorSpaces
{
    public struct OklchColor { ... }
    public static class ColorConverter { ... }
}

namespace Semi.Daisy.Theming.Semi
{
    public static class SemiResourceKeys { ... }
    public static class SemiThemeMapping { ... }
    public static class SemiThemeApplicator { ... }
}
```

---

### Usage Examples

**Loading and applying a theme:**

```csharp
using Semi.Daisy.Theming.Services;

var themeService = new ThemeService();

// Get available themes
foreach (var theme in themeService.AvailableThemes)
    Console.WriteLine($"{theme.Name} ({(theme.IsDark ? "dark" : "light")})");

// Apply a theme by name
var dracula = themeService.GetThemeByName("dracula");
if (dracula != null)
    themeService.ApplyTheme(dracula, Application.Current);
```

**Parsing custom CSS:**

```csharp
var customCss = File.ReadAllText("my-theme.css");
var theme = themeService.ParseCss(customCss, "my-theme");
var axaml = themeService.GenerateAxaml(theme);
```

**Direct color conversion:**

```csharp
using Semi.Daisy.Theming.ColorSpaces;

var oklch = OklchColor.Parse("75.461% 0.183 346.812");
var hex = ColorConverter.OklchToHex(oklch); // "#E879F9"
```

---

### Migration Summary

| Aspect | Before | After |
|--------|--------|-------|
| **Lines in main app** | ~500 in DaisyUiThemeConverter.cs | ~50 (service calls) |
| **Testability** | Hard to test color math | Unit testable ColorConverter |
| **Reusability** | Copy-paste to other projects | Project reference |
| **Documentation** | Separate from code | Co-located in `docs/` |
| **Maintenance** | Mixed concerns in one file | Clear boundaries |

---

---

## App-Level Theme Customizations

While `Semi.Daisy.Theming` handles the core DaisyUI→Semi mapping, the consuming app needs additional customizations to handle edge cases that Semi's architecture doesn't support natively.

### Custom Content Color Resources

Semi uses a single `ButtonSolidForeground` for all solid button types, but DaisyUI provides separate content colors. We inject custom resources:

| Resource Key | DaisyUI Source | Purpose |
|-------------|----------------|---------|
| `DaisySecondaryContentBrush` | `color-secondary-content` | Text on secondary buttons |
| `DaisyAccentContentBrush` | `color-accent-content` | Text on tertiary/accent buttons |
| `DaisySuccessContentBrush` | `color-success-content` | Text on success buttons |
| `DaisyWarningContentBrush` | `color-warning-content` | Text on warning buttons |
| `DaisyErrorContentBrush` | `color-error-content` | Text on danger buttons |
| `DaisyInfoContentBrush` | `color-info-content` | Text on info buttons |

These are injected by `SemiThemeApplicator.ApplyCustomResources()` when a DaisyUI theme is applied.

### App.axaml Style Overrides

Add styles that use the custom resources to override button foregrounds:

```xml
<!-- In App.axaml -->
<Style Selector="Button.secondary /template/ ContentPresenter#PART_ContentPresenter">
    <Setter Property="Foreground" Value="{DynamicResource DaisySecondaryContentBrush}"/>
</Style>
<Style Selector="Button.tertiary /template/ ContentPresenter#PART_ContentPresenter">
    <Setter Property="Foreground" Value="{DynamicResource DaisyAccentContentBrush}"/>
</Style>
<!-- ... similar for success, warning, danger -->
```

### ThemeResources.axaml Fallbacks

Provide fallback values for when no DaisyUI theme is active (pure Semi Light/Dark):

```xml
<!-- In ThemeResources.axaml, both Light and Dark dictionaries -->
<SolidColorBrush x:Key="DaisySecondaryContentBrush" Color="White" />
<SolidColorBrush x:Key="DaisyAccentContentBrush" Color="White" />
<SolidColorBrush x:Key="DaisySuccessContentBrush" Color="White" />
<SolidColorBrush x:Key="DaisyWarningContentBrush" Color="White" />
<SolidColorBrush x:Key="DaisyErrorContentBrush" Color="White" />
<SolidColorBrush x:Key="DaisyInfoContentBrush" Color="White" />
```

---

## ⚠️ Common Pitfalls (Lessons Learned)

### 1. Don't Use `color-neutral-content` for Muted Text

**Symptom:** Placeholder text, inactive tabs, or muted text becomes unreadable in certain themes.

**Cause:** `color-neutral-content` is designed to contrast with `color-neutral`, not with base backgrounds. In light themes like "caramellatte":
- `color-neutral`: 55% lightness (dark orange-brown)
- `color-neutral-content`: 98% lightness (very light cream)
- `color-base-200` (TextBox bg): 95% lightness

Using neutral-content for placeholder = 98% text on 95% background = invisible!

**Fix:** Use `color-base-content` for ALL text on base backgrounds, including muted/placeholder text.

### 2. Selected Item Foreground Must Use `color-primary-content`

**Symptom:** Selected items in ListBox/ComboBox have unreadable text.

**Cause:** Selected items have `color-primary` background. Using `color-base-content` (designed for base backgrounds) creates poor contrast.

**Fix:** Map `ListBoxItemSelectedForeground` and `ComboBoxItemSelectedForeground` to `color-primary-content`.

### 3. Solid Button Foregrounds Need Per-Class Overrides

**Symptom:** Secondary/tertiary solid buttons have wrong text color in some themes.

**Cause:** Semi uses one `ButtonSolidForeground` for all solid buttons, but DaisyUI has separate content colors.

**Fix:** Inject custom `Daisy*ContentBrush` resources and add app-level style overrides (see above).

### 4. Theme Clearing Must Remove from SemiTheme.Resources

**Symptom:** Switching from DaisyUI theme to pure Semi theme doesn't fully restore Semi colors.

**Cause:** `ApplyTheme` injects colors into three places: `app.Resources`, `ThemeDictionaries`, and `SemiTheme.Resources`. The clear logic must remove from all three.

**Fix:** `SemiThemeApplicator.ClearTheme()` iterates through `app.Styles` to find `SemiTheme` and removes injected keys from its resources.

---

### Future Considerations

1. **NuGet Publishing**: Could be published as a public NuGet package for other Avalonia projects.
2. **Theme Persistence**: The library does not handle persistence; consuming apps manage that (e.g., `AppSettings.DaisyUiTheme`).
3. **Additional Color Spaces**: `ColorSpaces/` could be extended with HSL, LAB, etc.
4. **Theme Validation**: Add a validation step that checks contrast ratios for critical color pairs.
