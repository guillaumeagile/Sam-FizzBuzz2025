using OmniProduct_CoreDomain.Events;
using OmniProduct_CoreDomain.Models;
using OmniProduct_CoreDomain.ValueObjects;

namespace OmniProduct_CoreDomain.Services;

// Owns the Product entity's lifecycle: creation, sale, deprecation. Orchestrates the per-concern
// services (Pricing/Supplier/Storage/Notification) for the parts of those operations that belong
// to their concern, rather than owning that vocabulary itself.
public class ProductLifecycleService
{
    private readonly PricingService _pricingService;
    private readonly SupplierService _supplierService;
    private readonly StorageService _storageService;
    private readonly NotificationService _notificationService;

    private readonly List<Product> _products = new();

    public ProductLifecycleService(
        PricingService pricingService,
        SupplierService supplierService,
        StorageService storageService,
        NotificationService notificationService)
    {
        _pricingService = pricingService;
        _supplierService = supplierService;
        _storageService = storageService;
        _notificationService = notificationService;
    }

    public Product AddProduct(string name, string slug, string region, decimal supplierPrice, string currency)
    {
        var supplier = _supplierService.FindSupplierForRegion(region);
        var warehouse = _storageService.FindWarehouseNear(region);

        var priceResult = Price.Create(supplierPrice, currency);
        if (priceResult.IsT1)
            throw new ArgumentException(priceResult.AsT1.Message);
        var price = priceResult.AsT0;
        var suppliersRegions = new Dictionary<string, Ulid> { { region, supplier.Id } };

        var product = new Product(
            id: Ulid.NewUlid(),
            name: name,
            slug: slug
        );

        _products.Add(product);
        _pricingService.AddPricing(product.Id, price);
        _supplierService.AddSupplierAssignment(product.Id, suppliersRegions);
        _storageService.AddStock(product.Id, warehouse.Id);

        return product;
    }

    public Product GetProduct(Ulid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            throw new Exception($"Product {id} not found");
        return product;
    }

    public List<Product> GetActiveProducts()
    {
        return _products.Where(p => !p.IsDeprecated()).ToList();
    }

    public void SellProduct(Ulid productId, int quantity)
    {
        var product = GetProduct(productId);
        var suppliersRegions = _supplierService.GetSuppliers(productId).SuppliersRegions;

        var storedProduct = _storageService.Withdraw(productId, quantity);
        product = SaveProduct(product.Sell(storedProduct.Stock));

        foreach (var (region, supplierId) in suppliersRegions)
        {
            _notificationService.AddNotification(new Notification
            {
                Recipient = _supplierService.GetSupplier(supplierId).Email,
                Subject = $"Sale confirmed: {product.Name}",
                Body = $"Sold {quantity} of {product.Name}. Stock left: {storedProduct.Stock}.",
                SentAt = DateTime.Now
            });
        }
    }

    public void DeprecateProduct(Ulid productId)
    {
        var product = GetProduct(productId);
        var suppliersRegions = _supplierService.GetSuppliers(productId).SuppliersRegions;
        product = SaveProduct(product.Deprecate());

        foreach (var (region, supplierId) in suppliersRegions)
        {
            _notificationService.AddNotification(new Notification
            {
                Recipient = _supplierService.GetSupplier(supplierId).Email,
                Subject = $"Product deprecated: {product.Name}",
                Body = $"The product {product.Name} has been deprecated and removed from the catalog.",
                SentAt = DateTime.Now
            });
        }

        _notificationService.AddNotification(new Notification
        {
            Recipient = "customers@omniproduct.com",
            Subject = $"[Discontinued] {product.Name}",
            Body = $"We're sorry, {product.Name} has been discontinued.",
            SentAt = DateTime.Now
        });
    }

    public void TouchProduct(Ulid productId)
    {
        SaveProduct(GetProduct(productId).Touch());
    }

    // Records are immutable: a mutation yields a new instance that has to replace the stored one.
    private Product SaveProduct(Product product)
    {
        _products[_products.FindIndex(p => p.Id == product.Id)] = product;
        return product;
    }
}
