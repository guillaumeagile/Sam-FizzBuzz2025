using OmniProduct_CoreDomain.Abstractions;

namespace OmniProduct_CoreDomain.Events;

public record Notification : IDentifiable
{
    public Ulid Id { get; init; }

    public string Recipient { get; init; }

    public string Subject { get; init; }

    public string Body { get; init; }

    public DateTime SentAt { get; init; }

    public Ulid ProductId { get; init; }
}
