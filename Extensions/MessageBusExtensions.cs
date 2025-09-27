using WPFBase.Interfaces;
using WPFBase.Models.Messages;

namespace WPFBase.Extensions;

/// <summary>
/// Extension methods for IMessageBus
/// </summary>
public static class MessageBusExtensions
{
    /// <summary>
    /// Publish a status message
    /// </summary>
    public static void PublishStatus(this IMessageBus messageBus, string text, StatusMessageType type = StatusMessageType.Information, TimeSpan? duration = null)
    {
        if (messageBus == null) throw new ArgumentNullException(nameof(messageBus));
        
        messageBus.Publish(new StatusMessage
        {
            Text = text,
            Type = type,
            Duration = duration
        });
    }
    
    /// <summary>
    /// Publish a progress message
    /// </summary>
    public static void PublishProgress(this IMessageBus messageBus, string operationName, double progress, string? statusText = null, bool isIndeterminate = false)
    {
        if (messageBus == null) throw new ArgumentNullException(nameof(messageBus));
        
        messageBus.Publish(new ProgressMessage
        {
            OperationName = operationName,
            Progress = progress,
            StatusText = statusText,
            IsIndeterminate = isIndeterminate,
            IsCompleted = progress >= 1.0
        });
    }
    
    /// <summary>
    /// Publish a navigation request message
    /// </summary>
    public static void PublishNavigation(this IMessageBus messageBus, Type viewModelType, object? parameter = null)
    {
        if (messageBus == null) throw new ArgumentNullException(nameof(messageBus));
        
        messageBus.Publish(new NavigationMessage
        {
            ViewModelType = viewModelType,
            Parameter = parameter,
            IsBackNavigation = false
        });
    }
}