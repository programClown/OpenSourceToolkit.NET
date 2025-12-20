using System;
using System.Collections.Generic;

namespace OpenSourceToolkit.NET.Services.Ai
{
    public class AiConnectionConfig
    {
        public AiProviderType ProviderType { get; set; }
        public string ApiKey { get; set; }
        public string Endpoint { get; set; }
        public string ModelId { get; set; }
        public int MaxTokens { get; set; } = 4096;
        public double Temperature { get; set; } = 0.7;

        public static AiConnectionConfig CreateDefault(AiProviderType providerType)
        {
            switch (providerType)
            {
                case AiProviderType.OpenAI:
                    return new AiConnectionConfig
                    {
                        ProviderType = AiProviderType.OpenAI,
                        Endpoint = "https://api.openai.com/v1",
                        ModelId = "gpt-5.1"
                    };
                case AiProviderType.OpenRouter:
                    return new AiConnectionConfig
                    {
                        ProviderType = AiProviderType.OpenRouter,
                        Endpoint = "https://openrouter.ai/api/v1",
                        ModelId = "anthropic/claude-sonnet-4.5"
                    };
                case AiProviderType.Anthropic:
                    return new AiConnectionConfig
                    {
                        ProviderType = AiProviderType.Anthropic,
                        Endpoint = "https://api.anthropic.com/v1",
                        ModelId = "claude-opus-4-5-20251101"
                    };
                case AiProviderType.Google:
                    return new AiConnectionConfig
                    {
                        ProviderType = AiProviderType.Google,
                        Endpoint = "https://generativelanguage.googleapis.com/v1beta",
                        ModelId = "gemini-3-pro-preview"
                    };
                case AiProviderType.Ollama:
                    return new AiConnectionConfig
                    {
                        ProviderType = AiProviderType.Ollama,
                        Endpoint = "http://localhost:11434",
                        ModelId = "llama3.2"
                    };
                case AiProviderType.LMStudio:
                    return new AiConnectionConfig
                    {
                        ProviderType = AiProviderType.LMStudio,
                        Endpoint = "http://localhost:1234/v1",
                        ModelId = "local-model"
                    };
                default:
                    return new AiConnectionConfig { ProviderType = providerType };
            }
        }

        public static List<string> GetDefaultModels(AiProviderType providerType)
        {
            switch (providerType)
            {
                case AiProviderType.OpenAI:
                    return new List<string>
                    {
                        "gpt-5.1",
                        "gpt-5.1-mini",
                        "gpt-5.1-nano",
                        "gpt-4o",
                        "gpt-4o-mini",
                        "gpt-4-turbo",
                        "gpt-4",
                        "gpt-3.5-turbo",
                        "o3",
                        "o3-mini",
                        "o4-mini",
                        "o1",
                        "o1-mini"
                    };
                case AiProviderType.OpenRouter:
                    return new List<string>
                    {
                        "anthropic/claude-sonnet-4.5",
                        "anthropic/claude-opus-4.5",
                        "openai/gpt-5.1",
                        "openai/o3",
                        "openai/o4-mini",
                        "google/gemini-3-pro",
                        "google/gemini-3-flash"
                    };
                case AiProviderType.Anthropic:
                    return new List<string>
                    {
                        "claude-opus-4-5-20251101",
                        "claude-sonnet-4-5-20251101"
                    };
                case AiProviderType.Google:
                    return new List<string>
                    {
                        "gemini-3-pro-preview",
                        "gemini-2.5-flash-preview"
                    };
                case AiProviderType.Ollama:
                    return new List<string>
                    {
                        "llama3.2",
                        "llama3.2:1b",
                        "llama3.1",
                        "llama3.1:70b",
                        "gemma2",
                        "gemma2:27b",
                        "mistral",
                        "mixtral",
                        "qwen2.5",
                        "phi3",
                        "codellama",
                        "llava",
                        "llava:34b"
                    };
                case AiProviderType.LMStudio:
                    return new List<string>
                    {
                        "local-model"
                    };
                default:
                    return new List<string>();
            }
        }

        public static string GetDefaultEndpoint(AiProviderType providerType)
        {
            return CreateDefault(providerType).Endpoint;
        }

        public static List<string> GetDefaultImageModels(AiProviderType providerType)
        {
            switch (providerType)
            {
                case AiProviderType.OpenAI:
                    return new List<string>
                    {
                        "gpt-image-1-mini",
                        "gpt-image-1"
                    };
                case AiProviderType.OpenRouter:
                    return new List<string>
                    {
                        "google/gemini-2.5-flash-image",
                        "black-forest-labs/flux-pro"
                    };
                case AiProviderType.Google:
                    return new List<string>
                    {
                        "imagen-3.0-generate-002",
                        "imagen-3.0-fast-generate-001"
                    };
                default:
                    return new List<string>();
            }
        }

        public static bool IsImageGenerationModel(AiProviderType providerType, string modelId)
        {
            if (string.IsNullOrEmpty(modelId))
                return false;

            var modelLower = modelId.ToLowerInvariant();

            switch (providerType)
            {
                case AiProviderType.OpenAI:
                    return modelLower.Contains("gpt-image");
                case AiProviderType.OpenRouter:
                    return modelLower.Contains("-image") || modelLower.Contains("flux");
                case AiProviderType.Google:
                    return modelLower.Contains("imagen");
                default:
                    return false;
            }
        }
    }

    public class AiConnection
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ProviderType { get; set; }
        public string ModelId { get; set; }
        public string CustomApiKey { get; set; }
        public string CustomEndpoint { get; set; }
        public int MaxTokens { get; set; } = 4096;
        public double Temperature { get; set; } = 0.7;
        public bool SupportsMultiModalInput { get; set; }
        public bool SupportsImageGeneration { get; set; }

        public AiConnection()
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        public AiConnection Clone()
        {
            return new AiConnection
            {
                Id = this.Id,
                Name = this.Name,
                ProviderType = this.ProviderType,
                ModelId = this.ModelId,
                CustomApiKey = this.CustomApiKey,
                CustomEndpoint = this.CustomEndpoint,
                MaxTokens = this.MaxTokens,
                Temperature = this.Temperature,
                SupportsMultiModalInput = this.SupportsMultiModalInput,
                SupportsImageGeneration = this.SupportsImageGeneration
            };
        }
    }

    public class AiProviderConfig
    {
        public string ProviderType { get; set; }
        public string ApiKey { get; set; }
        public string Endpoint { get; set; }
        public List<string> AvailableModels { get; set; } = new List<string>();
    }
}
