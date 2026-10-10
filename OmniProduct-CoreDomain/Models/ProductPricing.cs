using OmniProduct_CoreDomain.Abstractions;
using OmniProduct_CoreDomain.ValueObjects;

namespace OmniProduct_CoreDomain.Models;

// Pricing concern for a Product: the Price value and margin operations, keyed back to the product by ProductId.
public record ProductPricing : IDentifiable
{
    public Ulid ProductId { get; init; }

    public Price Price { get; init; }

    public ProductPricing(Ulid productId, Price price)
    {
        ProductId = productId;
        Price = price;
    }

    public decimal GetResellerPrice()
    {
        return Price.GetResellerPrice();
    }

    public ProductPricing SetMargin(decimal marginPercent) => this with { Price = Price with { Margin = marginPercent } };

    public Ulid Id { get; init; }
}
