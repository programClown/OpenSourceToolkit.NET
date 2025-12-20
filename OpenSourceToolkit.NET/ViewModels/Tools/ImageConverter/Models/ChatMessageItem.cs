using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OpenSourceToolkit.NET.ViewModels.Tools.ImageConverter.Models
{
    /// <summary>
    /// Represents a single chat message in the AI assistant conversation.
    /// </summary>
    public sealed class ChatMessageItem : ObservableObject
    {
        /// <summary>Message role: User, Assistant, or System.</summary>
        public ChatMessageRole Role { get; set; }

        private string _content = "";
        /// <summary>Message text content. Can be updated during streaming.</summary>
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        /// <summary>Timestamp when the message was created.</summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>Optional footer text (e.g., "Generating...", "Delivered", image count).</summary>
        public string Footer { get; set; }

        /// <summary>Whether this is an error message.</summary>
        public bool IsError { get; set; }

        /// <summary>Whether this message was cancelled.</summary>
        public bool IsCancelled { get; set; }

        /// <summary>Whether this is a success/info message.</summary>
        public bool IsSuccess { get; set; }

        /// <summary>For AI messages: whether the message is still being streamed.</summary>
        public bool IsStreaming { get; set; }

        /// <summary>
        /// Creates a user message.
        /// </summary>
        public static ChatMessageItem User(string content)
        {
            return new ChatMessageItem
            {
                Role = ChatMessageRole.User,
                Content = content,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// Creates an AI assistant message.
        /// </summary>
        public static ChatMessageItem Assistant(string content = "", bool isStreaming = false)
        {
            return new ChatMessageItem
            {
                Role = ChatMessageRole.Assistant,
                Content = content,
                IsStreaming = isStreaming,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// Creates a system message (info, error, cancelled).
        /// </summary>
        public static ChatMessageItem System(string content, bool isError = false, bool isCancelled = false, bool isSuccess = false)
        {
            return new ChatMessageItem
            {
                Role = ChatMessageRole.System,
                Content = content,
                IsError = isError,
                IsCancelled = isCancelled,
                IsSuccess = isSuccess,
                Timestamp = DateTime.Now
            };
        }
    }

    /// <summary>
    /// Chat message role.
    /// </summary>
    public enum ChatMessageRole
    {
        User,
        Assistant,
        System
    }

    /// <summary>
    /// Serializable data transfer object for chat message persistence.
    /// </summary>
    public class ChatMessageData
    {
        public string Role { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsError { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsSuccess { get; set; }
    }
}
