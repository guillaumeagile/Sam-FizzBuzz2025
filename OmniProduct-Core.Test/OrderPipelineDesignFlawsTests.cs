using AwesomeAssertions;
using OmniProduct_CoreDomain.Models;
using OmniProduct_CoreDomain.Ordering;

namespace OmniProduct_Core.Test;

public class OrderPipelineDesignFlawsTests
{
    private (Product product, Supplier supplier, OrderPipeline pipeline) BuildPipeline(int stock = 10)
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
            quantity: stock,
            stock: stock,
            warehouse: warehouse
        );

        var pipeline = new OrderPipeline(new List<Product> { product }, new List<Supplier> { supplier });
        return (product, supplier, pipeline);
    }

    // Flaw #1: StockCheckHandler mutates product.Stock as a side effect.
    // A well-designed pipeline should return the new stock in its result,
    // not silently mutate the source object.
    // FAILS: product.Stock is 7, not 10 — the pipeline changed it without telling anyone.
    [Fact]
    public void AfterSuccessfulOrder_SourceProduct_ShouldNotBeMutated()
    {
        var (product, _, pipeline) = BuildPipeline(stock: 10);

        pipeline.Process("p1", "FR", 3, "customer-42");

        product.Stock.Should().Be(10); // the pipeline should not reach into the domain object
    }

    // Flaw #2: running the pipeline twice with the same customer and order should be idempotent.
    // FAILS: stock goes to 4 — the pipeline has no memory, no guard, no idempotency.
    [Fact]
    public void RunningPipelineTwiceForSameOrder_ShouldNotDecrementStockTwice()
    {
        var (product, _, pipeline) = BuildPipeline(stock: 10);

        pipeline.Process("p1", "FR", 3, "customer-42");
        pipeline.Process("p1", "FR", 3, "customer-42");

        product.Stock.Should().Be(7); // second call should have been a no-op
    }

    // Flaw #3: when an order fails mid-chain, the result should be clean.
    // A failed order context should carry no pricing data — it is meaningless and misleading.
    // FAILS: CalculatedPrice is > 0 even though the order was rejected.
    [Fact]
    public void WhenOrderFails_ResultContext_ShouldNotContainPartialData()
    {
        var (_, _, pipeline) = BuildPipeline(stock: 1);

        var result = pipeline.Process("p1", "FR", 5, "customer-42");

        result.CanContinue.Should().BeFalse();
        result.CalculatedPrice.Should().Be(0); // a failed order has no price
    }

    // Flaw #4: StockCheckHandler should be testable in isolation.
    // A well-designed handler exposes a public Process(OrderContext) method.
    // FAILS TO COMPILE if you try — Process() is protected, only callable via the chain.
    // This test exposes the problem by proxy: we cannot assert stock-check behaviour
    // without also depending on supplier validation passing first.
    [Fact]
    public void StockCheck_ShouldBeTestableWithoutSupplierValidation()
    {
        // Ideally this test would be:
        //   var handler = new StockCheckHandler(products);
        //   var context = new OrderContext { ProductId = "p1", RequestedQuantity = 5 };
        //   handler.Process(context);
        //   context.StockAvailable.Should().BeFalse();
        //
        // Instead we must wire the whole pipeline and pray supplier validation
        // doesn't interfere with what we're actually trying to test.

        var (_, _, pipeline) = BuildPipeline(stock: 2);

        var result = pipeline.Process("p1", "FR", 5, "customer-42");

        // We want to assert on stock check behaviour,
        // but the failure reason could have come from any handler in the chain.
        result.FailureReason.Should().Be("Insufficient stock: requested 5, available 2");
    }

    [Fact]
    public void SingleResponsibilityCheckIsImpossible()
    {
        var products = new List<Product>();
        var handler = new StockCheckHandler(products, null);
        var context = new OrderContext { ProductId = "p1", RequestedQuantity = 10 };
      //     handler.Process(context);
        context.StockAvailable.Should().BeTrue();

    }

    // Flaw #5: when there is no supplier for the requested region, the order fails —
    // but the context still exposes SupplierEmail as a public property, now null.
    // Any caller who reads result.SupplierEmail without checking CanContinue first gets a null.
    // The design makes this accident trivially easy: nothing on the type signals "this may be absent".
    // FAILS: a properly designed result would make absent fields unreachable when the order failed.
    [Fact]
    public void WhenNoSupplierForRegion_FailedContext_ShouldNotExposeNullFields()
    {
        var (_, _, pipeline) = BuildPipeline(stock: 10);

        // "DE" has no supplier — product only has "FR"
        var result = pipeline.Process("p1", "DE", 1, "customer-42");

        result.CanContinue.Should().BeFalse();
        result.SupplierEmail.Should().NotBeNull(); // null — but nothing in the type told you that
    }
}
