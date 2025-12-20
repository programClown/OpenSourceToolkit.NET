using System;

namespace OpenSourceToolkit.Converters
{
    public static class TimestampConverter
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTimeSeconds(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - Epoch).TotalSeconds;
        }

        public static long ToUnixTimeMilliseconds(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - Epoch).TotalMilliseconds;
        }

        public static DateTime FromUnixTimeSeconds(long seconds)
        {
            return Epoch.AddSeconds(seconds);
        }

        public static DateTime FromUnixTimeMilliseconds(long milliseconds)
        {
            return Epoch.AddMilliseconds(milliseconds);
        }
    }
}
