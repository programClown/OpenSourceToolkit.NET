using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenSourceToolkit.Networking
{
    public class UptimeMonitor
    {
        private readonly HttpClient _httpClient;

        public event EventHandler<UptimeCheckResult> OnCheckCompleted;

        public UptimeMonitor(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<UptimeCheckResult> CheckAsync(string url)
        {
            var result = new UptimeCheckResult { Url = url, Timestamp = DateTime.Now };
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _httpClient.GetAsync(url);
                stopwatch.Stop();

                result.IsUp = response.IsSuccessStatusCode;
                result.StatusCode = (int)response.StatusCode;
                result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                result.IsUp = false;
                result.ErrorMessage = ex.Message;
            }

            OnCheckCompleted?.Invoke(this, result);
            return result;
        }
    }

    public class UptimeCheckResult
    {
        public string Url { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsUp { get; set; }
        public int StatusCode { get; set; }
        public long ResponseTimeMs { get; set; }
        public string ErrorMessage { get; set; }
    }
}
