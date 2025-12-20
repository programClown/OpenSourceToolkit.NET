using System.Runtime.InteropServices;

namespace OpenSourceToolkit.Hardware
{
    public static class PlatformSupport
    {
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        /// <summary>
        /// NAudio-based audio features (recording, playback, device enumeration) are Windows-only.
        /// </summary>
        public static bool IsAudioSupported => IsWindows;

        /// <summary>
        /// Camera capture via OpenCvSharp is supported on all platforms with appropriate backend.
        /// </summary>
        public static bool IsCameraSupported => true;

        /// <summary>
        /// Keyboard testing uses pure .NET and is cross-platform.
        /// </summary>
        public static bool IsKeyboardTestSupported => true;
    }
}
