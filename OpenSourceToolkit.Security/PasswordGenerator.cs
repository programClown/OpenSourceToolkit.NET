using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OpenSourceToolkit.Security
{
    public class PasswordOptions
    {
        public int Length { get; set; } = 12;
        public bool IncludeUppercase { get; set; } = true;
        public bool IncludeLowercase { get; set; } = true;
        public bool IncludeNumbers { get; set; } = true;
        public bool IncludeSymbols { get; set; } = true;
        public bool ExcludeSimilar { get; set; } = false;
        public bool ExcludeAmbiguous { get; set; } = false;
        public string CustomCharset { get; set; }
        public bool UseCustomCharset { get; set; } = false;
        public int MinUppercase { get; set; } = 1;
        public int MinLowercase { get; set; } = 1;
        public int MinNumbers { get; set; } = 1;
        public int MinSymbols { get; set; } = 1;
    }

    public static class PasswordGenerator
    {
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Numbers = "0123456789";
        private const string Symbols = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        private const string SimilarChars = "il1Lo0O";
        private const string AmbiguousChars = "{}[]()/\\'\"~,;.<>";

        private static readonly string[] CommonWords = new[]
        {
            "apple", "beach", "cloud", "dance", "earth", "flame", "green", "happy", "island", "jazz",
            "kite", "light", "music", "night", "ocean", "peace", "quick", "river", "stone", "tree",
            "unity", "voice", "water", "youth", "zebra", "brave", "chair", "dream", "eagle", "forest",
            "giant", "house", "image", "jump", "kind", "lemon", "magic", "noble", "orbit", "paper",
            "queen", "radio", "space", "tiger", "urban", "value", "world", "extra"
        };

        public static string Generate(PasswordOptions options)
        {
            if (options.UseCustomCharset && !string.IsNullOrEmpty(options.CustomCharset))
            {
                return GenerateFromCharset(options.CustomCharset, options.Length);
            }

            var passwordChars = new List<char>();
            var charsetBuilder = new StringBuilder();

            // 1. Collect requirements and build master charset
            if (options.IncludeUppercase)
            {
                string chars = FilterChars(Uppercase, options);
                AddRandomChars(passwordChars, chars, options.MinUppercase);
                charsetBuilder.Append(chars);
            }
            if (options.IncludeLowercase)
            {
                string chars = FilterChars(Lowercase, options);
                AddRandomChars(passwordChars, chars, options.MinLowercase);
                charsetBuilder.Append(chars);
            }
            if (options.IncludeNumbers)
            {
                string chars = FilterChars(Numbers, options);
                AddRandomChars(passwordChars, chars, options.MinNumbers);
                charsetBuilder.Append(chars);
            }
            if (options.IncludeSymbols)
            {
                string chars = FilterChars(Symbols, options);
                AddRandomChars(passwordChars, chars, options.MinSymbols);
                charsetBuilder.Append(chars);
            }

            string fullCharset = charsetBuilder.ToString();
            if (fullCharset.Length == 0)
            {
                // Fallback if nothing selected
                fullCharset = Lowercase;
            }

            // 2. Fill remaining length
            while (passwordChars.Count < options.Length)
            {
                passwordChars.Add(GetRandomChar(fullCharset));
            }

            // 3. Trim if we exceeded length due to min requirements (though UI usually prevents this, logic should handle it)
            // Note: If requirements > length, we prioritize requirements, so password might be longer.
            // Or we trim. Original TS implementation effectively loops `for (let i = password.length; i < options.length; i++)`,
            // implying if requirements filled it, it stops. It doesn't trim.
            // We'll match that behavior (return potentially longer if reqs > len).

            // 4. Shuffle
            Shuffle(passwordChars);

            return new string(passwordChars.ToArray());
        }

        // Legacy/Simple overload for backward compatibility with tests
        public static string Generate(int length = 12, bool useUpper = true, bool useNumbers = true, bool useSymbols = true)
        {
            return Generate(new PasswordOptions
            {
                Length = length,
                IncludeUppercase = useUpper,
                IncludeNumbers = useNumbers,
                IncludeSymbols = useSymbols,
                IncludeLowercase = true, // Default in original signature was implicit by char pool inclusion
                MinUppercase = 0, // Legacy didn't enforce min
                MinLowercase = 0,
                MinNumbers = 0,
                MinSymbols = 0
            });
        }

        public static string GeneratePin(int length)
        {
            return GenerateFromCharset(Numbers, length);
        }

        public static string GeneratePassphrase(int wordCount, string separator, bool capitalize, bool includeNumbers)
        {
            var words = new List<string>();
            for (int i = 0; i < wordCount; i++)
            {
                string word = CommonWords[GetRandomInt(CommonWords.Length)];

                if (capitalize)
                {
                    word = char.ToUpper(word[0]) + word.Substring(1);
                }

                if (includeNumbers)
                {
                    word += GetRandomInt(10).ToString();
                }

                words.Add(word);
            }

            // Handle "none" separator or custom
            string sep = separator == "none" ? "" : (separator == "space" ? " " : separator);
            return string.Join(sep, words);
        }

        private static string GenerateFromCharset(string charset, int length)
        {
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = GetRandomChar(charset);
            }
            return new string(chars);
        }

        private static string FilterChars(string chars, PasswordOptions options)
        {
            if (options.ExcludeSimilar)
            {
                foreach (char c in SimilarChars)
                {
                    chars = chars.Replace(c.ToString(), "");
                }
            }
            if (options.ExcludeAmbiguous)
            {
                foreach (char c in AmbiguousChars)
                {
                    chars = chars.Replace(c.ToString(), "");
                }
            }
            return chars;
        }

        private static void AddRandomChars(List<char> result, string source, int count)
        {
            if (string.IsNullOrEmpty(source)) return;
            for (int i = 0; i < count; i++)
            {
                result.Add(GetRandomChar(source));
            }
        }

        private static char GetRandomChar(string charset)
        {
            int index = GetRandomInt(charset.Length);
            return charset[index];
        }

        private static int GetRandomInt(int max)
        {
            byte[] buffer = new byte[4];
            RandomNumberGenerator.Fill(buffer);
            uint num = BitConverter.ToUInt32(buffer, 0);
            return (int)(num % (uint)max);
        }

        private static void Shuffle<T>(IList<T> list)
        {
            byte[] buffer = new byte[4];
            int n = list.Count;
            while (n > 1)
            {
                n--;
                RandomNumberGenerator.Fill(buffer);
                uint num = BitConverter.ToUInt32(buffer, 0);
                int k = (int)(num % (uint)(n + 1));
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
