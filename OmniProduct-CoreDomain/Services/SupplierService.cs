using OmniProduct_CoreDomain.Models;

namespace OmniProduct_CoreDomain.Services;

public class SupplierService
{
    private readonly List<Supplier> _suppliers = new();
    private readonly List<ProductSuppliers> _productSuppliers = new();

    public Supplier AddSupplier(string name, string email, string region)
    {
        var supplier = new Supplier
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Email = email,
            Region = region
        };
        _suppliers.Add(supplier);
        return supplier;
    }

    public ProductSuppliers AddSupplierAssignment(string productId, Dictionary<string, Supplier> suppliersRegions)
    {
        var productSuppliers = new ProductSuppliers(productId, suppliersRegions);
        _productSuppliers.Add(productSuppliers);
        return productSuppliers;
    }

    public ProductSuppliers GetSuppliers(string productId)
    {
        var productSuppliers = _productSuppliers.FirstOrDefault(s => s.ProductId == productId);
        if (productSuppliers == null)
            throw new Exception($"No supplier assignment found for product {productId}");
        return productSuppliers;
    }

    public Supplier FindSupplierForRegion(string region)
    {
        var supplier = _suppliers.FirstOrDefault(s => s.Region == region);
        if (supplier == null)
            throw new Exception($"No supplier found for region {region}");
        return supplier;
    }

    public void AddSupplierToRegion(string productId, string region)
    {
        GetSuppliers(productId).AddSupplierToRegion(region, _suppliers);
    }
}
