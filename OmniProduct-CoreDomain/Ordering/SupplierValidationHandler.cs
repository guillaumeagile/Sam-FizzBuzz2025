using OmniProduct_CoreDomain.Models;

namespace OmniProduct_CoreDomain.Ordering;

public class SupplierValidationHandler : OrderHandler
{
    private readonly List<Product> _products;
    private readonly List<Supplier> _suppliers;

    public SupplierValidationHandler(List<Product> products, List<Supplier> suppliers, OrderHandler next)
        : base(next)
    {
        _products = products;
        _suppliers = suppliers;
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

        if (!product.SuppliersRegions.ContainsKey(context.Region))
        {
            context.CanContinue = false;
            context.FailureReason = $"No supplier for region {context.Region}";
            return;
        }

        var supplier = product.SuppliersRegions[context.Region];
        context.SupplierEmail = supplier.Email;
        context.SupplierCanFulfill = true;
    }
}
