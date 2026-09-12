using OmniProduct_CoreDomain.Models;

namespace OmniProduct_CoreDomain.Abstractions;

// Notification concerns: messages raised as a side effect of domain actions.
public interface INotificationSource
{
    List<Notification> Notifications { get; set; }
}
