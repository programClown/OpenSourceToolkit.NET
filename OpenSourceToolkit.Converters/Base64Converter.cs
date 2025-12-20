using System;
using System.Text;

namespace OpenSourceToolkit.Converters
{
    public static class Base64Converter
    {
        public static string Encode(string text, Encoding encoding = null)
        {
            if (text == null) return null;
            var bytes = (encoding ?? Encoding.UTF8).GetBytes(text);
            return Convert.ToBase64String(bytes);
        }

        public static string Decode(string base64, Encoding encoding = null)
        {
            if (base64 == null) return null;
            var bytes = Convert.FromBase64String(base64);
            return (encoding ?? Encoding.UTF8).GetString(bytes);
        }

        public static string EncodeUrlSafe(string text, Encoding encoding = null)
        {
            var base64 = Encode(text, encoding);
            return base64.Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        public static string DecodeUrlSafe(string base64Url, Encoding encoding = null)
        {
            var base64 = base64Url.Replace("-", "+").Replace("_", "/");
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Decode(base64, encoding);
        }
    }
}
