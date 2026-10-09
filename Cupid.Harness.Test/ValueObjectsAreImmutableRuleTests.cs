using AwesomeAssertions;
using Cupid.Harness.Rules;

namespace Cupid.Harness.Test;

public class ValueObjectsAreImmutableRuleTests
{
    private readonly ValueObjectsAreImmutableRule _rule = new();

    [Fact]
    public void NoValueObjectAtAll_ShouldBeFlagged()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public class Product { }
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().ContainSingle(v => v.Message.Contains("No value object"));
    }

    [Fact]
    public void ImmutablePositionalRecord_ShouldPass()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            using System;
            namespace Sample;
            public record Perishable(DateOnly SellByDate, DateOnly UseByDate);
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }

    [Fact]
    public void ReadonlyRecordStruct_ShouldPass()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            using System;
            namespace Sample;
            public readonly record struct NonExpiring(DateOnly BestBeforeDate);
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }

    [Fact]
    public void RecordWithInitOnlyProperty_ShouldPass()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public record Money { public decimal Amount { get; init; } }
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }

    [Fact]
    public void RecordWithSetter_ShouldBeFlagged()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public record Money { public decimal Amount { get; set; } }
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().ContainSingle(v => v.Message.Contains("Money") && v.Message.Contains("Amount"));
    }

    [Fact]
    public void RecordWithMutableCollection_ShouldBeFlagged()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            using System.Collections.Generic;
            namespace Sample;
            public record Basket(List<string> Items);
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().ContainSingle(v => v.Message.Contains("Basket") && v.Message.Contains("Items"));
    }

    [Fact]
    public void RecordWithMutableField_ShouldBeFlagged()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public record Money { public decimal Amount; }
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().ContainSingle(v => v.Message.Contains("Money") && v.Message.Contains("Amount"));
    }

    [Fact]
    public void RecordImplementingIDentifiable_IsNotAValueObject_AndIsNotCounted()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public interface IDentifiable { }
            public record Customer : IDentifiable { public string Name { get; set; } }
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().ContainSingle(v => v.Message.Contains("No value object"));
    }

    [Fact]
    public void NonValueObjectsAreNotJudged_WhenAnImmutableValueObjectExists()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public interface IDentifiable { }
            public record Customer : IDentifiable { public string Name { get; set; } }
            public record Money(decimal Amount);
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }
}
