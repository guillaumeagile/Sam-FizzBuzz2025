using OmniProduct_CoreDomain.Abstractions;

namespace OmniProduct_CoreDomain.Models.Storage;

// Storage BC: the smallest slice of Product needed to stock merchandise in a Warehouse.
// No price, discounts, images, suppliers, or notifications - that's Catalog/Pricing/Sales concerns.
public record StoredProduct : IDentifiable
{
    public Ulid Id { get; init; }
    public Ulid ProductId { get; init; }

    public double Weight { get; init; }
    public string Dimensions { get; init; }

    public int Stock { get; init; }

    public Ulid WarehouseId { get; init; }

    public StoredProduct(Ulid productId, double weight, string dimensions, int stock, Ulid warehouseId)
    {
        ProductId = productId;
        Weight = weight;
        Dimensions = dimensions;
        Stock = stock;
        WarehouseId = warehouseId;
    }

    public StoredProduct Receive(int quantity)
    {
        return this with { Stock = Stock + quantity };
    }

    public StoredProduct Withdraw(int quantity)
    {
        if (Stock < quantity)
            throw new Exception("Not enough stock");

        return this with { Stock = Stock - quantity };
    }
}
