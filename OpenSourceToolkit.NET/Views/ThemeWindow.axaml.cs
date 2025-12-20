#nullable enable
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using OpenSourceToolkit.NET.ViewModels.Tools;

namespace OpenSourceToolkit.NET.Views
{
    public partial class ThemeWindow : Window
    {
        public ThemeWindow()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = new ThemeSelectionToolViewModel();

            // Close window on ESC key press
            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
