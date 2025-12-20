using System.Collections.Generic;

namespace OpenSourceToolkit.AI.Models
{
    public class ChatMessage
    {
        public ChatRole Role { get; set; }
        public string Content { get; set; }
        public List<ImageContent> Images { get; set; }

        public static ChatMessage System(string content) =>
            new ChatMessage { Role = ChatRole.System, Content = content };

        public static ChatMessage User(string content) =>
            new ChatMessage { Role = ChatRole.User, Content = content };

        public static ChatMessage User(string content, byte[] imageData, string mimeType = "image/png") =>
            new ChatMessage
            {
                Role = ChatRole.User,
                Content = content,
                Images = new List<ImageContent> { new ImageContent(imageData, mimeType) }
            };

        public static ChatMessage Assistant(string content) =>
            new ChatMessage { Role = ChatRole.Assistant, Content = content };
    }

    public enum ChatRole
    {
        System,
        User,
        Assistant
    }

    public class ImageContent
    {
        public byte[] Data { get; set; }
        public string MimeType { get; set; }

        public ImageContent() { }

        public ImageContent(byte[] data, string mimeType)
        {
            Data = data;
            MimeType = mimeType;
        }

        public string ToBase64DataUrl()
        {
            var base64 = System.Convert.ToBase64String(Data);
            return $"data:{MimeType};base64,{base64}";
        }
    }
}
