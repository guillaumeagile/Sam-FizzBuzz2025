using OmniProduct_CoreDomain.Abstractions;
using OmniProduct_CoreDomain.ValueObjects;
using OneOf;

namespace OmniProduct_CoreDomain.Models;

public record Product : IDentifiable
{
    public Ulid Id { get; init; }

    public string Name { get; init; }

    public OneOf<Active, OutOfStock, Deprecated> Status { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }


    public Product()
    {
    }

    public Product(Ulid id, string name, string slug)
    {
        Id = id;
        Name = name;
        Status = new Active();
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
    }

    // --- Sales ---

    // Storage owns the actual stock decrement (see StoredProduct.Withdraw); this only
    // records the sale's effect on the catalog side: status flip.
    public Product Sell(int remainingStock)
    {
        var sold = this with { UpdatedAt = DateTime.Now };

        return remainingStock == 0
            ? sold with { Status = new OutOfStock() }
            : sold;
    }

    // Deliberately badly coded (HA4.6 exercise): the model reads the clock itself,
    // so the sale rule cannot be tested without waiting.
    public bool CanSell(DateTime sellByDate)
    {
        return DateTime.Now <= sellByDate;
    }

    // --- Lifecycle ---

    public Product Deprecate()
    {
        return this with { Status = new Deprecated(), UpdatedAt = DateTime.Now };
    }

    public bool IsDeprecated() => Status.IsT2;

    public Product Touch()
    {
        return this with { UpdatedAt = DateTime.Now };
    }
}
