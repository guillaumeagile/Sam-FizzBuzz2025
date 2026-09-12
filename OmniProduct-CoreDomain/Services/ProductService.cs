using OmniProduct_CoreDomain.Models;
using OmniProduct_CoreDomain.Models.Suppliers;

namespace OmniProduct_CoreDomain.Services;

public class ProductService
{
    private readonly List<ActiveProduct> _products = new();
    private readonly List<Supplier> _suppliers = new();
    private readonly List<Warehouse> _warehouses = new();
    private readonly List<Notification> _notifications = new(); // kept in parallel with Product.Notifications, just in case

    // --- Catalog ---

    public ActiveProduct AddProduct(string name, string region, decimal supplierPrice, string currency)
    {
        var supplier = _suppliers.FirstOrDefault(s => s.Region == region);
        if (supplier == null)
            throw new Exception($"No supplier found for region {region}");

        var warehouse = _warehouses.FirstOrDefault(w => w.Region == region);
        if (warehouse == null)
            throw new Exception($"No warehouse found for region {region}");

        var slug = name.ToLower().Replace(" ", "-");
        var price = new Price(supplierPrice, currency);
        var suppliersRegions = new Dictionary<string, Supplier> { { region, supplier } };

        var product = new ActiveProduct(
            id: Guid.NewGuid().ToString(),
            name: name,
            slug: slug,
            price: price,
            discounts: new List<string>(),
            images: new Dictionary<string, string>(),
            suppliersRegions: suppliersRegions,
            weight: 0,
            dimensions: "",
            quantity: 0,
            stock: 0
        );

        _products.Add(product);
        return product;
    }

    public ActiveProduct GetProduct(string id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            throw new Exception($"Product {id} not found");
        return product;
    }

    public List<ActiveProduct> GetCatalog(string region)
    {
        return _products
            .Where(p => p.SuppliersRegions.ContainsKey(region) && p.Status != "deprecated")
            .ToList();
    }

    public void AddImage(string productId, string context, string url)
    {
        GetProduct(productId).AddImage(context, url);
    }

    public void AddDiscount(string productId, string discountCode)
    {
        GetProduct(productId).AddDiscount(discountCode);
    }

    // --- Suppliers ---

    public Supplier AddSupplier(string name, string email, string region)
    {
        Supplier supplier = region switch
        {
            "UK" => new UkSupplier(),
            "ASIA" => new AsiaSupplier(),
            "AMERICAS" => new AmericasSupplier(),
            _ => new EuropeanSupplier()
        };

        supplier.Id = Guid.NewGuid();
        supplier.Name = name;
        supplier.Email = email;
        supplier.Region = region;

        _suppliers.Add(supplier);
        return supplier;
    }

    public void AddSupplierToRegion(string productId, string region)
    {
        GetProduct(productId).AddSupplierToRegion(region, _suppliers);
    }

    // --- Pricing ---

    public decimal GetResellerPrice(string productId)
    {
        return GetProduct(productId).GetResellerPrice();
    }

    public void SetMargin(string productId, decimal marginPercent)
    {
        GetProduct(productId).SetMargin(marginPercent);
    }

    // --- Stock ---

    public Warehouse AddWarehouse(string name, string address, string region)
    {
        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Name = name,
            Address = address,
            Region = region
        };
        _warehouses.Add(warehouse);
        return warehouse;
    }

    public void ReceiveStock(string productId, int quantity)
    {
        GetProduct(productId).ReceiveStock(quantity);
    }

    public void SellProduct(string productId, int quantity)
    {
        var product = GetProduct(productId);
        product.Sell(quantity);

        // NOTE: Product.Sell() already raises notifications internally, but that code path
        // was flaky for a while so this was added here too as a safety net. Never removed.
        foreach (var (region, supplier) in product.SuppliersRegions)
        {
            _notifications.Add(new Notification
            {
                Recipient = supplier.Email,
                Subject = $"Sale confirmed: {product.Name}",
                Body = $"Sold {quantity} of {product.Name}. Stock left: {product.Stock}.",
                Channel = "email",
                SentAt = DateTime.Now
            });
        }
    }

    // --- Lifecycle ---

    public void DeprecateProduct(string productId)
    {
        var product = GetProduct(productId);
        product.Deprecate();

        // same story as SellProduct: duplicated on purpose (?) because customers said
        // they weren't getting the deprecation email. Nobody checked why.
        _notifications.Add(new Notification
        {
            Recipient = "customers@omniproduct.com",
            Subject = $"[Discontinued] {product.Name}",
            Body = $"We're sorry, {product.Name} has been discontinued.",
            Channel = "email",
            SentAt = DateTime.Now
        });
    }

    // --- Notifications (pulled from every product, plus the ones the service kept for itself) ---

    public List<Notification> GetNotifications()
    {
        return _products.SelectMany(p => p.Notifications)
            .Concat(_notifications)
            .ToList();
    }
}
