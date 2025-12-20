using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.Networking;
using System;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class IpCalculatorToolViewModel : ToolViewModel
    {
        public override int Id => 22;
        public override string Name => ToolkitLocalization.GetString("Tool_IpCalculator_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_IpCalculator_Description");
        public override string IconKey => "IpCalculatorIcon";

        private string _ipAddress = "192.168.1.10";
        public string IpAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }

        private string _subnetMask = "255.255.255.0";
        public string SubnetMask
        {
            get => _subnetMask;
            set => SetProperty(ref _subnetMask, value);
        }

        private string _output;
        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        public ICommand CalculateCommand { get; }

        public IpCalculatorToolViewModel()
        {
            CalculateCommand = new RelayCommand(Calculate);
        }

        private void Calculate()
        {
            try
            {
                var result = IpCalculator.Calculate(IpAddress, SubnetMask);
                Output = $"Network: {result.NetworkAddress}\nBroadcast: {result.BroadcastAddress}\nHosts: {result.NumberOfHosts}";
            }
            catch (Exception ex)
            {
                Output = $"Error: {ex.Message}";
            }
        }
    }
}
