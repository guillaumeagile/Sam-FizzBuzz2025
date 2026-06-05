using OmniProduct_CoreDomain.Models;

namespace OmniProduct_CoreDomain.Ordering;

public class PriceCalculationHandler : OrderHandler
{
    private readonly List<Product> _products;

    public PriceCalculationHandler(List<Product> products, OrderHandler next)
        : base(next)
    {
        _products = products;
    }

    protected override void Process(OrderContext context)
    {
        // Relies on SupplierValidationHandler having run first —
        // silently produces wrong price if context.SupplierCanFulfill was never set
        if (!context.SupplierCanFulfill)
        {
            context.CanContinue = false;
            context.FailureReason = "Cannot calculate price: supplier not validated";
            return;
        }

        var product = _products.FirstOrDefault(p => p.Id == context.ProductId);
        if (product == null)
        {
            context.CanContinue = false;
            context.FailureReason = $"Product {context.ProductId} not found";
            return;
        }

        // Region-specific margin would live here — for now uses the product's single margin
        context.CalculatedPrice = product.Price.GetResellerPrice() * context.RequestedQuantity;
    }
}
