using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenSourceToolkit.Networking
{
    public class SpeedTester
    {
        private readonly HttpClient _httpClient;

        public SpeedTester(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<SpeedTestResult> TestDownloadSpeedAsync(string url, int durationSeconds = 10)
        {
            var result = new SpeedTestResult();
            var stopwatch = Stopwatch.StartNew();
            long bytesReceived = 0;

            // This is a simplified test. A real test would use multiple streams and specific large files.
            // Here we just download as much as we can from a stream or loop downloads.

            try
            {
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds)))
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                         // Request a chunk or file.
                         // Warning: This logic assumes the URL returns data.
                         // Ideally, we need a specific speed test server endpoint.
                         // For safety, we just do a single HEAD/GET to measure latency if the URL is generic,
                         // but for "speed", we need bytes.

                         var buffer = new byte[8192];
                         using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token))
                         using (var stream = await response.Content.ReadAsStreamAsync())
                         {
                             int read;
                             while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, cts.Token)) > 0)
                             {
                                 bytesReceived += read;
                             }
                         }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected timeout
            }
            catch (Exception)
            {
                // Handle connection errors
            }

            stopwatch.Stop();

            result.BytesTransferred = bytesReceived;
            result.Duration = stopwatch.Elapsed;
            result.BitsPerSecond = (bytesReceived * 8) / stopwatch.Elapsed.TotalSeconds;

            return result;
        }
    }

    public class SpeedTestResult
    {
        public long BytesTransferred { get; set; }
        public TimeSpan Duration { get; set; }
        public double BitsPerSecond { get; set; }
        public double Mbps => BitsPerSecond / 1_000_000.0;
    }
}
