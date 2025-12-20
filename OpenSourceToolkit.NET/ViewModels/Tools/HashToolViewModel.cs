using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.Security;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class HashToolViewModel : ToolViewModel
    {
        public override int Id => 8;
        public override string Name => ToolkitLocalization.GetString("Tool_Hash_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_Hash_Description");
        public override string IconKey => "HashIcon";

        private string _input;
        public string Input
        {
            get => _input;
            set => SetProperty(ref _input, value);
        }

        private string _outputMd5;
        public string OutputMd5
        {
            get => _outputMd5;
            set => SetProperty(ref _outputMd5, value);
        }

        private string _outputSha256;
        public string OutputSha256
        {
            get => _outputSha256;
            set => SetProperty(ref _outputSha256, value);
        }

        public ICommand ComputeCommand { get; }

        public HashToolViewModel()
        {
            ComputeCommand = new RelayCommand(Compute);
        }

        private void Compute()
        {
            if (string.IsNullOrEmpty(Input)) return;
            OutputMd5 = HashGenerator.ComputeMd5(Input);
            OutputSha256 = HashGenerator.ComputeSha256(Input);
        }
    }
}
