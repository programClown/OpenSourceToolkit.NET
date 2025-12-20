#nullable enable
using System;
using System.IO;
using System.Linq;

namespace OpenSourceToolkit.NET.Services
{
    /// <summary>
    /// Simple file-based logger for debugging.
    /// Only active when:
    /// 1. App is launched with "/log" command line argument
    /// 2. Running in DEBUG build
    /// 3. Running on Desktop (not WASM/Mobile)
    /// </summary>
    public static class DebugLogger
    {
        private static readonly object _lock = new();
        private static string? _logPath;
        private static bool _initialized;
        private static bool _enabled;

        /// <summary>
        /// Gets whether logging is enabled.
        /// </summary>
        public static bool IsEnabled => _enabled;

        /// <summary>
        /// Gets the path to the current log file (null if logging is disabled).
        /// </summary>
        public static string? LogPath => _logPath;

        /// <summary>
        /// Initialize the logger. Call this from App startup with command line args.
        /// Logging is only enabled in DEBUG builds when /log argument is present.
        /// </summary>
        public static void Initialize(string[] args)
        {
#if DEBUG
            // Check if /log or --log argument is present
            _enabled = args.Any(a => 
                a.Equals("/log", StringComparison.OrdinalIgnoreCase) || 
                a.Equals("--log", StringComparison.OrdinalIgnoreCase));

            if (_enabled)
            {
                SetupLogFile();
            }
#else
            // Logging disabled in Release builds
            _enabled = false;
#endif
        }

        private static void SetupLogFile()
        {
            lock (_lock)
            {
                if (_initialized) return;
                
                // Use project's logs folder
                var logsFolder = @"D:\github\OpensourceToolkit.NET\logs";
                if (!Directory.Exists(logsFolder))
                    Directory.CreateDirectory(logsFolder);
                
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                _logPath = Path.Combine(logsFolder, $"debug_{timestamp}.log");
                
                _initialized = true;
                
                // Write header
                File.WriteAllText(_logPath, $"=== OpenSourceToolkit.NET Debug Log ===\n");
                File.AppendAllText(_logPath, $"Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
                File.AppendAllText(_logPath, $"Log file: {_logPath}\n");
                File.AppendAllText(_logPath, new string('=', 50) + "\n\n");
                
                System.Diagnostics.Debug.WriteLine($"[DebugLogger] Logging enabled. File: {_logPath}");
            }
        }

        /// <summary>
        /// Logs a message to the debug file (if logging is enabled).
        /// </summary>
        public static void Log(string message)
        {
            if (!_enabled || _logPath == null) return;
            
            lock (_lock)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var line = $"[{timestamp}] {message}\n";
                File.AppendAllText(_logPath, line);
                
                // Also write to Debug output for IDE visibility
                System.Diagnostics.Debug.WriteLine(message);
            }
        }

        /// <summary>
        /// Logs a message with a category tag (if logging is enabled).
        /// </summary>
        public static void Log(string category, string message)
        {
            Log($"[{category}] {message}");
        }
    }
}
