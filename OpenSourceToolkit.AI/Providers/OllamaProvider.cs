using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenSourceToolkit.AI.Models;

namespace OpenSourceToolkit.AI.Providers
{
    public class OllamaProvider : BaseProvider
    {
        public override AiProviderType ProviderType => AiProviderType.Ollama;
        public override bool SupportsMultiModal => true;
        public override bool SupportsStreaming => true;
        public override bool SupportsImageGeneration => false; // Ollama does not support image generation natively

        public OllamaProvider(AiProviderSettings settings) : base(settings)
        {
        }

        public override async Task<ChatResponse> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default)
        {
            var payload = BuildPayload(request, stream: false);
            var endpoint = $"{Settings.Endpoint.TrimEnd('/')}/api/chat";

            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                httpRequest.Content = CreateJsonContent(payload);

                using (var response = await HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false))
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        return ChatResponse.Error($"HTTP {(int)response.StatusCode}: {content}");
                    }

                    using (var doc = JsonDocument.Parse(content))
                    {
                        var root = doc.RootElement;

                        if (root.TryGetProperty("error", out var errorElement))
                        {
                            return ChatResponse.Error(errorElement.GetString());
                        }

                        var message = root.GetProperty("message");
                        var responseContent = message.GetProperty("content").GetString();

                        var result = ChatResponse.Success(responseContent);

                        if (root.TryGetProperty("done_reason", out var doneReason))
                            result.FinishReason = doneReason.GetString();

                        if (root.TryGetProperty("prompt_eval_count", out var promptTokens))
                            result.PromptTokens = promptTokens.GetInt32();

                        if (root.TryGetProperty("eval_count", out var evalCount))
                            result.CompletionTokens = evalCount.GetInt32();

                        return result;
                    }
                }
            }
        }

        public override async Task StreamAsync(ChatRequest request, Action<string> onChunk, CancellationToken cancellationToken = default)
        {
            var payload = BuildPayload(request, stream: true);
            var endpoint = $"{Settings.Endpoint.TrimEnd('/')}/api/chat";

            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                httpRequest.Content = CreateJsonContent(payload);

                using (var response = await HttpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new AiException($"HTTP {(int)response.StatusCode}: {errorContent}");
                    }

                    using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var reader = new StreamReader(stream))
                    {
                        string line;
                        while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                return;

                            if (string.IsNullOrWhiteSpace(line))
                                continue;

                            using (var doc = JsonDocument.Parse(line))
                            {
                                var root = doc.RootElement;

                                if (root.TryGetProperty("done", out var doneEl) && doneEl.GetBoolean())
                                    return;

                                if (root.TryGetProperty("message", out var message))
                                {
                                    if (message.TryGetProperty("content", out var contentEl))
                                    {
                                        var chunk = contentEl.GetString();
                                        if (!string.IsNullOrEmpty(chunk))
                                            onChunk(chunk);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            var endpoint = $"{Settings.Endpoint.TrimEnd('/')}/api/tags";

            using (var response = await HttpClient.GetAsync(endpoint, cancellationToken).ConfigureAwait(false))
            {
                return response.IsSuccessStatusCode;
            }
        }

        private object BuildPayload(ChatRequest request, bool stream)
        {
            var messages = new List<object>();

            foreach (var msg in request.Messages)
            {
                if (msg.Images != null && msg.Images.Count > 0)
                {
                    var images = new List<string>();
                    foreach (var img in msg.Images)
                    {
                        images.Add(Convert.ToBase64String(img.Data));
                    }

                    messages.Add(new
                    {
                        role = msg.Role.ToString().ToLowerInvariant(),
                        content = msg.Content,
                        images = images
                    });
                }
                else
                {
                    messages.Add(new
                    {
                        role = msg.Role.ToString().ToLowerInvariant(),
                        content = msg.Content
                    });
                }
            }

            return new
            {
                model = Settings.ModelId,
                messages = messages,
                stream = stream,
                options = new
                {
                    num_predict = request.MaxTokens ?? Settings.MaxTokens,
                    temperature = request.Temperature ?? Settings.Temperature
                }
            };
        }

        public override Task<ImageGenerationResponse> GenerateImageAsync(ImageGenerationRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ImageGenerationResponse.Error("Ollama does not support image generation."));
        }
    }
}
