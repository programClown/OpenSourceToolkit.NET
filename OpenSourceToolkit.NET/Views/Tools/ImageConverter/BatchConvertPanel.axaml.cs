using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenSourceToolkit.NET.Views.Tools.ImageConverter
{
    /// <summary>
    /// Batch Convert panel for the Image Converter tool.
    /// Expects DataContext to be a BatchConversionViewModel.
    /// </summary>
    public partial class BatchConvertPanel : UserControl
    {
        public BatchConvertPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
