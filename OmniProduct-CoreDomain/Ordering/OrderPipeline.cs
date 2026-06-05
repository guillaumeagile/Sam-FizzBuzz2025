using OmniProduct_CoreDomain.Models;

namespace OmniProduct_CoreDomain.Ordering;

public class OrderPipeline
{
    private readonly OrderHandler _chain;

    public OrderPipeline(List<Product> products, List<Supplier> suppliers)
    {
        // The order is hardwired here — changing it means rewriting this constructor.
        // Want to skip VAT for some regions? Add a loyalty discount? Add an export check?
        // You either add another subclass into this nesting, or you modify this constructor.
        _chain = new SupplierValidationHandler(products, suppliers,
                     new PriceCalculationHandler(products,
                         new StockCheckHandler(products,
                             new WarehouseDispatchHandler(products))));
    }

    public OrderContext Process(string productId, string region, int quantity, string customerId)
    {
        var context = new OrderContext
        {
            ProductId = productId,
            Region = region,
            RequestedQuantity = quantity,
            CustomerId = customerId
        };

        _chain.Handle(context);
        return context;
    }
}
