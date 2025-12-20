using System.Collections.Generic;
using System.IO;
using OpenSourceToolkit.NET.Services;

namespace OpenSourceToolkit.Tests.Mocks
{
    /// <summary>
    /// Mock file service for testing. Stores files in memory.
    /// </summary>
    public class MockFileService : IFileService
    {
        private readonly Dictionary<string, byte[]> _files = new Dictionary<string, byte[]>();
        private readonly Dictionary<string, MockFileInfo> _fileInfos = new Dictionary<string, MockFileInfo>();

        /// <summary>
        /// Gets all files that were written during the test.
        /// </summary>
        public IReadOnlyDictionary<string, byte[]> WrittenFiles => _files;

        /// <summary>
        /// Pre-populates a file for reading.
        /// </summary>
        public void SetupFile(string path, byte[] content)
        {
            _files[path] = content;
            _fileInfos[path] = new MockFileInfo(path, content.Length);
        }

        public byte[] ReadAllBytes(string path)
        {
            if (_files.TryGetValue(path, out var bytes))
                return bytes;
            throw new FileNotFoundException($"File not found: {path}");
        }

        public void WriteAllBytes(string path, byte[] bytes)
        {
            _files[path] = bytes;
            _fileInfos[path] = new MockFileInfo(path, bytes.Length);
        }

        public bool FileExists(string path)
        {
            return _files.ContainsKey(path);
        }

        public FileInfo GetFileInfo(string path)
        {
            // Note: FileInfo requires an actual path, so we return a mock wrapper
            // In real tests, you might want to create a IFileInfo interface
            return new FileInfo(path);
        }
    }

    internal class MockFileInfo
    {
        public string Path { get; }
        public long Length { get; }

        public MockFileInfo(string path, long length)
        {
            Path = path;
            Length = length;
        }
    }
}
