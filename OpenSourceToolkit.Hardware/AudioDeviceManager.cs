using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace OpenSourceToolkit.Hardware
{
    public class AudioDeviceManager
    {
        public class AudioDevice
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Channels { get; set; }
        }

        public static List<AudioDevice> GetInputDevices()
        {
            if (!PlatformSupport.IsAudioSupported)
                throw new PlatformNotSupportedException("Audio device enumeration is only supported on Windows.");

            var devices = new List<AudioDevice>();
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var capabilities = WaveIn.GetCapabilities(i);
                devices.Add(new AudioDevice
                {
                    Id = i,
                    Name = capabilities.ProductName,
                    Channels = capabilities.Channels
                });
            }
            return devices;
        }

        public static List<AudioDevice> GetOutputDevices()
        {
            if (!PlatformSupport.IsAudioSupported)
                throw new PlatformNotSupportedException("Audio device enumeration is only supported on Windows.");

            var devices = new List<AudioDevice>();
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var capabilities = WaveOut.GetCapabilities(i);
                devices.Add(new AudioDevice
                {
                    Id = i,
                    Name = capabilities.ProductName,
                    Channels = capabilities.Channels
                });
            }
            return devices;
        }
    }
}
