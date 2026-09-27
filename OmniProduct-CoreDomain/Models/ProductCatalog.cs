namespace OmniProduct_CoreDomain.Models;

// Catalog concern for a Product: images and discounts, keyed back to the product by ProductId.
public class ProductCatalog
{
    public string ProductId { get; set; }

    public Dictionary<string, string> Images { get; set; }          // key = context (e.g. "thumbnail", "hero"), value = url

    public List<string> Discounts { get; set; }

    public ProductCatalog(string productId, Dictionary<string, string> images, List<string> discounts)
    {
        ProductId = productId;
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

    public void AddImage(string context, string url)
    {
        Images[context] = url;
    }

    public void AddDiscount(string discountCode)
    {
        Discounts.Add(discountCode);
    }
}
