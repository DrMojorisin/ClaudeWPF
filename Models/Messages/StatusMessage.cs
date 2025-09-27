using System;

namespace WPFBase.Models.Messages
{
    /// <summary>
    /// Message for status updates
    /// </summary>
    public class StatusMessage
    {
        /// <summary>
        /// Gets or sets the status text
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status type
        /// </summary>
        public StatusMessageType Type { get; set; }

        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the duration in milliseconds (optional)
        /// </summary>
        public int? DurationMs { get; set; }

        /// <summary>
        /// Gets or sets additional context data
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Gets or sets the display duration
        /// </summary>
        public TimeSpan? Duration { get; set; }
    }

    /// <summary>
    /// Status message types
    /// </summary>
    public enum StatusMessageType
    {
        Information,
        Success,
        Warning,
        Error,
        Debug
    }
}