using System;

namespace WPFBase.Models.Messages
{
    /// <summary>
    /// Message for progress updates
    /// </summary>
    public class ProgressMessage
    {
        /// <summary>
        /// Gets or sets the operation name
        /// </summary>
        public string OperationName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the progress value (0.0 to 100.0)
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// Gets or sets the status text
        /// </summary>
        public string? StatusText { get; set; }

        /// <summary>
        /// Gets or sets whether the operation is completed
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Gets or sets whether the operation is indeterminate
        /// </summary>
        public bool IsIndeterminate { get; set; }

        /// <summary>
        /// Gets or sets whether the operation can be cancelled
        /// </summary>
        public bool CanCancel { get; set; }

        /// <summary>
        /// Gets or sets the operation ID for tracking
        /// </summary>
        public string OperationId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the current item being processed (optional)
        /// </summary>
        public int? CurrentItem { get; set; }

        /// <summary>
        /// Gets or sets the total number of items (optional)
        /// </summary>
        public int? TotalItems { get; set; }
    }
}