using AwesomeAssertions;
using OmniProduct_CoreDomain.Models;
using OmniProduct_CoreDomain.Models.Suppliers;
using OmniProduct_CoreDomain.Services;

namespace OmniProduct_Core.Test;

public class ProductServiceTests
{
    // Weak test #1: tests that a constructor sets a property.
    // Not wrong, but tells us nothing about domain behaviour.
    [Fact]
    public void Product_Name_ShouldBeSet()
    {
        var price = new Price(100m, "EUR");
        var supplier = new EuropeanSupplier { Id = Guid.NewGuid(), Name = "Acme", Email = "acme@example.com", Region = "FR" };
        var warehouse = new Warehouse { Id = Guid.NewGuid(), Name = "Paris Hub", Address = "1 rue de la Paix", Region = "FR" };

        var product = new ActiveProduct(
            id: "p1",
            name: "Super Widget",
            slug: "super-widget",
            price: price,
            discounts: new List<string>(),
            images: new Dictionary<string, string>(),
            suppliersRegions: new Dictionary<string, Supplier> { { "FR", supplier } },
            weight: 0.5,
            dimensions: "10x5x3",
            quantity: 0,
            stock: 0,
            warehouse: warehouse
        );

        product.Name.Should().Be("Super Widget");
    }

    // Weak test #2: tests that adding a supplier to the service increments a list.
    // Asserts internal plumbing, not business intent.
    [Fact]
    public void AddSupplier_ShouldReturnSupplierWithCorrectRegion()
    {
        var service = new ProductService();

        var supplier = service.AddSupplier("Acme", "acme@example.com", "FR");

        supplier.Region.Should().Be("FR");
    }

    // Too-broad test: one test, full lifecycle, asserting everything everywhere.
    // Passes today, impossible to diagnose when it fails.
    [Fact]
    public void FullProductLifecycle_ShouldWork()
    {
        var service = new ProductService();

        var supplier = service.AddSupplier("Acme", "acme@example.com", "FR");
        var warehouse = service.AddWarehouse("Paris Hub", "1 rue de la Paix", "FR");

        var product = service.AddProduct("Super Widget", "FR", 100m, "EUR");

        product.Should().NotBeNull();
        product.Name.Should().Be("Super Widget");
        product.Stock.Should().Be(0);
        product.Status.Should().Be("active");

        service.ReceiveStock(product.Id, 50);
        product.Stock.Should().Be(50);

        service.SellProduct(product.Id, 10);
        product.Stock.Should().Be(40);
        product.Status.Should().Be("active");

        service.SellProduct(product.Id, 40);
        product.Stock.Should().Be(0);
        product.Status.Should().Be("out_of_stock");

        var resellerPrice = service.GetResellerPrice(product.Id);
        resellerPrice.Should().Be(124m); // 100 + 20% margin + 20% VAT on margin

        service.DeprecateProduct(product.Id);
        product.Status.Should().Be("deprecated");
        product.Stock.Should().Be(0);

        var catalog = service.GetCatalog("FR");
        catalog.Should().NotContain(p => p.Id == product.Id);
    }

    // Dirty state test #1: asserts the runtime type name as a string, because the "state
    // hierarchy" (ActiveProduct/OutOfStockProduct/DeprecatedProduct/EmbargoProduct) exists
    // but nothing ever actually switches an instance's type - Sell()/Deprecate() just mutate
    // the Status string in place on whatever concrete type it started as.
    [Fact]
    public void NewProduct_IsAlwaysActiveProductType_RegardlessOfStatusString()
    {
        var service = new ProductService();
        service.AddSupplier("Acme", "acme@example.com", "FR");
        service.AddWarehouse("Paris Hub", "1 rue de la Paix", "FR");

        var product = service.AddProduct("Widget", "FR", 10m, "EUR");

        // "Proves" the state hierarchy is decorative: driving Status to "out_of_stock" or
        // "deprecated" never produces an OutOfStockProduct/DeprecatedProduct instance.
        product.GetType().Name.Should().Be(nameof(ActiveProduct));

        service.SellProduct(product.Id, 0); // no-op sell, stock stays 0-ish, still ActiveProduct
        product.GetType().Name.Should().Be(nameof(ActiveProduct));
    }

    // Dirty state test #2: string-compares Status after a transition that "should" have
    // produced an OutOfStockProduct per the hierarchy's own naming, but didn't.
    [Fact]
    public void SellingLastUnit_SetsStatusString_ButLeavesTypeAsActiveProduct()
    {
        var service = new ProductService();
        service.AddSupplier("Acme", "acme@example.com", "FR");
        service.AddWarehouse("Paris Hub", "1 rue de la Paix", "FR");
        var product = service.AddProduct("Widget", "FR", 10m, "EUR");
        service.ReceiveStock(product.Id, 1);

        service.SellProduct(product.Id, 1);

        product.Status.Should().Be("out_of_stock");
        product.GetType().Name.Should().NotBe(nameof(OutOfStockProduct));
        product.Should().BeOfType<ActiveProduct>();
    }

    // Dirty state test #3: same story for Deprecate() vs DeprecatedProduct.
    [Fact]
    public void Deprecate_SetsStatusString_ButLeavesTypeAsActiveProduct()
    {
        var service = new ProductService();
        service.AddSupplier("Acme", "acme@example.com", "FR");
        service.AddWarehouse("Paris Hub", "1 rue de la Paix", "FR");
        var product = service.AddProduct("Widget", "FR", 10m, "EUR");

        service.DeprecateProduct(product.Id);

        product.Status.Should().Be("deprecated");
        product.Should().NotBeOfType<DeprecatedProduct>();
        product.Should().BeOfType<ActiveProduct>();
    }

    // Dirty state test #4: exercises the never-instantiated branches of the hierarchy directly,
    // by hand, just to prove they compile and carry their own hardcoded Status default that
    // has nothing to do with how any real Status value ever gets set elsewhere.
    [Fact]
    public void OrphanStateSubclasses_DefaultToOwnHardcodedStatus_NeverWiredToAnything()
    {
        var outOfStock = new OutOfStockProduct();
        var deprecated = new DeprecatedProduct();
        var embargo = new EmbargoProduct();

        outOfStock.Status.Should().Be("out_of_stock");
        deprecated.Status.Should().Be("deprecated");
        embargo.Status.Should().Be("UnderEmbargo"); // inconsistent casing vs every other Status literal

        // Nothing in ProductService or ProductBase ever new()s these three - dead branches
        // kept alive only by this test.
    }
}
