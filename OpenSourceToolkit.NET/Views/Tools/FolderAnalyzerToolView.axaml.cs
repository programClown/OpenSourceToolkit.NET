using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using OpenSourceToolkit.NET.ViewModels.Tools;

namespace OpenSourceToolkit.NET.Views.Tools
{
    public partial class FolderAnalyzerToolView : ToolViewBase
    {
        public FolderAnalyzerToolView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void Browse_Click(object sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Folder to Analyze",
                AllowMultiple = false,
                SuggestedStartLocation = await GetStartFolderAsync()
            });

            if (folders.Count >= 1 && DataContext is FolderAnalyzerToolViewModel vm)
            {
                var path = folders[0].Path.LocalPath;
                SaveLastFolderDirect(path);
                vm.Path = path;
            }
        }
    }
}
