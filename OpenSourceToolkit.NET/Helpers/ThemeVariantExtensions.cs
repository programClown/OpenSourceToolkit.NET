using Avalonia.Styling;

namespace OpenSourceToolkit.NET.Helpers
{
    /// <summary>
    /// Extension methods for parsing and serializing theme variants.
    /// Simplified to support only Light/Dark since DaisyUI themes handle 
    /// all styling variations via Flowery.NET's DaisyThemeManager.
    /// </summary>
    public static class ThemeVariantExtensions
    {
        /// <summary>
        /// Parse a saved theme string to a ThemeVariant.
        /// Legacy Semi theme names (aquatic, desert, dusk, nightsky) are mapped to Dark.
        /// </summary>
        public static ThemeVariant ParseThemeVariant(this string theme)
        {
            if (string.IsNullOrWhiteSpace(theme))
                return ThemeVariant.Dark;

            switch (theme.Trim().ToLowerInvariant())
            {
                case "light":
                    return ThemeVariant.Light;
                case "dark":
                    return ThemeVariant.Dark;
                // Legacy Semi themes - map to Dark for backward compatibility
                case "aquatic":
                case "desert":
                case "dusk":
                case "nightsky":
                    return ThemeVariant.Dark;
                default:
                    return ThemeVariant.Dark;
            }
        }

        /// <summary>
        /// Convert a ThemeVariant to a settings string.
        /// </summary>
        public static string ToSettingsString(this ThemeVariant theme)
        {
            if (theme == null)
                return "dark";

            if (theme == ThemeVariant.Light)
                return "light";
            
            return "dark";
        }
    }
}
