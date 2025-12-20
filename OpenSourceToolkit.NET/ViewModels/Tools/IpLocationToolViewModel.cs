using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using System.Threading.Tasks;
using System.Windows.Input;
using OpenSourceToolkit.Networking;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class IpLocationToolViewModel : ToolViewModel
    {
        public override int Id => 15;
        public override string Name => ToolkitLocalization.GetString("Tool_IpLocation_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_IpLocation_Description");
        public override string IconKey => "IpLocationIcon";

        private string _ipAddress = "8.8.8.8";
        public string IpAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }

        private string _output;
        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        public ICommand LookupCommand { get; }

        private readonly IIpGeolocationProvider _provider;

        public IpLocationToolViewModel()
        {
            _provider = new DummyIpGeolocationProvider();
            LookupCommand = new RelayCommand(async () => await Lookup());
        }

        private async Task Lookup()
        {
            if (string.IsNullOrWhiteSpace(IpAddress)) return;

            Output = "Locating...";
            var result = await _provider.GetLocationAsync(IpAddress);

            Output = $"IP: {result.Ip}\n" +
                     $"Country: {result.Country}\n" +
                     $"Region: {result.Region}\n" +
                     $"City: {result.City}\n" +
                     $"ISP: {result.Isp}\n" +
                     $"Timezone: {result.Timezone}\n" +
                     $"Coordinates: {result.Latitude}, {result.Longitude}";
        }
    }
}
