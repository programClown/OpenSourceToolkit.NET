using System;
using System.Collections.Generic;
using System.IO;
using OpenSourceToolkit.NET.Services.Ai;

namespace OpenSourceToolkit.NET.Services
{
    /// <summary>
    /// Application-specific secure storage wrapper.
    /// Uses OpenSourceToolkit.Security.SecureStorage with app-specific paths.
    /// Implements ISecretStorage for use with AiSettingsManager.
    /// </summary>
    public class SecureStorage : ISecretStorage
    {
        private static readonly string SecureStoragePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OpenSourceToolkit.NET",
            ".secrets"
        );

        private const string AppIdentifier = "OpenSourceToolkit.NET.v1";

        private static readonly Lazy<OpenSourceToolkit.Security.SecureStorage> _instance =
            new Lazy<OpenSourceToolkit.Security.SecureStorage>(() =>
                new OpenSourceToolkit.Security.SecureStorage(SecureStoragePath, AppIdentifier));

        private static OpenSourceToolkit.Security.SecureStorage Instance => _instance.Value;

        // Singleton for static access
        private static readonly Lazy<SecureStorage> _singleton = new Lazy<SecureStorage>(() => new SecureStorage());

        /// <summary>
        /// Gets the singleton instance implementing ISecretStorage.
        /// </summary>
        public static SecureStorage Default => _singleton.Value;

        // Prefix for keys stored in secure storage (to identify migrated keys in settings)
        public const string SecureKeyPrefix = "secure:";

        #region ISecretStorage Implementation

        void ISecretStorage.Store(string key, string value) => Instance.Store(key, value);
        string ISecretStorage.Retrieve(string key) => Instance.Retrieve(key);
        void ISecretStorage.Remove(string key) => Instance.Remove(key);
        bool ISecretStorage.Contains(string key) => Instance.Contains(key);
        void ISecretStorage.Clear() => Instance.Clear();

        #endregion

        #region Static API (for backward compatibility)

        /// <summary>
        /// Stores a secret securely using platform-appropriate encryption.
        /// </summary>
        public static void Store(string key, string value) => Instance.Store(key, value);

        /// <summary>
        /// Retrieves a secret from secure storage.
        /// </summary>
        public static string Retrieve(string key) => Instance.Retrieve(key);

        /// <summary>
        /// Removes a secret from secure storage.
        /// </summary>
        public static void Remove(string key) => Instance.Remove(key);

        /// <summary>
        /// Checks if a secret exists in secure storage.
        /// </summary>
        public static bool Contains(string key) => Instance.Contains(key);

        /// <summary>
        /// Gets all stored keys (not values).
        /// </summary>
        public static IEnumerable<string> GetAllKeys() => Instance.GetAllKeys();

        /// <summary>
        /// Clears all stored secrets.
        /// </summary>
        public static void Clear() => Instance.Clear();

        /// <summary>
        /// Reloads secrets from disk, discarding any cached values.
        /// </summary>
        public static void Reload() => Instance.Reload();

        #endregion
    }
}
