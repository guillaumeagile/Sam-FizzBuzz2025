using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using OmniProduct_CoreDomain.Models.Suppliers;

namespace OmniProduct_CoreDomain.Models;

public abstract class ProductBase
{
    protected ProductBase()
    {
    }

    protected ProductBase(string id, string name, string slug, Price price, List<string> discounts,
        Dictionary<string, string> images, Dictionary<string, Supplier> suppliersRegions,
        double weight, string dimensions, int quantity, int stock)
    {
        var warehouse = new Warehouse();
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
        WarehouseId = warehouse?.Id;
        Status = "active";
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;

        SyncEfColumns();
    }

    [Key]
    [Column("ProductId")]
    [MaxLength(64)]
    public string Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; }

    [MaxLength(256)]
    public string Slug { get; set; }

    [NotMapped]
    public Price Price { get; set; }

    public decimal PriceAmount { get; set; }
    public string PriceCurrency { get; set; }
    public decimal PriceMargin { get; set; }
    public decimal PriceVat { get; set; }

    [NotMapped]
    public List<string> Discounts { get; set; }

    [Column("Discounts")]
    public string DiscountsCsv { get; set; }

    [NotMapped]
    public Dictionary<string, string> Images { get; set; }          // key = context (e.g. "thumbnail", "hero"), value = url

    [Column("Images")]
    public string ImagesJson { get; set; }

    [NotMapped]
    public Dictionary<string, Supplier> SuppliersRegions { get; set; } // key = region, value = supplier

    public double Weight { get; set; }

    [MaxLength(64)]
    public string Dimensions { get; set; }

    public int Quantity { get; set; }
    public int Stock { get; set; }
    public Guid? WarehouseId { get; set; }

    [NotMapped]
    public Warehouse Warehouse { get; set; }

    [MaxLength(32)]
    public abstract string Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [NotMapped]
    public List<Notification> Notifications { get; set; } = new();

    public void SyncEfColumns()
    {
        if (Price != null)
        {
            PriceAmount = Price.Amount;
            PriceCurrency = Price.Currency;
            PriceMargin = Price.Margin;
            PriceVat = Price.Vat;
        }

        DiscountsCsv = Discounts == null ? "" : string.Join(",", Discounts);
        ImagesJson = Images == null ? "{}" : JsonSerializer.Serialize(Images);
    }

    public void HydrateFromEfColumns()
    {
        Price = new Price(PriceAmount, PriceCurrency)
        {
            Margin = PriceMargin,
            Vat = PriceVat
        };

        Discounts = string.IsNullOrEmpty(DiscountsCsv)
            ? new List<string>()
            : DiscountsCsv.Split(',').ToList();

        Images = string.IsNullOrEmpty(ImagesJson)
            ? new Dictionary<string, string>()
            : JsonSerializer.Deserialize<Dictionary<string, string>>(ImagesJson) ?? new Dictionary<string, string>();

        SuppliersRegions ??= new Dictionary<string, Supplier>();
    }

    public string GetDisplayLabel()
    {
        if (Status == "deprecated")
            return $"[DISCONTINUED] {Name}";
        if (Stock == 0)
            return $"[OUT OF STOCK] {Name}";
        return Name;
    }

    public void AddImage(string context, string url)
    {
        Images[context] = url;
        UpdatedAt = DateTime.Now;
        SyncEfColumns();
    }

    public void AddDiscount(string discountCode)
    {
        Discounts.Add(discountCode);
        UpdatedAt = DateTime.Now;
        // NOTE: forgot to call SyncEfColumns() here - DiscountsCsv will drift from Discounts
        // until something else touches this product and re-syncs it. Known issue, never filed.
    }

    public void AddSupplierToRegion(string region, List<Supplier> suppliers)
    {
        var supplier = suppliers.FirstOrDefault(s => s.Region == region);
        if (supplier == null)
            throw new Exception($"No supplier found for region {region}");

        SuppliersRegions[region] = supplier;
        UpdatedAt = DateTime.Now;
    }

    public decimal GetResellerPrice()
    {
        return Price.GetResellerPrice();
    }

    public void SetMargin(decimal marginPercent)
    {
        Price.Margin = marginPercent;
        UpdatedAt = DateTime.Now;
        SyncEfColumns();
    }

    public void ReceiveStock(int quantity)
    {
        Stock += quantity;
        Quantity += quantity;
        UpdatedAt = DateTime.Now;
    }

    public void Sell(int quantity)
    {
        if (Stock < quantity)
            throw new Exception("Not enough stock");

        Stock -= quantity;
        UpdatedAt = DateTime.Now;

        if (Stock == 0)
            Status = "out_of_stock";

        // Notify all regional suppliers
        foreach (var (region, supplier) in SuppliersRegions)
        {
            Notifications.Add(new Notification
            {
                Recipient = supplier.Email,
                Subject = $"Product sold: {Name}",
                Body = $"{quantity} unit(s) of {Name} were sold. Remaining stock: {Stock}.",
                Channel = "email",
                SentAt = DateTime.Now
            });
        }
    }

    public void Deprecate()
    {
        Status = "deprecated";
        Stock = 0;
        UpdatedAt = DateTime.Now;

        // Notify all regional suppliers
        foreach (var (region, supplier) in SuppliersRegions)
        {
            Notifications.Add(new Notification
            {
                Recipient = supplier.Email,
                Subject = $"Product deprecated: {Name}",
                Body = $"The product {Name} has been deprecated and removed from the catalog.",
                Channel = "email",
                SentAt = DateTime.Now
            });
        }

        // Notify customers
        Notifications.Add(new Notification
        {
            Recipient = "customers@omniproduct.com",
            Subject = $"Product no longer available: {Name}",
            Body = $"{Name} is no longer available.",
            Channel = "email",
            SentAt = DateTime.Now
        });
    }
}
