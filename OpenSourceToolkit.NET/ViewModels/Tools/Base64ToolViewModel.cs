using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.Converters;
using OpenSourceToolkit.NET.Localization;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class Base64ToolViewModel : ToolViewModel
    {
        public override int Id => 11;
        public override string Name => ToolkitLocalization.GetString("Tool_Base64_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_Base64_Description");
        public override string IconKey => "Base64Icon";

        private string _input;
        public string Input
        {
            get => _input;
            set => SetProperty(ref _input, value);
        }

        private string _output;
        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        public ICommand EncodeCommand { get; }
        public ICommand DecodeCommand { get; }

        public Base64ToolViewModel()
        {
            EncodeCommand = new RelayCommand(Encode);
            DecodeCommand = new RelayCommand(Decode);
        }

        private void Encode()
        {
            if (string.IsNullOrEmpty(Input)) return;
            Output = Base64Converter.Encode(Input);
        }

        private void Decode()
        {
            if (string.IsNullOrEmpty(Input)) return;
            try {
                Output = Base64Converter.Decode(Input);
            } catch {
                Output = "Error: Invalid Base64";
            }
        }
    }
}
