using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.Hardware;
using OpenSourceToolkit.NET.Localization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class HardwareToolViewModel : ToolViewModel
    {
        public override int Id => 19;
        public override string Name => ToolkitLocalization.GetString("Tool_Hardware_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_Hardware_Description");
        public override string IconKey => "HardwareIcon";

        public bool IsAudioSupported => PlatformSupport.IsAudioSupported;

        public ICommand PlayToneCommand { get; }

        public HardwareToolViewModel()
        {
            PlayToneCommand = new AsyncRelayCommand(PlayToneAsync, () => IsAudioSupported);
        }

        private async Task PlayToneAsync()
        {
            using (var speaker = new SpeakerTester())
            {
                await speaker.PlayToneAsync(440, 0.5f);
            }
        }
    }
}
