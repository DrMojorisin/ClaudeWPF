using System;

namespace WPFBase.Models.Messages
{
    /// <summary>
    /// Message for navigation events
    /// </summary>
    public class NavigationMessage
    {
        /// <summary>
        /// Gets or sets the ViewModel type to navigate to
        /// </summary>
        public Type? ViewModelType { get; set; }

        /// <summary>
        /// Gets or sets the ViewModel instance to navigate to
        /// </summary>
        public object? ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the navigation parameter
        /// </summary>
        public object? Parameter { get; set; }

        /// <summary>
        /// Gets or sets whether this is a back navigation
        /// </summary>
        public bool IsBackNavigation { get; set; }

        /// <summary>
        /// Gets or sets whether to clear the navigation history
        /// </summary>
        public bool ClearHistory { get; set; }
    }
}