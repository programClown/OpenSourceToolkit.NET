using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using OpenSourceToolkit.NET.ViewModels.Tools;
using System.IO;

namespace OpenSourceToolkit.NET.Views.Tools
{
    public partial class JsonFormatterToolView : ToolViewBase
    {
        public JsonFormatterToolView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnDataContextChanged(System.EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is JsonFormatterToolViewModel vm)
            {
                vm.PickFileAction = async () =>
                {
                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel == null) return null;

                    var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "Open JSON File",
                        AllowMultiple = false,
                        FileTypeFilter = new[]
                        {
                            new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } },
                            new FilePickerFileType("XML Files") { Patterns = new[] { "*.xml" } },
                            new FilePickerFileType("YAML Files") { Patterns = new[] { "*.yaml", "*.yml" } },
                            new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
                        },
                        SuggestedStartLocation = await GetStartFolderAsync()
                    });

                    if (files != null && files.Count > 0)
                    {
                        var file = files[0];
                        SaveLastFolder(file.Path.LocalPath);
                        using (var stream = await file.OpenReadAsync())
                        using (var reader = new StreamReader(stream))
                        {
                            return await reader.ReadToEndAsync();
                        }
                    }
                    return null;
                };
            }
        }
    }
}
