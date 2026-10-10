using OmniProduct_CoreDomain.Abstractions;
using OmniProduct_CoreDomain.Models.Storage;

namespace OmniProduct_CoreDomain.Models;

public record Warehouse : IDentifiable
{
    public Ulid Id { get; init; }

    public string Name { get; init; }

    public string Address { get; init; }

    public string Region { get; init; }

    public IReadOnlyList<Ulid> StoredProductIds { get; init; } = Array.Empty<Ulid>();
}
