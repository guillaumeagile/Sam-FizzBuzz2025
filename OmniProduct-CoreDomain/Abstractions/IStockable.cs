namespace OmniProduct_CoreDomain.Abstractions;

// Inventory concerns: quantities on hand and stock movements.
public interface IStockable
{
    int Quantity { get; set; }
    int Stock { get; set; }
    void ReceiveStock(int quantity);
    void Sell(int quantity);
}
