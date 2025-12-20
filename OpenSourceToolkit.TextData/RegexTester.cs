using System.Text.RegularExpressions;

namespace OpenSourceToolkit.TextData
{
    public static class RegexTester
    {
        public static bool Test(string pattern, string input)
        {
            try
            {
                return Regex.IsMatch(input, pattern);
            }
            catch
            {
                return false;
            }
        }

        public static MatchCollection Matches(string pattern, string input)
        {
            try
            {
                return Regex.Matches(input, pattern);
            }
            catch
            {
                return null;
            }
        }
    }
}
