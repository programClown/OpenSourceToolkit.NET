using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSourceToolkit.Hardware;
using System.Threading;

namespace OpenSourceToolkit.Tests
{
    [TestClass]
    public class HardwareTests
    {
        [TestMethod]
        public void KeyboardTester_SpeedCalculation_Works()
        {
            var tester = new KeyboardTester();
            tester.StartTest();

            // Simulate typing
            tester.RegisterKeyPress();
            Thread.Sleep(100); // Small delay
            tester.RegisterKeyPress(); // 2 keys in ~100ms = ~1200 CPM

            var cpm = tester.CalculateTypingSpeedCpm();
            Assert.IsTrue(cpm > 0);

            var wpm = tester.CalculateTypingSpeedWpm();
            Assert.IsTrue(wpm > 0);
        }

        [TestMethod]
        public void KeyboardTester_SingleKeyPress_ReturnsZeroSpeed()
        {
            var tester = new KeyboardTester();
            tester.StartTest();
            tester.RegisterKeyPress();

            var cpm = tester.CalculateTypingSpeedCpm();
            var wpm = tester.CalculateTypingSpeedWpm();

            Assert.AreEqual(0, cpm);
            Assert.AreEqual(0, wpm);
        }

        [TestMethod]
        public void AudioDeviceManager_Enumeration_DoesNotCrash()
        {
            // May return 0 devices on CI/Cloud, but shouldn't crash
            var inputs = AudioDeviceManager.GetInputDevices();
            var outputs = AudioDeviceManager.GetOutputDevices();

            Assert.IsNotNull(inputs);
            Assert.IsNotNull(outputs);
        }

        [TestMethod]
        public void AudioProcessor_ProcessingPipeline_DoesNotCrash()
        {
            // Create a dummy wav file
            string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"test_audio_{System.Guid.NewGuid()}.wav");
            using (var writer = new NAudio.Wave.WaveFileWriter(tempFile, new NAudio.Wave.WaveFormat(44100, 16, 1)))
            {
                writer.WriteSamples(new float[44100], 0, 44100); // 1 sec silence
            }

            try
            {
                using (var processor = new AudioProcessor())
                {
                    processor.LoadAudio(tempFile);
                    processor.UpdateSettings(0, 80, 8000, 0, 0, 0, -24, 4, 3, 250);

                    // We can't easily test playback without audio device, but we can test Export
                    string exportPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"test_export_{System.Guid.NewGuid()}.wav");
                    try
                    {
                        processor.ExportProcessed(exportPath);
                        Assert.IsTrue(System.IO.File.Exists(exportPath));
                        Assert.IsTrue(new System.IO.FileInfo(exportPath).Length > 0);
                    }
                    finally
                    {
                        if (System.IO.File.Exists(exportPath)) System.IO.File.Delete(exportPath);
                    }
                }
            }
            finally
            {
                if (System.IO.File.Exists(tempFile)) System.IO.File.Delete(tempFile);
            }
        }
    }
}
