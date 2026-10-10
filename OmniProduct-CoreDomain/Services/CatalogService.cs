using OmniProduct_CoreDomain.Models;

namespace OmniProduct_CoreDomain.Services;

// Pure catalog concern: images, discounts, and the catalog listing. Product creation/sale/
// deprecation is orchestrated by ProductLifecycleService.
public class CatalogService
{
    private readonly ProductLifecycleService _productLifecycleService;
    private readonly SupplierService _supplierService;

    private readonly List<ProductCatalog> _catalogs = new();

    public CatalogService(ProductLifecycleService productLifecycleService, SupplierService supplierService)
    {
        _productLifecycleService = productLifecycleService;
        _supplierService = supplierService;
    }

    public Product CreateListing(string name, string region, decimal supplierPrice, string currency)
    {
        var slug = name.ToLower().Replace(" ", "-");

        var product = _productLifecycleService.AddProduct(name, slug, region, supplierPrice, currency);

        _catalogs.Add(new ProductCatalog(product.Id, slug, new Dictionary<string, string>(), new List<string>()));

        return product;
    }

    public ProductCatalog GetListing(Ulid productId)
    {
        var catalog = _catalogs.FirstOrDefault(c => c.ProductId == productId);
        if (catalog == null)
            throw new Exception($"No catalog entry found for product {productId}");
        return catalog;
    }

    public List<Product> GetCatalog(string region)
    {
        return _productLifecycleService.GetActiveProducts()
            .Where(p => _supplierService.GetSuppliers(p.Id).SuppliersRegions.ContainsKey(region))
            .ToList();
    }

    public void AddImage(Ulid productId, string context, string url)
    {
        SaveListing(GetListing(productId).AddImage(context, url));
        _productLifecycleService.TouchProduct(productId);
    }

    public void AddDiscount(Ulid productId, string discountCode)
    {
        SaveListing(GetListing(productId).AddDiscount(discountCode));
        _productLifecycleService.TouchProduct(productId);
    }

    // Records are immutable: a mutation yields a new instance that has to replace the stored one.
    private void SaveListing(ProductCatalog listing)
    {
        _catalogs[_catalogs.FindIndex(c => c.ProductId == listing.ProductId)] = listing;
    }
}
