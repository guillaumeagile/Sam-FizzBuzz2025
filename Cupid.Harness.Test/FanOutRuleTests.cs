using AwesomeAssertions;
using Cupid.Harness.Rules;

namespace Cupid.Harness.Test;

public class FanOutRuleTests
{
    private readonly FanOutRule _rule = new(maxDistinctDomainTypes: 3);

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
    public void ClassTouchingFourEntityTypes_ShouldBeFlagged()
    {
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

        violations.Should().ContainSingle(v => v.Message.Contains("ProductService") && v.Message.Contains("4 distinct entity types"));
    }

    [Fact]
    public void ClassTouchingThreeEntityTypes_ShouldPass()
    {
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            namespace OmniProduct_CoreDomain.Models;
            using OmniProduct_CoreDomain.Abstractions;
            public class Supplier : IDentifiable { public string Id { get; set; } }
            public class Warehouse : IDentifiable { public string Id { get; set; } }
            public class Price : IDentifiable { public string Id { get; set; } }

            public class ProductService
            {
                private readonly Supplier _supplier;
                private readonly Warehouse _warehouse;
                private readonly Price _price;

                public ProductService(Supplier supplier, Warehouse warehouse, Price price)
                {
                    _supplier = supplier;
                    _warehouse = warehouse;
                    _price = price;
                }
            }
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }

    [Fact]
    public void NonEntityDomainTypes_ShouldNotCountTowardFanOut()
    {
        // Value objects / services that don't implement IDentifiable are not entities, so a class
        // may reference more than the cap of them without tripping HA7.
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            namespace OmniProduct_CoreDomain.Models;
            public class Money { }
            public class Slug { }
            public class TaxRate { }
            public class Margin { }

            public class PricingCalculator
            {
                private readonly Money _money;
                private readonly Slug _slug;
                private readonly TaxRate _taxRate;
                private readonly Margin _margin;

                public PricingCalculator(Money money, Slug slug, TaxRate taxRate, Margin margin)
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
    public void IDentifiableTypesOutsideModelsNamespace_ShouldNotCountTowardFanOut()
    {
        // HA9 should already forbid an IDentifiable implementer outside Models.*, but HA7 doesn't
        // rely on that - it only trusts entities declared under OmniProduct_CoreDomain.Models.*.
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            namespace Sample;
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
        var (trees, compilation) = RuleTestHarness.Compile("""
            using System;
            using System.Collections.Generic;
            namespace Sample;
            public class Widget
            {
                private readonly List<string> _tags = new();
                private readonly Guid _id = Guid.NewGuid();
                private readonly DateTime _createdAt = DateTime.Now;
                private readonly Dictionary<string, string> _images = new();
            }
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }
}
