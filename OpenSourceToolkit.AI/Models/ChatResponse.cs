using System.Collections.Generic;

namespace OpenSourceToolkit.AI.Models
{
    public class ChatResponse
    {
        public string Content { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
        public string FinishReason { get; set; }

        /// <summary>
        /// Generated images (if the response includes image generation)
        /// </summary>
        public List<GeneratedImage> GeneratedImages { get; set; }

        /// <summary>
        /// Returns true if this response contains generated images
        /// </summary>
        public bool HasGeneratedImages => GeneratedImages != null && GeneratedImages.Count > 0;

        public static ChatResponse Success(string content) =>
            new ChatResponse { Content = content, IsSuccess = true };

        public static ChatResponse Error(string errorMessage) =>
            new ChatResponse { ErrorMessage = errorMessage, IsSuccess = false };
    }
}
