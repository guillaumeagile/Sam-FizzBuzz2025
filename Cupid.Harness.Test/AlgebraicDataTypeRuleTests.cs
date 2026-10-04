using AwesomeAssertions;
using Cupid.Harness.Rules;

namespace Cupid.Harness.Test;

public class AlgebraicDataTypeRuleTests
{
    private readonly AlgebraicDataTypeRule _rule = new();

    [Fact]
    public void OneOfWithMultipleAlternatives_ShouldPass()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            using OneOf;
            namespace Sample;
            public class ProductLookup
            {
                public OneOf<Product, ProductNotFound> Find(string id) => throw new System.NotImplementedException();
            }
            public sealed record Product;
            public sealed record ProductNotFound;
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }

    [Fact]
    public void FullyQualifiedOneOfWithMultipleAlternatives_ShouldPass()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public class ProductLookup
            {
                public OneOf.OneOf<Product, ProductNotFound> Find(string id) => throw new System.NotImplementedException();
            }
            public sealed record Product;
            public sealed record ProductNotFound;
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }

    [Fact]
    public void LookalikeTypeNamedOneOf_ShouldFail()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace OneOf
            {
                public record OneOf<T>;
            }
            namespace Sample;
            public class ProductLookup
            {
                public OneOf.OneOf<Product> Find(string id) => throw new System.NotImplementedException();
            }
            public sealed record Product;
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().ContainSingle(v => v.Message.Contains("No OneOf ADT"));
    }

    [Fact]
    public void RecordHierarchyWithoutOneOf_ShouldFail()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public abstract record Shape;
            public sealed record Circle : Shape;
            public sealed record Square : Shape;
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().ContainSingle(v => v.Message.Contains("No OneOf ADT"));
    }

    [Fact]
    public void NoUnion_ShouldFail()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public class ProductLookup
            {
                public Product? Find(string id) => null;
            }
            public sealed record Product;
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().ContainSingle(v => v.Message.Contains("No OneOf ADT"));
    }
}
