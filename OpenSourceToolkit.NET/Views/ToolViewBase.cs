using Avalonia.Controls;
using Avalonia.Platform.Storage;
using OpenSourceToolkit.NET.ViewModels;
using System.IO;
using System.Threading.Tasks;

namespace OpenSourceToolkit.NET.Views
{
    public class ToolViewBase : UserControl
    {
        /// <summary>
        /// Gets the ViewModel's last folder path as an IStorageFolder for file picker's SuggestedStartLocation.
        /// </summary>
        protected async Task<IStorageFolder> GetStartFolderAsync()
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return null;

            var vm = DataContext as ToolViewModel;
            var lastPath = vm?.LastFolderPath;

            if (!string.IsNullOrEmpty(lastPath) && Directory.Exists(lastPath))
                return await topLevel.StorageProvider.TryGetFolderFromPathAsync(lastPath);

            return null;
        }

        /// <summary>
        /// Saves the folder path from a selected file to the ViewModel's LastFolderPath.
        /// </summary>
        protected void SaveLastFolder(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            var vm = DataContext as ToolViewModel;
            if (vm != null)
                vm.LastFolderPath = Path.GetDirectoryName(filePath);
        }

        /// <summary>
        /// Saves the folder path directly to the ViewModel's LastFolderPath.
        /// </summary>
        protected void SaveLastFolderDirect(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return;
            var vm = DataContext as ToolViewModel;
            if (vm != null)
                vm.LastFolderPath = folderPath;
        }
    }
}
