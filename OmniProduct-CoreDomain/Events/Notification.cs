using OmniProduct_CoreDomain.Abstractions;

namespace OmniProduct_CoreDomain.Events;

public class Notification : IDentifiable
{
    public string Id { get; set; }

    public string Recipient { get; set; }

    public string Subject { get; set; }

    public string Body { get; set; }

    public DateTime SentAt { get; set; }

    public string ProductId { get; set; }
}
