using OmniProduct_CoreDomain.Abstractions;

namespace OmniProduct_CoreDomain.Models;

public record Supplier : IDentifiable
{
    public Ulid Id { get; init; }

    public string Name { get; init; }

    public string Email { get; init; }

    public string Region { get; init; }

    public IReadOnlyList<Ulid> ProductIds { get; init; } = Array.Empty<Ulid>();
}
