using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenCvSharp;

namespace OpenSourceToolkit.Hardware
{
    public class CameraDeviceManager
    {
        public class VideoDevice
        {
            public string Name { get; set; }
            public int Index { get; set; }
        }

        private static VideoCaptureAPIs GetPreferredApi()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return VideoCaptureAPIs.DSHOW;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return VideoCaptureAPIs.V4L2;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return VideoCaptureAPIs.AVFOUNDATION;
            return VideoCaptureAPIs.ANY;
        }

        public static Task<List<VideoDevice>> GetAvailableDevicesAsync()
        {
            return Task.Run(() =>
            {
                var devices = new List<VideoDevice>();
                var api = GetPreferredApi();
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        using (var cap = new VideoCapture(i, api))
                        {
                            if (cap.IsOpened())
                            {
                                devices.Add(new VideoDevice { Name = $"Camera {i}", Index = i });
                            }
                        }
                    }
                    catch
                    {
                        // Ignore errors during enumeration
                    }
                }
                return devices;
            });
        }

        public static VideoCapture CreateCapture(int index)
        {
            var capture = new VideoCapture(index, GetPreferredApi());
            if (!capture.IsOpened())
            {
                capture.Dispose();
                throw new Exception($"Could not open camera at index {index}");
            }
            return capture;
        }
    }
}
