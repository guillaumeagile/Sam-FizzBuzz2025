using OmniProduct_CoreDomain.Abstractions;

namespace OmniProduct_CoreDomain.Models;

// Supplier concern for a Product: which supplier serves which region, keyed back to the product by ProductId.
public class ProductSuppliers : IDentifiable
{
    public string ProductId { get; set; }

    public Dictionary<string, Supplier> SuppliersRegions { get; set; } // key = region, value = supplier

    public ProductSuppliers(string productId, Dictionary<string, Supplier> suppliersRegions)
    {
        ProductId = productId;
        SuppliersRegions = suppliersRegions;
    }

    public void AddSupplierToRegion(string region, List<Supplier> suppliers)
    {
        var supplier = suppliers.FirstOrDefault(s => s.Region == region);
        if (supplier == null)
            throw new Exception($"No supplier found for region {region}");

        SuppliersRegions[region] = supplier;
    }

    public string Id { get; set; }
}
