using System.Collections.Immutable;
using OmniProduct_CoreDomain.Abstractions;

namespace OmniProduct_CoreDomain.Models;

// Catalog concern for a Product: images and discounts, keyed back to the product by ProductId.
public record ProductCatalog : IDentifiable
{
    public Ulid ProductId { get; init; }

    public string Slug { get; init; }

    public IReadOnlyDictionary<string, string> Images { get; init; }          // key = context (e.g. "thumbnail", "hero"), value = url

    public IReadOnlyList<string> Discounts { get; init; }

    public ProductCatalog(Ulid productId, string slug, IReadOnlyDictionary<string, string> images, IReadOnlyList<string> discounts)
    {
        ProductId = productId;
        Slug = slug;
        Images = images;
        Discounts = discounts;
    }

    public string GetDisplayLabel(string productName, string productStatus, int stock)
    {
        if (productStatus == "deprecated")
            return $"[DISCONTINUED] {productName}";
        if (stock == 0)
            return $"[OUT OF STOCK] {productName}";
        return productName;
    }

    public ProductCatalog AddImage(string context, string url)
    {
        return this with { Images = Images.ToImmutableDictionary().SetItem(context, url) };
    }

    public ProductCatalog AddDiscount(string discountCode)
    {
        return this with { Discounts = Discounts.ToImmutableList().Add(discountCode) };
    }

    public Ulid Id { get; init; }
}
