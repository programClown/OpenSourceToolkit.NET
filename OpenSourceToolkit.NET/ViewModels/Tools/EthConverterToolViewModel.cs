using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.Converters;
using OpenSourceToolkit.NET.Localization;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class EthConverterToolViewModel : ToolViewModel
    {
        public override int Id => 28;
        public override string Name => ToolkitLocalization.GetString("Tool_EthConverter_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_EthConverter_Description");
        public override string IconKey => "EthConverterIcon";

        private decimal _eth = 1;
        public decimal Eth
        {
            get => _eth;
            set => SetProperty(ref _eth, value);
        }

        private string _output;
        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        public ICommand ConvertCommand { get; }

        public EthConverterToolViewModel()
        {
            ConvertCommand = new RelayCommand(Convert);
        }

        private void Convert()
        {
            decimal wei = EthConverter.ToWei(Eth);
            decimal gwei = EthConverter.ToGwei(Eth);
            Output = $"Wei: {wei}\nGwei: {gwei}";
        }
    }
}
