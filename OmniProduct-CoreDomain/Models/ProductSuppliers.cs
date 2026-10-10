using System.Collections.Immutable;
using OmniProduct_CoreDomain.Abstractions;

namespace OmniProduct_CoreDomain.Models;

// Supplier concern for a Product: which supplier serves which region, keyed back to the product by ProductId.
public record ProductSuppliers : IDentifiable
{
    public Ulid ProductId { get; init; }

    public IReadOnlyDictionary<string, Ulid> SuppliersRegions { get; init; } // key = region, value = supplier id

    public ProductSuppliers(Ulid productId, IReadOnlyDictionary<string, Ulid> suppliersRegions)
    {
        ProductId = productId;
        SuppliersRegions = suppliersRegions;
    }

    public ProductSuppliers AddSupplierToRegion(string region, Ulid supplierId)
    {
        return this with { SuppliersRegions = SuppliersRegions.ToImmutableDictionary().SetItem(region, supplierId) };
    }

    public Ulid Id { get; init; }
}
