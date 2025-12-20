namespace OpenSourceToolkit.NET.Services.Ai
{
    /// <summary>
    /// Interface for secure secret storage. Implementations should use platform-appropriate
    /// encryption (DPAPI on Windows, Keychain on macOS, etc.)
    /// </summary>
    public interface ISecretStorage
    {
        void Store(string key, string value);
        string Retrieve(string key);
        void Remove(string key);
        bool Contains(string key);
        void Clear();
    }
}
