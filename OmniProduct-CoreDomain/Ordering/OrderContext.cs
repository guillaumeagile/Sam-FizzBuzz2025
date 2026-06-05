namespace OmniProduct_CoreDomain.Ordering;

public class OrderContext
{
    // Input
    public string ProductId { get; set; }
    public string Region { get; set; }
    public int RequestedQuantity { get; set; }
    public string CustomerId { get; set; }

    // Populated by SupplierValidationHandler
    public string SupplierEmail { get; set; }
    public bool SupplierCanFulfill { get; set; }

    // Populated by PriceCalculationHandler
    public decimal CalculatedPrice { get; set; }

    // Populated by StockCheckHandler
    public bool StockAvailable { get; set; }
    public int ReservedQuantity { get; set; }

    // Populated by WarehouseDispatchHandler
    public string DispatchReference { get; set; }
    public string WarehouseName { get; set; }

    // Chain control — each handler sets this to false to abort
    public bool CanContinue { get; set; } = true;
    public string FailureReason { get; set; }
}
