using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSourceToolkit.NET.Services.Ai;

namespace OpenSourceToolkit.Tests
{
    [TestClass]
    public class AiTests
    {
        #region Mocks

        class MockSecretStorage : ISecretStorage
        {
            private readonly Dictionary<string, string> _store = new Dictionary<string, string>();

            public void Store(string key, string value) => _store[key] = value;
            public string Retrieve(string key) => _store.TryGetValue(key, out var val) ? val : null;
            public void Remove(string key) => _store.Remove(key);
            public bool Contains(string key) => _store.ContainsKey(key);
            public void Clear() => _store.Clear();
        }

        #endregion

        [TestMethod]
        public void AiSettingsManager_SecurelyStoresKeys()
        {
            var storage = new MockSecretStorage();
            var manager = new AiSettingsManager(storage);
            var key = "secret-key-123";

            manager.SetProviderApiKey("OpenAI", key);

            Assert.IsTrue(storage.Contains("provider.OpenAI.apikey"));
            Assert.AreEqual(key, storage.Retrieve("provider.OpenAI.apikey"));

            var provider = manager.GetOrCreateProvider("OpenAI");
            Assert.IsTrue(provider.ApiKey.StartsWith(AiSettingsManager.SecureKeyPrefix));
            Assert.IsFalse(provider.ApiKey.Contains(key));

            Assert.AreEqual(key, manager.GetProviderApiKey("OpenAI"));
        }

        [TestMethod]
        public void AiSettingsManager_MigratesPlainKeys()
        {
            var storage = new MockSecretStorage();
            var manager = new AiSettingsManager(storage);

            var provider = manager.GetOrCreateProvider("OpenAI");
            provider.ApiKey = "plain-text-key";

            var migrated = manager.MigrateToSecureStorage();

            Assert.IsTrue(migrated);
            Assert.IsTrue(provider.ApiKey.StartsWith(AiSettingsManager.SecureKeyPrefix));
            Assert.AreEqual("plain-text-key", storage.Retrieve("provider.OpenAI.apikey"));
        }

        [TestMethod]
        public void AiSettingsManager_ConnectionKeys_OverrideProviderKeys()
        {
            var storage = new MockSecretStorage();
            var manager = new AiSettingsManager(storage);

            manager.SetProviderApiKey("OpenAI", "provider-key");
            var conn = manager.AddConnection("Test", "OpenAI", "gpt-4");

            Assert.AreEqual("provider-key", manager.GetEffectiveApiKey(conn.Id));

            manager.SetConnectionApiKey(conn.Id, "connection-key");
            Assert.AreEqual("connection-key", manager.GetEffectiveApiKey(conn.Id));
        }

        [TestMethod]
        public void AiSettingsManager_ManagesProviderModels()
        {
            var storage = new MockSecretStorage();
            var manager = new AiSettingsManager(storage);

            var defaults = manager.GetProviderModels("OpenAI");
            Assert.IsTrue(defaults.Count > 0);

            manager.AddProviderModel("OpenAI", "custom-gpt");
            var updated = manager.GetProviderModels("OpenAI");
            Assert.IsTrue(updated.Contains("custom-gpt"));

            manager.ResetProviderModels("OpenAI");
            var reset = manager.GetProviderModels("OpenAI");
            Assert.IsFalse(reset.Contains("custom-gpt"));
        }

        [TestMethod]
        public void AiConnectionConfig_CreateDefault_ReturnsCorrectEndpoints()
        {
            var openai = AiConnectionConfig.CreateDefault(AiProviderType.OpenAI);
            Assert.AreEqual("https://api.openai.com/v1", openai.Endpoint);

            var anthropic = AiConnectionConfig.CreateDefault(AiProviderType.Anthropic);
            Assert.AreEqual("https://api.anthropic.com/v1", anthropic.Endpoint);

            var google = AiConnectionConfig.CreateDefault(AiProviderType.Google);
            Assert.AreEqual("https://generativelanguage.googleapis.com/v1beta", google.Endpoint);

            var ollama = AiConnectionConfig.CreateDefault(AiProviderType.Ollama);
            Assert.AreEqual("http://localhost:11434", ollama.Endpoint);
        }

        [TestMethod]
        public void AiConnectionConfig_IsImageGenerationModel_DetectsCorrectly()
        {
            Assert.IsTrue(AiConnectionConfig.IsImageGenerationModel(AiProviderType.OpenAI, "gpt-image-1"));
            Assert.IsFalse(AiConnectionConfig.IsImageGenerationModel(AiProviderType.OpenAI, "gpt-4o"));

            Assert.IsTrue(AiConnectionConfig.IsImageGenerationModel(AiProviderType.OpenRouter, "google/gemini-2.5-flash-image"));
            Assert.IsTrue(AiConnectionConfig.IsImageGenerationModel(AiProviderType.OpenRouter, "black-forest-labs/flux-pro"));

            Assert.IsTrue(AiConnectionConfig.IsImageGenerationModel(AiProviderType.Google, "imagen-3.0-generate-002"));
            Assert.IsFalse(AiConnectionConfig.IsImageGenerationModel(AiProviderType.Google, "gemini-2.5-pro"));
        }

        [TestMethod]
        public void AiConnection_Clone_CreatesIndependentCopy()
        {
            var original = new AiConnection
            {
                Name = "Test",
                ProviderType = "OpenAI",
                ModelId = "gpt-4",
                MaxTokens = 8000,
                Temperature = 0.5
            };

            var clone = original.Clone();

            Assert.AreEqual(original.Id, clone.Id);
            Assert.AreEqual(original.Name, clone.Name);
            Assert.AreEqual(original.MaxTokens, clone.MaxTokens);

            clone.Name = "Modified";
            Assert.AreNotEqual(original.Name, clone.Name);
        }

        [TestMethod]
        public void AiSettingsManager_RemoveConnection_CleansUpSecureStorage()
        {
            var storage = new MockSecretStorage();
            var manager = new AiSettingsManager(storage);

            var conn = manager.AddConnection("Test", "OpenAI", "gpt-4", "custom-key");
            var connId = conn.Id;

            Assert.IsTrue(storage.Contains($"connection.{connId}.apikey"));

            manager.RemoveConnection(connId);

            Assert.IsFalse(storage.Contains($"connection.{connId}.apikey"));
            Assert.AreEqual(0, manager.Connections.Count);
        }

        [TestMethod]
        public void AiSettingsManager_CreateConfigFromConnection_ReturnsCorrectConfig()
        {
            var storage = new MockSecretStorage();
            var manager = new AiSettingsManager(storage);

            manager.SetProviderApiKey("OpenAI", "provider-key");
            var conn = manager.AddConnection("Test", "OpenAI", "gpt-4o");
            conn.MaxTokens = 16000;
            conn.Temperature = 0.3;

            var config = manager.CreateConfigFromConnection(conn.Id);

            Assert.IsNotNull(config);
            Assert.AreEqual(AiProviderType.OpenAI, config.ProviderType);
            Assert.AreEqual("provider-key", config.ApiKey);
            Assert.AreEqual("gpt-4o", config.ModelId);
            Assert.AreEqual(16000, config.MaxTokens);
            Assert.AreEqual(0.3, config.Temperature, 0.001);
        }
    }
}
