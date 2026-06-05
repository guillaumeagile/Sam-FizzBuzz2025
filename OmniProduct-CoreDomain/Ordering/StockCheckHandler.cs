using OmniProduct_CoreDomain.Models;

namespace OmniProduct_CoreDomain.Ordering;

public class StockCheckHandler : OrderHandler
{
    private readonly List<Product> _products;

    public StockCheckHandler(List<Product> products, OrderHandler next)
        : base(next)
    {
        _products = products;
    }

    protected override void Process(OrderContext context)
    {
        var product = _products.FirstOrDefault(p => p.Id == context.ProductId);
        if (product == null)
        {
            context.CanContinue = false;
            context.FailureReason = $"Product {context.ProductId} not found";
            return;
        }

        if (product.Stock < context.RequestedQuantity)
        {
            context.CanContinue = false;
            context.StockAvailable = false;
            context.FailureReason = $"Insufficient stock: requested {context.RequestedQuantity}, available {product.Stock}";
            return;
        }

        // Mutates product directly — side effect buried in the middle of the chain
        product.Stock -= context.RequestedQuantity;

        context.StockAvailable = true;

        context.ReservedQuantity = context.RequestedQuantity;
    }
}
