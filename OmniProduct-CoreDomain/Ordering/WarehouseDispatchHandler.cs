using OmniProduct_CoreDomain.Models;

namespace OmniProduct_CoreDomain.Ordering;

public class WarehouseDispatchHandler : OrderHandler
{
    private readonly List<Product> _products;

    // Terminal handler — no next
    public WarehouseDispatchHandler(List<Product> products)
        : base(null!)
    {
        _products = products;
    }

    protected override void Process(OrderContext context)
    {
        // Reads context.ReservedQuantity set by StockCheckHandler —
        // if StockCheckHandler was skipped or reordered, this is 0 and dispatch is silent
        if (context.ReservedQuantity == 0)
        {
            context.CanContinue = false;
            context.FailureReason = "Cannot dispatch: no quantity reserved";
            return;
        }

        var product = _products.FirstOrDefault(p => p.Id == context.ProductId);
        if (product == null)
        {
            context.CanContinue = false;
            context.FailureReason = $"Product {context.ProductId} not found";
            return;
        }

        context.WarehouseName = product.Warehouse.Name;
        context.DispatchReference = $"DISPATCH-{context.ProductId}-{DateTime.Now:yyyyMMddHHmmss}";
    }
}
