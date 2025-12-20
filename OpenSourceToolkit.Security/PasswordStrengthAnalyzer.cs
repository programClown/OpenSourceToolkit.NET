using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenSourceToolkit.Security
{
    public class PasswordStrengthResult
    {
        public int Strength { get; set; } // 0-5
        public string Label { get; set; }
        public double Entropy { get; set; }
        public string CrackTime { get; set; }
    }

    public static class PasswordStrengthAnalyzer
    {
        public static PasswordStrengthResult Analyze(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return new PasswordStrengthResult
                {
                    Strength = 0,
                    Label = "Very Weak",
                    Entropy = 0,
                    CrackTime = "< 1 minute"
                };
            }

            int score = 0;
            int charsetSize = 0;

            bool hasLower = Regex.IsMatch(password, "[a-z]");
            bool hasUpper = Regex.IsMatch(password, "[A-Z]");
            bool hasNumber = Regex.IsMatch(password, "[0-9]");
            bool hasSymbol = Regex.IsMatch(password, "[^a-zA-Z0-9]");

            if (hasLower) charsetSize += 26;
            if (hasUpper) charsetSize += 26;
            if (hasNumber) charsetSize += 10;
            if (hasSymbol) charsetSize += 32;

            double entropy = Math.Log(Math.Pow(charsetSize, password.Length), 2);

            // Length scoring
            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            if (password.Length >= 16) score++;
            if (password.Length >= 20) score++;

            // Variety scoring
            if (hasLower) score++;
            if (hasUpper) score++;
            if (hasNumber) score++;
            if (hasSymbol) score++;

            // Penalties
            if (Regex.IsMatch(password, @"(.)\1{2,}")) score--; // Repeated chars
            if (Regex.IsMatch(password, "012|123|234|345|456|567|678|789|890")) score--;

            // Simplified sequential letter check (subset of original for brevity)
            if (Regex.IsMatch(password.ToLower(), "abc|bcd|cde|def|efg|fgh|ghi|hij|ijk|jkl|klm|lmn|mno|nop|opq|pqr|qrs|rst|stu|tuv|uvw|vwx|wxy|xyz")) score--;

            int strength = Math.Max(0, Math.Min(5, score));

            string[] labels = { "Very Weak", "Weak", "Fair", "Good", "Strong", "Very Strong" };
            string label = labels[strength];

            // Crack time
            double guessesPerSecond = 1_000_000_000;
            double totalCombinations = Math.Pow(charsetSize, password.Length);
            double secondsToCrack = totalCombinations / (2 * guessesPerSecond);

            string crackTime;
            if (secondsToCrack < 60) crackTime = "< 1 minute";
            else if (secondsToCrack < 3600) crackTime = $"{Math.Round(secondsToCrack / 60)} minutes";
            else if (secondsToCrack < 86400) crackTime = $"{Math.Round(secondsToCrack / 3600)} hours";
            else if (secondsToCrack < 31536000) crackTime = $"{Math.Round(secondsToCrack / 86400)} days";
            else if (secondsToCrack < 31536000000) crackTime = $"{Math.Round(secondsToCrack / 31536000)} years";
            else crackTime = "Centuries";

            return new PasswordStrengthResult
            {
                Strength = strength,
                Label = label,
                Entropy = Math.Round(entropy),
                CrackTime = crackTime
            };
        }
    }
}
