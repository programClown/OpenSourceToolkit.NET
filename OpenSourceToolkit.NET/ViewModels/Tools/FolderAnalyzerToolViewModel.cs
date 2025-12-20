using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.IO;
using OpenSourceToolkit.NET.Localization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class FolderAnalyzerToolViewModel : ToolViewModel
    {
        public override int Id => 17;
        public override string Name => ToolkitLocalization.GetString("Tool_FolderAnalyzer_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_FolderAnalyzer_Description");
        public override string IconKey => "FolderAnalyzerIcon";

        private CancellationTokenSource _cts;

        private string _path;
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        private string _output;
        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        private bool _isAnalyzing;
        public bool IsAnalyzing
        {
            get => _isAnalyzing;
            set
            {
                if (SetProperty(ref _isAnalyzing, value))
                {
                    AnalyzeCommand.NotifyCanExecuteChanged();
                    StopCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _progressText;
        public string ProgressText
        {
            get => _progressText;
            set => SetProperty(ref _progressText, value);
        }

        public RelayCommand AnalyzeCommand { get; }
        public RelayCommand StopCommand { get; }

        public FolderAnalyzerToolViewModel()
        {
            AnalyzeCommand = new RelayCommand(async () => await Analyze(), () => !IsAnalyzing);
            StopCommand = new RelayCommand(Stop, () => IsAnalyzing);
        }

        private void Stop()
        {
            _cts?.Cancel();
        }

        private async Task Analyze()
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                Output = "Please enter a folder path.";
                return;
            }

            _cts = new CancellationTokenSource();
            IsAnalyzing = true;
            Output = "";
            ProgressText = "Starting...";

            try
            {
                var token = _cts.Token;
                FolderAnalyzer.AnalysisResult result = null;

                await Task.Run(() =>
                {
                    result = FolderAnalyzer.Analyze(Path, token, progress =>
                    {
                        ProgressText = $"Scanned {progress.DirectoriesScanned:N0} folders, {progress.FilesScanned:N0} files ({FormatSize(progress.TotalSize)})";
                    });
                });

                Output = FormatNode(result.Root, 0);
                if (result.WasCancelled)
                {
                    ProgressText = $"Cancelled (partial): {result.FinalProgress.DirectoriesScanned:N0} folders, {result.FinalProgress.FilesScanned:N0} files ({FormatSize(result.FinalProgress.TotalSize)})";
                }
                else
                {
                    ProgressText = $"Done: {result.Root.Children?.Count ?? 0:N0} items, {FormatSize(result.Root.Size)}";
                }
            }
            catch (Exception ex)
            {
                Output = $"Error: {ex.Message}";
                ProgressText = "Error";
            }
            finally
            {
                IsAnalyzing = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        private string FormatNode(FolderAnalyzer.FileNode node, int depth)
        {
            var indent = new string(' ', depth * 2);
            var size = FormatSize(node.Size);
            var sb = new System.Text.StringBuilder();

            if (!string.IsNullOrEmpty(node.Error))
            {
                sb.AppendLine($"{indent}{node.Name} [ERROR: {node.Error}]");
            }
            else
            {
                sb.AppendLine($"{indent}{node.Name} ({size})");
            }

            if (node.Children != null)
            {
                if (depth < 2)
                {
                    foreach (var child in node.Children)
                    {
                        sb.Append(FormatNode(child, depth + 1));
                    }
                }
                else if (node.Children.Count > 0)
                {
                     sb.AppendLine($"{indent}  ... {node.Children.Count:N0} items ...");
                }
            }
            return sb.ToString();
        }

        private string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
