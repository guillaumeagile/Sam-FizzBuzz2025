using OmniProduct_CoreDomain.Abstractions;

namespace OmniProduct_CoreDomain.Models;

// Pricing concern for a Product: the Price value and margin operations, keyed back to the product by ProductId.
public class ProductPricing : IDentifiable
{
    public string ProductId { get; set; }

    public Price Price { get; set; }

    public ProductPricing(string productId, Price price)
    {
        ProductId = productId;
        Price = price;
    }

    public decimal GetResellerPrice()
    {
        return Price.GetResellerPrice();
    }

    public void SetMargin(decimal marginPercent)
    {
        Price.Margin = marginPercent;
        // use immutability here
    }

    public string Id { get; set; }
}
