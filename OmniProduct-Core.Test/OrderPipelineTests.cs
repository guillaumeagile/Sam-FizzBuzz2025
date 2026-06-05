using AwesomeAssertions;
using OmniProduct_CoreDomain.Models;
using OmniProduct_CoreDomain.Ordering;

namespace OmniProduct_Core.Test;

public class OrderPipelineTests
{
    // Weak test: only checks the happy path end-to-end with a fully wired pipeline.
    // Never exercises a handler in isolation.
    // Never tests what happens if the chain is reordered.
    // Never catches that StockCheckHandler mutates product.Stock as a side effect.
    // Never notices that WarehouseDispatchHandler silently fails if ReservedQuantity is 0.
    // Green = confidence. False confidence.
    [Fact]
    public void OrderPipeline_ShouldDispatch_WhenEverythingIsAvailable()
    {
        var supplier = new Supplier { Id = Guid.NewGuid(), Name = "Acme", Email = "acme@example.com", Region = "FR" };
        var warehouse = new Warehouse { Id = Guid.NewGuid(), Name = "Paris Hub", Address = "1 rue de la Paix", Region = "FR" };
        var price = new Price(100m, "EUR");

        var product = new Product(
            id: "p1",
            name: "Super Widget",
            slug: "super-widget",
            price: price,
            discounts: new List<string>(),
            images: new Dictionary<string, string>(),
            suppliersRegions: new Dictionary<string, Supplier> { { "FR", supplier } },
            weight: 0.5,
            dimensions: "10x5x3",
            quantity: 10,
            stock: 10,
            warehouse: warehouse
        );

        var pipeline = new OrderPipeline(
            new List<Product> { product },
            new List<Supplier> { supplier }
        );

        var result = pipeline.Process("p1", "FR", 3, "customer-42");

        result.CanContinue.Should().BeTrue();
        result.DispatchReference.Should().NotBeNullOrEmpty();

        //Green = confidence. False confidence
    }
}
