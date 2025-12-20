using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenSourceToolkit.NET.Views.Tools
{
    public partial class AudioNoiseReductionToolView : UserControl
    {
        public AudioNoiseReductionToolView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
