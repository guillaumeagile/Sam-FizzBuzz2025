using AwesomeAssertions;
using Cupid.Harness.Rules;

namespace Cupid.Harness.Test;

public class FanOutRuleTests
{
    private readonly FanOutRule _rule = new();

    private const string IDentifiable = """
        namespace OmniProduct_CoreDomain.Abstractions
        {
            public interface IDentifiable
            {
                string Id { get; set; }
            }
        }

        """;

    [Fact]
    public void EntityTouchingFourEntityTypes_ShouldBeFlagged()
    {
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            namespace OmniProduct_CoreDomain.Models;
            using OmniProduct_CoreDomain.Abstractions;
            public class Supplier : IDentifiable { public string Id { get; set; } }
            public class Warehouse : IDentifiable { public string Id { get; set; } }
            public class Price : IDentifiable { public string Id { get; set; } }
            public class Notification : IDentifiable { public string Id { get; set; } }

            public class Product : IDentifiable
            {
                public string Id { get; set; }

                private readonly Supplier _supplier;
                private readonly Warehouse _warehouse;
                private readonly Price _price;
                private readonly Notification _notification;

                public Product(Supplier supplier, Warehouse warehouse, Price price, Notification notification)
                {
                    _supplier = supplier;
                    _warehouse = warehouse;
                    _price = price;
                    _notification = notification;
                }
            }
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().ContainSingle(v => v.Message.Contains("Product") && v.Message.Contains("4 distinct entity types"));
    }

    [Fact]
    public void EntityTouchingThreeEntityTypes_ShouldBeFlagged()
    {
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            namespace OmniProduct_CoreDomain.Models;
            using OmniProduct_CoreDomain.Abstractions;
            public class Supplier : IDentifiable { public string Id { get; set; } }
            public class Warehouse : IDentifiable { public string Id { get; set; } }
            public class Price : IDentifiable { public string Id { get; set; } }

            public class Product : IDentifiable
            {
                public string Id { get; set; }
                public Supplier Supplier { get; set; }
                public Warehouse Warehouse { get; set; }
                public Price Price { get; set; }
            }
            """);

        _rule.Check(trees, compilation).Should().ContainSingle(v => v.Message.Contains("3 distinct entity types"));
    }

    [Fact]
    public void EntityReferencingASingleOtherEntity_ShouldBeFlagged()
    {
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            namespace OmniProduct_CoreDomain.Models;
            using OmniProduct_CoreDomain.Abstractions;
            public class Warehouse : IDentifiable { public string Id { get; set; } }

            public class Product : IDentifiable
            {
                public string Id { get; set; }
                public Warehouse IsStoredIn { get; set; }
            }
            """);

