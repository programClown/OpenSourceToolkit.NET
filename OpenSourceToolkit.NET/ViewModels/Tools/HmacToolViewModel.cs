using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.Security;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class HmacToolViewModel : ToolViewModel
    {
        public override int Id => 9;
        public override string Name => ToolkitLocalization.GetString("Tool_Hmac_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_Hmac_Description");
        public override string IconKey => "HmacIcon";

        private string _input;
        public string Input
        {
            get => _input;
            set => SetProperty(ref _input, value);
        }

        private string _key;
        public string Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }

        private string _outputSha256;
        public string OutputSha256
        {
            get => _outputSha256;
            set => SetProperty(ref _outputSha256, value);
        }

        private string _outputSha512;
        public string OutputSha512
        {
            get => _outputSha512;
            set => SetProperty(ref _outputSha512, value);
        }

        public ICommand GenerateCommand { get; }

        public HmacToolViewModel()
        {
            GenerateCommand = new RelayCommand(Generate);
        }

        private void Generate()
        {
            if (string.IsNullOrEmpty(Input) || string.IsNullOrEmpty(Key)) return;

            OutputSha256 = HmacGenerator.GenerateHmacSha256(Input, Key);
            OutputSha512 = HmacGenerator.GenerateHmacSha512(Input, Key);
        }
    }
}
