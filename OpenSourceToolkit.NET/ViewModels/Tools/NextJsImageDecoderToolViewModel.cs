using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.Media;
using OpenSourceToolkit.NET.Localization;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class NextJsImageDecoderToolViewModel : ToolViewModel
    {
        public override int Id => 24;
        public override string Name => ToolkitLocalization.GetString("Tool_NextJsImageDecoder_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_NextJsImageDecoder_Description");
        public override string IconKey => "NextJsImageIcon";

        private string _inputUrl;
        public string InputUrl
        {
            get => _inputUrl;
            set => SetProperty(ref _inputUrl, value);
        }

        private string _output;
        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        public ICommand DecodeCommand { get; }
        public ICommand LoadExampleCommand { get; }

        public NextJsImageDecoderToolViewModel()
        {
            DecodeCommand = new RelayCommand(Decode);
            LoadExampleCommand = new RelayCommand(LoadExample);
        }

        private void LoadExample()
        {
            InputUrl = "https://example.com/_next/image?url=%2Fimg.jpg&w=640&q=75";
        }

        private void Decode()
        {
            var info = NextJsImageUrlParser.Parse(InputUrl);
            if (info.IsValid)
            {
                Output = $"Original URL: {info.OriginalUrl}\nWidth: {info.Width}\nQuality: {info.Quality}";
            }
            else
            {
                Output = "Invalid Next.js Image URL";
            }
        }
    }
}
