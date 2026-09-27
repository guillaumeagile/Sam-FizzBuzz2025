using AwesomeAssertions;
using Cupid.Harness.Rules;

namespace Cupid.Harness.Test;

public class RecordsAreValueObjectsRuleTests
{
    private readonly RecordsAreValueObjectsRule _rule = new();

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
    public void RecordImplementingIDentifiable_ShouldBeFlagged()
    {
        var (trees, compilation) = RuleTestHarness.Compile(IDentifiable + """
            namespace OmniProduct_CoreDomain.Models;
            public record Price(decimal Amount, string Currency) : OmniProduct_CoreDomain.Abstractions.IDentifiable
            {
                string OmniProduct_CoreDomain.Abstractions.IDentifiable.Id { get; set; } = "";
            }
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().Contain(v => v.Message.Contains("Price") && v.Message.Contains("IDentifiable"));
    }

    [Fact]
    public void PriceAsPlainRecordUnderModels_ShouldPass()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace OmniProduct_CoreDomain.Models;
            public record Price(decimal Amount, string Currency);
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }

    [Fact]
    public void MissingPriceRecordUnderModels_ShouldBeFlagged()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace OmniProduct_CoreDomain.Models;
            public record Margin(decimal Percentage);
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().ContainSingle(v => v.Message.Contains("Price") && v.Message.Contains("Models"));
    }

    [Fact]
    public void PriceRecordOutsideModelsNamespace_ShouldStillBeFlaggedAsMissing()
    {
        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public record Price(decimal Amount, string Currency);
            """);

        var violations = _rule.Check(trees, compilation);

        violations.Should().ContainSingle(v => v.Message.Contains("Price"));
    }
}
