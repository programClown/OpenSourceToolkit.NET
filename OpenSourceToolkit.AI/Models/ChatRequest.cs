using System.Collections.Generic;

namespace OpenSourceToolkit.AI.Models
{
    public class ChatRequest
    {
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public int? MaxTokens { get; set; }
        public double? Temperature { get; set; }
        public bool Stream { get; set; }

        public ChatRequest() { }

        public ChatRequest(string userPrompt)
        {
            Messages.Add(ChatMessage.User(userPrompt));
        }

        public ChatRequest(string systemPrompt, string userPrompt)
        {
            Messages.Add(ChatMessage.System(systemPrompt));
            Messages.Add(ChatMessage.User(userPrompt));
        }

        public ChatRequest WithImage(byte[] imageData, string mimeType = "image/png")
        {
            if (Messages.Count > 0)
            {
                var lastMessage = Messages[Messages.Count - 1];
                if (lastMessage.Role == ChatRole.User)
                {
                    if (lastMessage.Images == null)
                        lastMessage.Images = new List<ImageContent>();
                    lastMessage.Images.Add(new ImageContent(imageData, mimeType));
                }
            }
            return this;
        }
    }
}
