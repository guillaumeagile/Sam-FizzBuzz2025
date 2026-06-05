using OmniProduct_CoreDomain.Models;

namespace OmniProduct_CoreDomain.Services;

public class ProductService
{
    private readonly List<Product> _products = new();
    private readonly List<Supplier> _suppliers = new();
    private readonly List<Warehouse> _warehouses = new();
    private readonly List<Notification> _notifications = new();

    // --- Catalog ---

    public Product AddProduct(string name, string region, decimal supplierPrice, string currency)
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

        var product = new Product(
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
            stock: 0,
            warehouse: warehouse
        );

        _products.Add(product);
        return product;
    }

    public Product GetProduct(string id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            throw new Exception($"Product {id} not found");
        return product;
    }

    public List<Product> GetCatalog(string region)
    {
        return _products
            .Where(p => p.SuppliersRegions.ContainsKey(region) && p.Status != "deprecated")
            .ToList();
    }

    public void AddImage(string productId, string context, string url)
    {
        var product = GetProduct(productId);
        product.Images[context] = url;
        product.UpdatedAt = DateTime.Now;
    }

    public void AddDiscount(string productId, string discountCode)
    {
        var product = GetProduct(productId);
        product.Discounts.Add(discountCode);
        product.UpdatedAt = DateTime.Now;
    }

    // --- Suppliers ---

    public Supplier AddSupplier(string name, string email, string region)
    {
        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Region = region
        };
        _suppliers.Add(supplier);
        return supplier;
    }

    public void AddSupplierToRegion(string productId, string region)
    {
        var product = GetProduct(productId);
        var supplier = _suppliers.FirstOrDefault(s => s.Region == region);
        if (supplier == null)
            throw new Exception($"No supplier found for region {region}");

        product.SuppliersRegions[region] = supplier;
        product.UpdatedAt = DateTime.Now;
    }

    // --- Pricing ---

    public decimal GetResellerPrice(string productId)
    {
        var product = GetProduct(productId);
        return product.Price.GetResellerPrice();
    }

    public void SetMargin(string productId, decimal marginPercent)
    {
        var product = GetProduct(productId);
        product.Price.Margin = marginPercent;
        product.UpdatedAt = DateTime.Now;
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
        var product = GetProduct(productId);
        product.Stock += quantity;
        product.Quantity += quantity;
        product.UpdatedAt = DateTime.Now;
    }

    public void SellProduct(string productId, int quantity)
    {
        var product = GetProduct(productId);

        if (product.Stock < quantity)
            throw new Exception("Not enough stock");

        product.Stock -= quantity;
        product.UpdatedAt = DateTime.Now;

        if (product.Stock == 0)
            product.Status = "out_of_stock";

        // Notify all regional suppliers
        foreach (var (region, supplier) in product.SuppliersRegions)
        {
            _notifications.Add(new Notification
            {
                Recipient = supplier.Email,
                Subject = $"Product sold: {product.Name}",
                Body = $"{quantity} unit(s) of {product.Name} were sold. Remaining stock: {product.Stock}.",
                Channel = "email",
                SentAt = DateTime.Now
            });
        }
    }

    // --- Lifecycle ---

    public void DeprecateProduct(string productId)
    {
        var product = GetProduct(productId);

        product.Status = "deprecated";
        product.Stock = 0;
        product.UpdatedAt = DateTime.Now;

        // Notify all regional suppliers
        foreach (var (region, supplier) in product.SuppliersRegions)
        {
            _notifications.Add(new Notification
            {
                Recipient = supplier.Email,
                Subject = $"Product deprecated: {product.Name}",
                Body = $"The product {product.Name} has been deprecated and removed from the catalog.",
                Channel = "email",
                SentAt = DateTime.Now
            });
        }

        // Notify customers
        _notifications.Add(new Notification
        {
            Recipient = "customers@omniproduct.com",
            Subject = $"Product no longer available: {product.Name}",
            Body = $"{product.Name} is no longer available.",
            Channel = "email",
            SentAt = DateTime.Now
        });
    }
}
