using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.Networking;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class UptimeToolViewModel : ToolViewModel
    {
        public override int Id => 13;
        public override string Name => ToolkitLocalization.GetString("Tool_Uptime_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_Uptime_Description");
        public override string IconKey => "UptimeIcon";

        private string _url = "https://google.com";
        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        private string _statusText = "Ready";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private bool _isUp;
        public bool IsUp
        {
            get => _isUp;
            set => SetProperty(ref _isUp, value);
        }

        private long _responseTime;
        public long ResponseTime
        {
            get => _responseTime;
            set => SetProperty(ref _responseTime, value);
        }

        private int _statusCode;
        public int StatusCode
        {
            get => _statusCode;
            set => SetProperty(ref _statusCode, value);
        }

        public ICommand CheckCommand { get; }

        private readonly UptimeMonitor _monitor;

        public UptimeToolViewModel()
        {
            _monitor = new UptimeMonitor();
            CheckCommand = new RelayCommand(async () => await Check());
        }

        private async Task Check()
        {
            StatusText = "Checking...";
            try
            {
                var result = await _monitor.CheckAsync(Url);
                IsUp = result.IsUp;
                StatusCode = result.StatusCode;
                ResponseTime = result.ResponseTimeMs;
                StatusText = result.IsUp ? "UP" : "DOWN";

                if (!result.IsUp && !string.IsNullOrEmpty(result.ErrorMessage))
                {
                    StatusText += $" ({result.ErrorMessage})";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
        }
    }
}
