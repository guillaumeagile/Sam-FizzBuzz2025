using AwesomeAssertions;
using Cupid.Harness.Rules;

namespace Cupid.Harness.Test;

public class BareIdentifierIsWeakRuleTests
{
    private readonly BareIdentifierIsWeakRule _rule = new();

    private const string Preamble = """
        using System;
        namespace Sample;
        public readonly struct Ulid { }
        public record ProductId(Guid Value);

        """;

    private IReadOnlyList<Violation> Run(string source)
    {
        var (trees, compilation) = RuleTestHarness.Compile(Preamble + source);
        return _rule.Check(trees, compilation);
    }

    [Fact]
    public void IsAWarning() => _rule.Severity.Should().Be(HarnessSeverity.Warning);

    [Fact]
    public void BareGuidId_ShouldWarn() =>
        Run("public class Product { public Guid Id { get; } }").Should().ContainSingle(v => v.Message.Contains("Guid"));

    [Fact]
    public void BareUlidSuffixId_ShouldWarn() =>
        Run("public class Stock { public Ulid SupplierId { get; } }").Should().ContainSingle(v => v.Message.Contains("Ulid"));

    [Fact]
    public void WrapperRecord_ShouldNotWarn() =>
        Run("public class Product { public ProductId Id { get; } }").Should().BeEmpty();

    [Fact]
    public void StringId_ShouldWarn() =>
        Run("public class Product { public string Id { get; } }").Should().ContainSingle(v => v.Message.Contains("string"));

    [Fact]
    public void IntSuffixId_ShouldWarn() =>
        Run("public class Stock { public int ProductId { get; } }").Should().ContainSingle(v => v.Message.Contains("int"));

    [Fact]
    public void WrapperOverString_ShouldWarn() =>
        Run("public record NameId(string Value); public class Product { public NameId Id { get; } }")
            .Should().ContainSingle(v => v.Message.Contains("NameId"));

    [Fact]
    public void ServicesNamespace_IsOutOfScope() =>
        Run("namespace Sample.Services { public class Svc { public Guid Id { get; } } }").Should().BeEmpty();
}
