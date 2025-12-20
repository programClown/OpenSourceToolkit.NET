using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class SpeedTestResult
    {
        public double Download { get; set; }
        public double Upload { get; set; }
        public double Ping { get; set; }
        public string Timestamp { get; set; }
    }

    public class SpeedTestToolViewModel : ToolViewModel
    {
        public override int Id => 35;
        public override string Name => ToolkitLocalization.GetString("Tool_SpeedTest_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_SpeedTest_Description");
        public override string IconKey => "SpeedTestIcon";

        private readonly HttpClient _httpClient;
        private CancellationTokenSource _cts;

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (SetProperty(ref _isRunning, value))
                {
                    ((RelayCommand)StartTestCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)StopTestCommand).NotifyCanExecuteChanged();
                }
            }
        }

        private string _currentTest;
        public string CurrentTest
        {
            get => _currentTest;
            set => SetProperty(ref _currentTest, value);
        }

        private int _progress;
        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        private string _error;
        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        private double _pingResult;
        public double PingResult
        {
            get => _pingResult;
            set => SetProperty(ref _pingResult, value);
        }

        private double _downloadResult;
        public double DownloadResult
        {
            get => _downloadResult;
            set => SetProperty(ref _downloadResult, value);
        }

        private double _uploadResult;
        public double UploadResult
        {
            get => _uploadResult;
            set => SetProperty(ref _uploadResult, value);
        }

        private bool _hasResults;
        public bool HasResults
        {
            get => _hasResults;
            set => SetProperty(ref _hasResults, value);
        }

        public ObservableCollection<SpeedTestResult> History { get; } = new ObservableCollection<SpeedTestResult>();

        public ICommand StartTestCommand { get; }
        public ICommand StopTestCommand { get; }
        public ICommand ClearHistoryCommand { get; }

        public SpeedTestToolViewModel()
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            StartTestCommand = new RelayCommand(async () => await RunSpeedTestAsync(), () => !IsRunning);
            StopTestCommand = new RelayCommand(StopTest, () => IsRunning);
            ClearHistoryCommand = new RelayCommand(() => History.Clear());
        }

        private async Task RunSpeedTestAsync()
        {
            IsRunning = true;
            Error = null;
            Progress = 0;
            HasResults = false;
            _cts = new CancellationTokenSource();

            try
            {
                // Ping test
                CurrentTest = "Testing Ping...";
                PingResult = await MeasurePingAsync(_cts.Token);
                Progress = 33;

                if (_cts.IsCancellationRequested) return;

                // Download test
                CurrentTest = "Testing Download Speed...";
                DownloadResult = await MeasureDownloadAsync(_cts.Token);
                Progress = 66;

                if (_cts.IsCancellationRequested) return;

                // Upload test
                CurrentTest = "Testing Upload Speed...";
                UploadResult = await MeasureUploadAsync(_cts.Token);
                Progress = 100;

                CurrentTest = "Test Complete!";
                HasResults = true;

                var result = new SpeedTestResult
                {
                    Download = DownloadResult,
                    Upload = UploadResult,
                    Ping = PingResult,
                    Timestamp = DateTime.Now.ToString("g")
                };

                History.Insert(0, result);
                if (History.Count > 10)
                    History.RemoveAt(History.Count - 1);
            }
            catch (OperationCanceledException)
            {
                CurrentTest = "Test cancelled.";
            }
            catch (Exception ex)
            {
                Error = $"Speed test failed: {ex.Message}";
                CurrentTest = null;
            }
            finally
            {
                IsRunning = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        private async Task<double> MeasurePingAsync(CancellationToken ct)
        {
            const int iterations = 5;
            double totalPing = 0;
            int successfulPings = 0;

            for (int i = 0; i < iterations; i++)
            {
                ct.ThrowIfCancellationRequested();
                var sw = Stopwatch.StartNew();
                try
                {
                    // Use a linked token with 5-second timeout per ping request
                    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
                    
                    await _httpClient.GetAsync($"https://httpbin.org/get?t={DateTime.Now.Ticks}", linkedCts.Token);
                    sw.Stop();
                    totalPing += sw.ElapsedMilliseconds;
                    successfulPings++;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    // User cancelled - re-throw to stop the entire test
                    throw;
                }
                catch (Exception ex)
                {
                    // If the FIRST ping fails, abort the entire test
                    if (i == 0)
                    {
                        throw new Exception("Speed test server is not reachable. Please try again later.", ex);
                    }
                    // Subsequent ping failures are tolerated - count as failed ping
                    totalPing += 5000;
                }
            }

            return successfulPings > 0 ? totalPing / successfulPings : 5000;
        }

        private async Task<double> MeasureDownloadAsync(CancellationToken ct)
        {
            int[] testSizes = { 1, 2, 5 }; // MB
            double totalSpeed = 0;
            int successfulTests = 0;

            foreach (var size in testSizes)
            {
                ct.ThrowIfCancellationRequested();
                var sw = Stopwatch.StartNew();
                try
                {
                    // Use a linked token with 15-second timeout per download
                    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
                    
                    var response = await _httpClient.GetAsync($"https://httpbin.org/bytes/{size * 1024 * 1024}", linkedCts.Token);
                    var data = await response.Content.ReadAsByteArrayAsync();
                    sw.Stop();

                    double durationSeconds = sw.ElapsedMilliseconds / 1000.0;
                    if (durationSeconds > 0)
                    {
                        // bits = bytes * 8, then divide by seconds for bps, then by 1_000_000 for Mbps
                        double speedMbps = (data.Length * 8.0) / durationSeconds / 1_000_000.0;
                        totalSpeed += speedMbps;
                        successfulTests++;
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch
                {
                    // Skip failed/timed out test
                }
            }

            return successfulTests > 0 ? totalSpeed / successfulTests : 0;
        }

        private async Task<double> MeasureUploadAsync(CancellationToken ct)
        {
            double[] testSizes = { 0.5, 1, 2 }; // MB
            double totalSpeed = 0;
            int successfulTests = 0;

            foreach (var size in testSizes)
            {
                ct.ThrowIfCancellationRequested();
                var data = new byte[(int)(size * 1024 * 1024)];
                var content = new ByteArrayContent(data);

                var sw = Stopwatch.StartNew();
                try
                {
                    // Use a linked token with 30-second timeout per upload
                    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
                    
                    await _httpClient.PostAsync("https://httpbin.org/post", content, linkedCts.Token);
                    sw.Stop();

                    double durationSeconds = sw.ElapsedMilliseconds / 1000.0;
                    if (durationSeconds > 0)
                    {
                        // size is in MB, convert to bytes (*1024*1024), then to bits (*8), divide by seconds, then by 1_000_000 for Mbps
                        double speedMbps = (size * 1024.0 * 1024.0 * 8.0) / durationSeconds / 1_000_000.0;
                        totalSpeed += speedMbps;
                        successfulTests++;
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch
                {
                    // Skip failed/timed out test
                }
            }

            return successfulTests > 0 ? totalSpeed / successfulTests : 0;
        }


        private void StopTest()
        {
            _cts?.Cancel();
        }

        public string FormatSpeed(double speed)
        {
            if (speed >= 1000)
                return $"{speed / 1000:F2} Gbps";
            return $"{speed:F2} Mbps";
        }

        public string FormatPing(double ping)
        {
            return $"{ping:F0} ms";
        }

        public string GetSpeedColor(double speed, bool isDownload)
        {
            double threshold = isDownload ? 25 : 10;
            if (speed >= threshold * 2) return "Green";
            if (speed >= threshold) return "Orange";
            return "Red";
        }

        public string GetPingColor(double ping)
        {
            if (ping <= 20) return "Green";
            if (ping <= 50) return "Orange";
            return "Red";
        }

        public override void Cleanup()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _httpClient?.Dispose();
        }
    }
}
