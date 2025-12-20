using System;
using System.Threading;
using System.Threading.Tasks;
using OpenSourceToolkit.AI.Models;

namespace OpenSourceToolkit.AI
{
    public interface IAiProvider
    {
        AiProviderType ProviderType { get; }
        bool SupportsMultiModal { get; }
        bool SupportsStreaming { get; }
        bool SupportsImageGeneration { get; }

        Task<ChatResponse> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default);
        Task StreamAsync(ChatRequest request, Action<string> onChunk, CancellationToken cancellationToken = default);
        Task<ImageGenerationResponse> GenerateImageAsync(ImageGenerationRequest request, CancellationToken cancellationToken = default);
        Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
    }
}
