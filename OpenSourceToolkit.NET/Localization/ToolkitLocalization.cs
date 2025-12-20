using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

#nullable enable

namespace OpenSourceToolkit.NET.Localization
{
    /// <summary>
    /// Provides JSON-based localization services for the OpenSourceToolkit.NET application.
    /// This implementation uses embedded JSON files for WASM compatibility with runtime language switching.
    /// </summary>
    public class ToolkitLocalization : INotifyPropertyChanged
    {
        private static CultureInfo _currentCulture = CultureInfo.CurrentUICulture;
        private static readonly Dictionary<string, Dictionary<string, string>> _translations = new();
        private static readonly Lazy<ToolkitLocalization> _instance = new(() => new ToolkitLocalization());

        /// <summary>
        /// Singleton instance for XAML markup extension bindings.
        /// </summary>
        public static ToolkitLocalization Instance => _instance.Value;

        /// <summary>
        /// Event fired when the culture is changed. Subscribe to this to refresh UI bindings.
        /// </summary>
        public static event EventHandler<CultureInfo>? CultureChanged;

        /// <summary>
        /// PropertyChanged event for INotifyPropertyChanged interface (used by XAML bindings).
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Supported languages with their codes and display names.
        /// This is the SINGLE SOURCE OF TRUTH for all language-related code.
        /// </summary>
        public static readonly IReadOnlyList<(string Code, string DisplayName)> SupportedLanguages = new[]
        {
            ("en", "English"),
            ("ar", "العربية"),
            ("he", "עברית"),
            ("de", "Deutsch"),
            ("es", "Español"),
            ("fr", "Français"),
            ("it", "Italiano"),
            ("ja", "日本語"),
            ("ko", "한국어"),
            ("tr", "Türkçe"),
            ("uk", "Українська"),
            ("zh-Hans", "简体中文")
        };

        static ToolkitLocalization()
        {
            // LAZY LOADING: Only load English (fallback) at startup.
            // Other languages are loaded on-demand when SetCulture is called.
            // This reduces startup memory from ~7K strings to ~678 strings (~90% reduction).
            LoadTranslation("en");

            // Subscribe to FloweryLocalization culture changes
            try
            {
                Flowery.Localization.FloweryLocalization.CultureChanged += OnFloweryCultureChanged;
            }
            catch
            {
                // Flowery may not be loaded; ignore
            }
        }

        private static void OnFloweryCultureChanged(object? sender, CultureInfo culture)
        {
            SetCulture(culture);
        }

        private ToolkitLocalization() { }

        /// <summary>
        /// Gets the current UI culture used for localization.
        /// </summary>
        public static CultureInfo CurrentCulture => _currentCulture;

        /// <summary>
        /// Indexer to support XAML markup extension bindings.
        /// Usage in XAML: {loc:Localize Button_Generate} binds to this[Button_Generate]
        /// </summary>
        public string this[string key] => GetString(key);

        /// <summary>
        /// Sets the current UI culture and notifies subscribers.
        /// </summary>
        /// <param name="culture">The culture to switch to.</param>
        public static void SetCulture(CultureInfo culture)
        {
            if (culture == null)
                throw new ArgumentNullException(nameof(culture));

            if (_currentCulture.Name == culture.Name)
                return;

            // LAZY LOADING: Ensure the target language is loaded before switching
            EnsureTranslationLoaded(culture);

            _currentCulture = culture;

            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;

            CultureChanged?.Invoke(null, culture);

            Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs("Item"));
            Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs("Item[]"));
        }

        /// <summary>
        /// Sets the current UI culture by name and notifies subscribers.
        /// </summary>
        /// <param name="cultureName">The culture name (e.g., "en-US", "de-DE", "de").</param>
        public static void SetCulture(string cultureName)
        {
            SetCulture(new CultureInfo(cultureName));
        }

        /// <summary>
        /// Gets a localized string by key.
        /// Follows fallback chain: exact culture -> language code -> zh-Hans special -> English
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <returns>The localized string, or the key if not found.</returns>
        public static string GetString(string key)
        {
            try
            {
                // Try exact culture match first (e.g., "de-DE", "zh-Hans")
                if (_translations.TryGetValue(_currentCulture.Name, out var exactDict) && 
                    exactDict.TryGetValue(key, out var exactValue))
                {
                    return exactValue;
                }

                // Try language-only match (e.g., "de")
                var languageCode = _currentCulture.TwoLetterISOLanguageName;
                if (_translations.TryGetValue(languageCode, out var langDict) && 
                    langDict.TryGetValue(key, out var langValue))
                {
                    return langValue;
                }

                // Special handling for Chinese - try zh-Hans for any zh-* culture
                if (languageCode == "zh")
                {
                    if (_translations.TryGetValue("zh-Hans", out var zhDict) && 
                        zhDict.TryGetValue(key, out var zhValue))
                    {
                        return zhValue;
                    }
                }

                // Fallback to English
                if (_translations.TryGetValue("en", out var enDict) && 
                    enDict.TryGetValue(key, out var enValue))
                {
                    return enValue;
                }

                // Return key if not found
                return key;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ToolkitLocalization] GetString('{key}') exception: {ex.Message}");
                return key;
            }
        }

        /// <summary>
        /// Ensures translations for the given culture are loaded.
        /// Lazy-loads the language file if not already in memory.
        /// </summary>
        private static void EnsureTranslationLoaded(CultureInfo culture)
        {
            // Try exact culture match first (e.g., "zh-Hans")
            var exactName = culture.Name;
            if (_translations.ContainsKey(exactName))
                return;

            // Try language-only match (e.g., "de" from "de-DE")
            var languageCode = culture.TwoLetterISOLanguageName;
            if (_translations.ContainsKey(languageCode))
                return;

            // Special handling for Chinese
            if (languageCode == "zh")
            {
                if (!_translations.ContainsKey("zh-Hans"))
                    LoadTranslation("zh-Hans");
                return;
            }

            // Try to load by exact name first, then by language code
            if (SupportedLanguages.Any(l => l.Code.Equals(exactName, StringComparison.OrdinalIgnoreCase)))
            {
                LoadTranslation(exactName);
            }
            else if (SupportedLanguages.Any(l => l.Code.Equals(languageCode, StringComparison.OrdinalIgnoreCase)))
            {
                LoadTranslation(languageCode);
            }
            // else: unsupported language - will fall back to English
        }

        private static void LoadTranslation(string languageCode)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"OpenSourceToolkit.NET.Localization.{languageCode}.json";

                System.Diagnostics.Debug.WriteLine($"[ToolkitLocalization] Attempting to load: {resourceName}");

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[ToolkitLocalization] Resource not found: {resourceName}");
                    return;
                }

                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();

                // Use source generator context for AOT compatibility
                var dict = JsonSerializer.Deserialize(json, ToolkitLocalizationJsonContext.Default.DictionaryStringString);

                if (dict != null)
                {
                    _translations[languageCode] = dict;
                    System.Diagnostics.Debug.WriteLine($"[ToolkitLocalization] ✓ Loaded {dict.Count} strings for '{languageCode}'");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ToolkitLocalization] ✗ Failed to load '{languageCode}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// JSON source generator context for AOT/WASM compatibility.
    /// </summary>
    [JsonSourceGenerationOptions(
        WriteIndented = false,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        GenerationMode = JsonSourceGenerationMode.Metadata)]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    internal partial class ToolkitLocalizationJsonContext : JsonSerializerContext
    {
    }
}
