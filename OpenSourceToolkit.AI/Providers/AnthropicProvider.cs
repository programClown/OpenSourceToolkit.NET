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
    public class AnthropicProvider : BaseProvider
    {
        private const string AnthropicVersion = "2023-06-01";

        public override AiProviderType ProviderType => AiProviderType.Anthropic;
        public override bool SupportsMultiModal => true;
        public override bool SupportsStreaming => true;
        public override bool SupportsImageGeneration => false; // Anthropic does not support image generation

        public AnthropicProvider(AiProviderSettings settings) : base(settings)
        {
            HttpClient.DefaultRequestHeaders.Add("x-api-key", settings.ApiKey);
            HttpClient.DefaultRequestHeaders.Add("anthropic-version", AnthropicVersion);
        }

        public override async Task<ChatResponse> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default)
        {
            var payload = BuildPayload(request, stream: false);
            var endpoint = $"{Settings.Endpoint.TrimEnd('/')}/messages";

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
                            var errorMsg = errorElement.TryGetProperty("message", out var msgEl)
                                ? msgEl.GetString()
                                : content;
                            return ChatResponse.Error(errorMsg);
                        }

                        var contentArray = root.GetProperty("content");
                        var responseText = "";
                        foreach (var block in contentArray.EnumerateArray())
                        {
                            if (block.TryGetProperty("type", out var typeEl) && typeEl.GetString() == "text")
                            {
                                if (block.TryGetProperty("text", out var textEl))
                                    responseText += textEl.GetString();
                            }
                        }

                        var result = ChatResponse.Success(responseText);

                        if (root.TryGetProperty("stop_reason", out var stopReason))
                            result.FinishReason = stopReason.GetString();

                        if (root.TryGetProperty("usage", out var usage))
                        {
                            if (usage.TryGetProperty("input_tokens", out var it))
                                result.PromptTokens = it.GetInt32();
                            if (usage.TryGetProperty("output_tokens", out var ot))
                                result.CompletionTokens = ot.GetInt32();
                        }

                        return result;
                    }
                }
            }
        }

        public override async Task StreamAsync(ChatRequest request, Action<string> onChunk, CancellationToken cancellationToken = default)
        {
            var payload = BuildPayload(request, stream: true);
            var endpoint = $"{Settings.Endpoint.TrimEnd('/')}/messages";

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

                            if (!line.StartsWith("data: "))
                                continue;

                            var data = line.Substring(6);

                            using (var doc = JsonDocument.Parse(data))
                            {
                                var root = doc.RootElement;
                                if (!root.TryGetProperty("type", out var typeEl))
                                    continue;

                                var eventType = typeEl.GetString();
                                if (eventType == "content_block_delta")
                                {
                                    if (root.TryGetProperty("delta", out var delta))
                                    {
                                        if (delta.TryGetProperty("text", out var textEl))
                                        {
                                            var chunk = textEl.GetString();
                                            if (!string.IsNullOrEmpty(chunk))
                                                onChunk(chunk);
                                        }
                                    }
                                }
                                else if (eventType == "message_stop")
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private object BuildPayload(ChatRequest request, bool stream)
        {
            string systemPrompt = null;
            var messages = new List<object>();

            foreach (var msg in request.Messages)
            {
                if (msg.Role == ChatRole.System)
                {
                    systemPrompt = msg.Content;
                    continue;
                }

                if (msg.Images != null && msg.Images.Count > 0)
                {
                    var contentParts = new List<object>();

                    foreach (var img in msg.Images)
                    {
                        contentParts.Add(new
                        {
                            type = "image",
                            source = new
                            {
                                type = "base64",
                                media_type = img.MimeType,
                                data = Convert.ToBase64String(img.Data)
                            }
                        });
                    }

                    contentParts.Add(new { type = "text", text = msg.Content });

                    messages.Add(new
                    {
                        role = msg.Role == ChatRole.Assistant ? "assistant" : "user",
                        content = contentParts
                    });
                }
                else
                {
                    messages.Add(new
                    {
                        role = msg.Role == ChatRole.Assistant ? "assistant" : "user",
                        content = msg.Content
                    });
                }
            }

            if (systemPrompt != null)
            {
                return new
                {
                    model = Settings.ModelId,
                    system = systemPrompt,
                    messages = messages,
                    max_tokens = request.MaxTokens ?? Settings.MaxTokens,
                    stream = stream
                };
            }

            return new
            {
                model = Settings.ModelId,
                messages = messages,
                max_tokens = request.MaxTokens ?? Settings.MaxTokens,
                stream = stream
            };
        }

        public override Task<ImageGenerationResponse> GenerateImageAsync(ImageGenerationRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ImageGenerationResponse.Error("Anthropic does not support image generation."));
        }
    }
}
