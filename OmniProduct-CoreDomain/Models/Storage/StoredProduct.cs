namespace OmniProduct_CoreDomain.Models.Storage;

// Storage BC: the smallest slice of Product needed to stock merchandise in a Warehouse.
// No price, discounts, images, suppliers, or notifications - that's Catalog/Pricing/Sales concerns.
public class StoredProduct
{
    public string ProductId { get; set; }

    public double Weight { get; set; }
    public string Dimensions { get; set; }

    public int Stock { get; set; }

    public Guid WarehouseId { get; set; }

    public StoredProduct(string productId, double weight, string dimensions, int stock, Guid warehouseId)
    {
        ProductId = productId;
        Weight = weight;
        Dimensions = dimensions;
        Stock = stock;
        WarehouseId = warehouseId;
    }

    public void Receive(int quantity)
    {
        Stock += quantity;
    }

    public void Withdraw(int quantity)
    {
        if (Stock < quantity)
            throw new Exception("Not enough stock");

        Stock -= quantity;
    }
}
