using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenSourceToolkit.NET.Views.Tools
{
    public partial class ApiTesterToolView : UserControl
    {
        public ApiTesterToolView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
