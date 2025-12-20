using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenSourceToolkit.Hardware
{
    public class KeyboardTester
    {
        private readonly Stopwatch _stopwatch;
        private readonly List<long> _keyPressTimestamps;

        public KeyboardTester()
        {
            _stopwatch = new Stopwatch();
            _keyPressTimestamps = new List<long>();
        }

        public void StartTest()
        {
            _stopwatch.Restart();
            _keyPressTimestamps.Clear();
        }

        public void RegisterKeyPress()
        {
            if (_stopwatch.IsRunning)
            {
                _keyPressTimestamps.Add(_stopwatch.ElapsedMilliseconds);
            }
        }

        public double CalculateTypingSpeedCpm()
        {
            if (_keyPressTimestamps.Count < 2) return 0;

            var durationMs = _keyPressTimestamps.Last() - _keyPressTimestamps.First();
            if (durationMs == 0) return 0;

            var minutes = durationMs / 60000.0;
            return _keyPressTimestamps.Count / minutes;
        }

        public double CalculateTypingSpeedWpm(double avgWordLength = 5.0)
        {
            return CalculateTypingSpeedCpm() / avgWordLength;
        }
    }
}
