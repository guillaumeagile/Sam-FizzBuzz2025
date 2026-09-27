using OmniProduct_CoreDomain.Models;
using OmniProduct_CoreDomain.Models.Storage;

namespace OmniProduct_CoreDomain.Services;

public class ProductService
{
    private readonly List<Product> _products = new();
    private readonly List<ProductCatalog> _catalogs = new();
    private readonly List<ProductPricing> _pricings = new();
    private readonly List<ProductSuppliers> _productSuppliers = new();
    private readonly List<Supplier> _suppliers = new();
    private readonly List<Warehouse> _warehouses = new();
    private readonly List<StoredProduct> _storedProducts = new();
    private readonly List<Notification> _notifications = new(); // kept in parallel with Product.Notifications, just in case

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
            slug: slug
        );

        _products.Add(product);
        _catalogs.Add(new ProductCatalog(product.Id, new Dictionary<string, string>(), new List<string>()));
        _pricings.Add(new ProductPricing(product.Id, price));
        _productSuppliers.Add(new ProductSuppliers(product.Id, suppliersRegions));

        _storedProducts.Add(new StoredProduct(
            productId: product.Id,
            weight: 0,
            dimensions: "",
            stock: 0,
            warehouseId: warehouse.Id
        ));

        return product;
    }

    public StoredProduct GetStoredProduct(string productId)
    {
        var storedProduct = _storedProducts.FirstOrDefault(sp => sp.ProductId == productId);
        if (storedProduct == null)
            throw new Exception($"No stock record found for product {productId}");
        return storedProduct;
    }

    public Product GetProduct(string id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            throw new Exception($"Product {id} not found");
        return product;
    }

    public ProductCatalog GetProductCatalog(string productId)
    {
        var catalog = _catalogs.FirstOrDefault(c => c.ProductId == productId);
        if (catalog == null)
            throw new Exception($"No catalog entry found for product {productId}");
        return catalog;
    }

    public ProductPricing GetProductPricing(string productId)
    {
        var pricing = _pricings.FirstOrDefault(p => p.ProductId == productId);
        if (pricing == null)
            throw new Exception($"No pricing found for product {productId}");
        return pricing;
    }

    public ProductSuppliers GetProductSuppliers(string productId)
    {
        var productSuppliers = _productSuppliers.FirstOrDefault(s => s.ProductId == productId);
        if (productSuppliers == null)
            throw new Exception($"No supplier assignment found for product {productId}");
        return productSuppliers;
    }

    public List<Product> GetCatalog(string region)
    {
        return _products
            .Where(p => GetProductSuppliers(p.Id).SuppliersRegions.ContainsKey(region) && p.Status != "deprecated")
            .ToList();
    }

    public void AddImage(string productId, string context, string url)
    {
        GetProductCatalog(productId).AddImage(context, url);
        GetProduct(productId).UpdatedAt = DateTime.Now;
    }

    public void AddDiscount(string productId, string discountCode)
    {
        GetProductCatalog(productId).AddDiscount(discountCode);
        GetProduct(productId).UpdatedAt = DateTime.Now;
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
        GetProductSuppliers(productId).AddSupplierToRegion(region, _suppliers);
        GetProduct(productId).UpdatedAt = DateTime.Now;
    }

    // --- Pricing ---

    public decimal GetResellerPrice(string productId)
    {
        return GetProductPricing(productId).GetResellerPrice();
    }

    public void SetMargin(string productId, decimal marginPercent)
    {
        GetProductPricing(productId).SetMargin(marginPercent);
        GetProduct(productId).UpdatedAt = DateTime.Now;
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
        GetStoredProduct(productId).Receive(quantity);
    }

    public void SellProduct(string productId, int quantity)
    {
        var product = GetProduct(productId);
        var storedProduct = GetStoredProduct(productId);
        var suppliersRegions = GetProductSuppliers(productId).SuppliersRegions;

        storedProduct.Withdraw(quantity);
        product.Sell(quantity, storedProduct.Stock, suppliersRegions);

        // NOTE: Product.Sell() already raises notifications internally, but that code path
        // was flaky for a while so this was added here too as a safety net. Never removed.
        foreach (var (region, supplier) in suppliersRegions)
        {
            _notifications.Add(new Notification
            {
                Recipient = supplier.Email,
                Subject = $"Sale confirmed: {product.Name}",
                Body = $"Sold {quantity} of {product.Name}. Stock left: {storedProduct.Stock}.",
                SentAt = DateTime.Now
            });
        }
    }

    // --- Lifecycle ---

    public void DeprecateProduct(string productId)
    {
        var product = GetProduct(productId);
        var suppliersRegions = GetProductSuppliers(productId).SuppliersRegions;
        product.Deprecate(suppliersRegions);

        // same story as SellProduct: duplicated on purpose (?) because customers said
        // they weren't getting the deprecation email. Nobody checked why.
        _notifications.Add(new Notification
        {
            Recipient = "customers@omniproduct.com",
            Subject = $"[Discontinued] {product.Name}",
            Body = $"We're sorry, {product.Name} has been discontinued.",
            SentAt = DateTime.Now
        });

        //  AN EVENT  SHOULD BE EMITTED HERE, instead of unsing notifications
        // how to find a heursitic on that ???????????????????????????????????

    }

    // --- Notifications (pulled from every product, plus the ones the service kept for itself) ---
    public List<Notification> GetNotifications()
    {
        return _products.SelectMany(p => p.Notifications)
            .Concat(_notifications)
            .ToList();
    }

    // HA9 (todo) note for a new heuristic; ask to remove unused Method, then detect that _notification is never read (so , useless)
}
