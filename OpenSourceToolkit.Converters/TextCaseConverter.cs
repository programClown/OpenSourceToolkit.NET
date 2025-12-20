using System.Globalization;

namespace OpenSourceToolkit.Converters
{
    public static class TextCaseConverter
    {
        public static string ToUpperCase(string text) => text?.ToUpperInvariant();
        public static string ToLowerCase(string text) => text?.ToLowerInvariant();

        public static string ToTitleCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
        }

        public static string ToSentenceCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var lower = text.ToLower();
            return char.ToUpper(lower[0]) + lower.Substring(1);
        }
    }
}
