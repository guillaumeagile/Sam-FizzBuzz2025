using AwesomeAssertions;
using Cupid.Harness.Rules;

namespace Cupid.Harness.Test;

public class FanOutRuleTests
{
    private readonly FanOutRule _rule = new(maxDistinctDomainTypes: 3);

    [Fact]
    public void ClassTouchingFourDomainTypes_ShouldBeFlagged()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public class Supplier { }
            public class Warehouse { }
            public class Price { }
            public class Notification { }

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

        violations.Should().ContainSingle(v => v.Message.Contains("ProductService") && v.Message.Contains("4 distinct domain types"));
    }

    [Fact]
    public void ClassTouchingThreeDomainTypes_ShouldPass()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public class Supplier { }
            public class Warehouse { }
            public class Price { }

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
    public void SelfReferences_ShouldNotCountTowardOwnFanOut()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public class Node
            {
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
