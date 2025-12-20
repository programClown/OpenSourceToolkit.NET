using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenSourceToolkit.AI.Models;

namespace OpenSourceToolkit.AI.Providers
{
    public abstract class BaseProvider : IAiProvider
    {
        protected readonly AiProviderSettings Settings;
        protected readonly HttpClient HttpClient;
        protected static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public abstract AiProviderType ProviderType { get; }
        public abstract bool SupportsMultiModal { get; }
        public abstract bool SupportsStreaming { get; }
        public abstract bool SupportsImageGeneration { get; }

        protected BaseProvider(AiProviderSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            HttpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        }

        public abstract Task<ChatResponse> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default);
        public abstract Task StreamAsync(ChatRequest request, Action<string> onChunk, CancellationToken cancellationToken = default);
        public abstract Task<ImageGenerationResponse> GenerateImageAsync(ImageGenerationRequest request, CancellationToken cancellationToken = default);

        public virtual async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            var testRequest = new ChatRequest("Say 'OK' and nothing else.")
            {
                MaxTokens = 10,
                Temperature = 0
            };
            var response = await CompleteAsync(testRequest, cancellationToken).ConfigureAwait(false);
            return response.IsSuccess;
        }

        protected StringContent CreateJsonContent(object payload)
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        protected async Task<T> ReadJsonResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(content, JsonOptions);
        }
    }
}