        _rule.Check(trees, compilation).Should().ContainSingle(v => v.Message.Contains("Product") && v.Message.Contains("Warehouse"));
    }

    [Fact]
    public void EntityReferencingEntityInsideGenericsOrArrays_ShouldBeFlagged()
    {
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            using System.Collections.Generic;
            namespace OmniProduct_CoreDomain.Models;
            using OmniProduct_CoreDomain.Abstractions;
            public class Supplier : IDentifiable { public string Id { get; set; } }
            public class Warehouse : IDentifiable { public string Id { get; set; } }

            public class Product : IDentifiable
            {
                public string Id { get; set; }
                public Dictionary<string, Supplier> Suppliers { get; set; }
                public Warehouse[] Warehouses { get; set; }
            }
            """);

        _rule.Check(trees, compilation).Should().ContainSingle(v => v.Message.Contains("Supplier") && v.Message.Contains("Warehouse"));
    }

    [Fact]
    public void EntityReferencingMethodParameterAndReturnType_ShouldBeFlagged()
    {
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            namespace OmniProduct_CoreDomain.Models;
            using OmniProduct_CoreDomain.Abstractions;
            public class Supplier : IDentifiable { public string Id { get; set; } }

            public class Product : IDentifiable
            {
                public string Id { get; set; }
                public Supplier Pick(Supplier s) => s;
            }
            """);

        _rule.Check(trees, compilation).Should().ContainSingle(v => v.Message.Contains("Supplier"));
    }

    [Fact]
    public void EntityReferencingOtherEntitiesByIdOnly_ShouldPass()
    {
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            using System;
            using System.Collections.Generic;
            namespace OmniProduct_CoreDomain.Models;
            using OmniProduct_CoreDomain.Abstractions;
            public class Supplier : IDentifiable { public string Id { get; set; } }
            public class Warehouse : IDentifiable { public string Id { get; set; } }

            public class Product : IDentifiable
            {
                public string Id { get; set; }
                public Guid? WarehouseId { get; set; }
                public Dictionary<string, Guid> SupplierIdsByRegion { get; set; }
            }
            """);

        _rule.Check(trees, compilation).Should().BeEmpty();
    }

    [Fact]
    public void SameNamedTypeInAnotherNamespace_ShouldNotBeConfusedWithEntity()
    {
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            namespace Other { public class Supplier { } }
            namespace OmniProduct_CoreDomain.Models
            {
                using OmniProduct_CoreDomain.Abstractions;
                public class Supplier : IDentifiable { public string Id { get; set; } }
                public class Product : IDentifiable
                {
                    public string Id { get; set; }
                    public Other.Supplier External { get; set; }
                }
            }
            """);

        _rule.Check(trees, compilation).Should().BeEmpty();
    }

    [Fact]
    public void NonEntityClassTouchingFourEntityTypes_ShouldNotBeFlagged()
    {
        // HA7 only applies to entities. A service wiring together many entities is expected -
        // that's what services do - and is out of scope for this rule entirely.
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            namespace OmniProduct_CoreDomain.Models;
            using OmniProduct_CoreDomain.Abstractions;
            public class Supplier : IDentifiable { public string Id { get; set; } }
            public class Warehouse : IDentifiable { public string Id { get; set; } }
            public class Price : IDentifiable { public string Id { get; set; } }
            public class Notification : IDentifiable { public string Id { get; set; } }

            public class ProductService
            {
                private readonly Supplier _supplier;
                private readonly Warehouse _warehouse;
                private readonly Price _price;
                private readonly Notification _notification;

                public ProductService(Supplier supplier, Warehouse warehouse, Price price, Notification notification)
                {
                    _supplier = supplier;
                    _warehouse = warehouse;
                    _price = price;
                    _notification = notification;
                }
            }
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }

    [Fact]
    public void NonEntityDomainTypes_ShouldNotCountTowardFanOut()
    {
        // Value objects that don't implement IDentifiable are not entities, so an entity may
        // reference more than the cap of them without tripping HA7.
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            namespace OmniProduct_CoreDomain.Models;
            using OmniProduct_CoreDomain.Abstractions;
            public class Money { }
            public class Slug { }
            public class TaxRate { }
            public class Margin { }

            public class Product : IDentifiable
            {
                public string Id { get; set; }

                private readonly Money _money;
                private readonly Slug _slug;
                private readonly TaxRate _taxRate;
                private readonly Margin _margin;

                public Product(Money money, Slug slug, TaxRate taxRate, Margin margin)
                {
                    _money = money;
                    _slug = slug;
                    _taxRate = taxRate;
                    _margin = margin;
                }
            }
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }

    [Fact]
    public void IDentifiableTypesOutsideModelsNamespace_ShouldNotCountTowardFanOutOrBeSubjectToIt()
    {
        // HA9 should already forbid an IDentifiable implementer outside Models.*, but HA7 doesn't
        // rely on that - it only trusts entities declared under OmniProduct_CoreDomain.Models.*,
        // both as the type under test and as what counts toward its fan-out.
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            namespace Sample;
            using OmniProduct_CoreDomain.Abstractions;
            public class Supplier : IDentifiable { public string Id { get; set; } }
            public class Warehouse : IDentifiable { public string Id { get; set; } }
            public class Price : IDentifiable { public string Id { get; set; } }
            public class Notification : IDentifiable { public string Id { get; set; } }

            public class Product : IDentifiable
            {
                public string Id { get; set; }

                private readonly Supplier _supplier;
                private readonly Warehouse _warehouse;
                private readonly Price _price;
                private readonly Notification _notification;

                public Product(Supplier supplier, Warehouse warehouse, Price price, Notification notification)
                {
                    _supplier = supplier;
                    _warehouse = warehouse;
                    _price = price;
                    _notification = notification;
                }
            }
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }

    [Fact]
    public void SelfReferences_ShouldNotCountTowardOwnFanOut()
    {
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            namespace OmniProduct_CoreDomain.Models;
            using OmniProduct_CoreDomain.Abstractions;
            public class Node : IDentifiable
            {
                public string Id { get; set; }
                public Node? Next { get; set; }
            }
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }

    [Fact]
    public void BclTypes_ShouldNotCountTowardFanOut()
    {
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            using System;
            using System.Collections.Generic;
            namespace OmniProduct_CoreDomain.Models;
            using OmniProduct_CoreDomain.Abstractions;
            public class Widget : IDentifiable
            {
                public string Id { get; set; }

                private readonly List<string> _tags = new();
                private readonly Guid _guid = Guid.NewGuid();
                private readonly DateTime _createdAt = DateTime.Now;
                private readonly Dictionary<string, string> _images = new();
            }
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }
}
