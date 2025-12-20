using System;
using System.Threading;
using System.Threading.Tasks;
using OpenSourceToolkit.AI.Models;

namespace OpenSourceToolkit.AI
{
    public interface IAiService
    {
        bool IsConfigured { get; }
        AiProviderType? CurrentProvider { get; }
        bool SupportsImageGeneration { get; }

        Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken = default);
        Task<string> CompleteAsync(string prompt, byte[] imageData, string mimeType = "image/png", CancellationToken cancellationToken = default);
        Task<ChatResponse> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default);
        Task StreamAsync(string prompt, Action<string> onChunk, CancellationToken cancellationToken = default);
        Task StreamAsync(ChatRequest request, Action<string> onChunk, CancellationToken cancellationToken = default);
        Task<ImageGenerationResponse> GenerateImageAsync(string prompt, CancellationToken cancellationToken = default);
        Task<ImageGenerationResponse> GenerateImageAsync(ImageGenerationRequest request, CancellationToken cancellationToken = default);
        Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);

        void Configure(AiProviderSettings settings);
    }
}
