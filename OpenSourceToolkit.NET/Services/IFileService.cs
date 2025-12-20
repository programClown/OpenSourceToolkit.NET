using System.IO;
using System.Threading.Tasks;

namespace OpenSourceToolkit.NET.Services
{
    /// <summary>
    /// Abstraction for file system operations.
    /// Allows ViewModels to be tested without actual file I/O.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Reads all bytes from a file.
        /// </summary>
        byte[] ReadAllBytes(string path);

        /// <summary>
        /// Writes all bytes to a file.
        /// </summary>
        void WriteAllBytes(string path, byte[] bytes);

        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        bool FileExists(string path);

        /// <summary>
        /// Gets file info (name, size, etc.)
        /// </summary>
        FileInfo GetFileInfo(string path);
    }

    /// <summary>
    /// Production implementation using System.IO.
    /// </summary>
    public class FileService : IFileService
    {
        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public void WriteAllBytes(string path, byte[] bytes)
        {
            File.WriteAllBytes(path, bytes);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public FileInfo GetFileInfo(string path)
        {
            return new FileInfo(path);
        }
    }
}
