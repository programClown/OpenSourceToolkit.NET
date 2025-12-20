using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace OpenSourceToolkit.IO
{
    public class FolderAnalyzer
    {
        public class FileNode
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public long Size { get; set; }
            public string Type { get; set; } // "file" or "directory"
            public string Error { get; set; }
            public List<FileNode> Children { get; set; } = new List<FileNode>();
        }

        public class AnalysisProgress
        {
            public int DirectoriesScanned { get; set; }
            public int FilesScanned { get; set; }
            public long TotalSize { get; set; }
            public string CurrentPath { get; set; }
        }

        public class AnalysisResult
        {
            public FileNode Root { get; set; }
            public bool WasCancelled { get; set; }
            public AnalysisProgress FinalProgress { get; set; }
        }

        public static AnalysisResult Analyze(string path, CancellationToken cancellationToken = default, Action<AnalysisProgress> onProgress = null)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory not found: {path}");

            var progress = new AnalysisProgress();
            var root = new FileNode
            {
                Name = Path.GetFileName(path),
                Path = path,
                Type = "directory",
                Size = 0
            };

            var wasCancelled = !AnalyzeRecursive(root, cancellationToken, progress, onProgress);
            return new AnalysisResult { Root = root, WasCancelled = wasCancelled, FinalProgress = progress };
        }

        private static bool AnalyzeRecursive(FileNode node, CancellationToken cancellationToken, AnalysisProgress progress, Action<AnalysisProgress> onProgress)
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            try
            {
                var dirInfo = new DirectoryInfo(node.Path);
                progress.CurrentPath = node.Path;
                progress.DirectoriesScanned++;
                onProgress?.Invoke(progress);

                foreach (var dir in dirInfo.GetDirectories())
                {
                    if (cancellationToken.IsCancellationRequested)
                        return false;

                    var childNode = new FileNode
                    {
                        Name = dir.Name,
                        Path = dir.FullName,
                        Type = "directory",
                        Size = 0
                    };
                    if (!AnalyzeRecursive(childNode, cancellationToken, progress, onProgress))
                        return false;
                    node.Children.Add(childNode);
                    node.Size += childNode.Size;
                }

                foreach (var file in dirInfo.GetFiles())
                {
                    if (cancellationToken.IsCancellationRequested)
                        return false;

                    var childNode = new FileNode
                    {
                        Name = file.Name,
                        Path = file.FullName,
                        Type = "file",
                        Size = file.Length
                    };
                    node.Children.Add(childNode);
                    node.Size += childNode.Size;

                    progress.FilesScanned++;
                    progress.TotalSize += file.Length;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                node.Error = $"Access denied: {ex.Message}";
            }
            catch (IOException ex)
            {
                node.Error = $"IO error: {ex.Message}";
            }
            return true;
        }
    }
}
