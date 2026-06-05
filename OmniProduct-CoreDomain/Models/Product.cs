namespace OmniProduct_CoreDomain.Models;

public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public Price Price { get; set; }
    public List<string> Discounts { get; set; }
    public Dictionary<string, string> Images { get; set; }          // key = context (e.g. "thumbnail", "hero"), value = url
    public Dictionary<string, Supplier> SuppliersRegions { get; set; } // key = region, value = supplier
    public double Weight { get; set; }
    public string Dimensions { get; set; }
    public int Quantity { get; set; }
    public int Stock { get; set; }
    public Warehouse Warehouse { get; set; }

    // added over time, not in the original constructor
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Product(string id, string name, string slug, Price price, List<string> discounts,
                   Dictionary<string, string> images, Dictionary<string, Supplier> suppliersRegions,
                   double weight, string dimensions, int quantity, int stock, Warehouse warehouse)
    {
        Id = id;
        Name = name;
        Slug = slug;
        Price = price;
        Discounts = discounts;
        Images = images;
        SuppliersRegions = suppliersRegions;
        Weight = weight;
        Dimensions = dimensions;
        Quantity = quantity;
        Stock = stock;
        Warehouse = warehouse;
        Status = "active";
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
    }

    public string GetDisplayLabel()
    {
        if (Status == "deprecated")
            return $"[DISCONTINUED] {Name}";
        if (Stock == 0)
            return $"[OUT OF STOCK] {Name}";
        return Name;
    }
}
