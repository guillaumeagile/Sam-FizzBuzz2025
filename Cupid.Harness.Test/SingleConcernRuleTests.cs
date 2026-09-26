using AwesomeAssertions;
using Cupid.Harness.Rules;

namespace Cupid.Harness.Test;

public class SingleConcernRuleTests
{
    private static UbiquitousLanguageMap SampleMap()
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, """
            {
              "concerns": {
                "Catalog": ["AddImage", "GetCatalog"],
                "Pricing": ["GetResellerPrice", "SetMargin"],
                "Storage": ["ReceiveStock"]
              }
            }
            """);
        try
        {
            return UbiquitousLanguageMap.Load(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ClassMixingTwoConcerns_ShouldBeFlagged()
    {
        var rule = new SingleConcernRule(SampleMap(), maxConcernsPerClass: 1);

        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public class ProductService
            {
                public void AddImage(string url) { }
                public void SetMargin(decimal margin) { }
            }
            """);

        var violations = rule.Check(trees, compilation);

        violations.Should().ContainSingle(v =>
            v.Message.Contains("ProductService") &&
            v.Message.Contains("Catalog") &&
            v.Message.Contains("Pricing"));
    }

    [Fact]
    public void ClassWithSingleConcern_ShouldPass()
    {
        var rule = new SingleConcernRule(SampleMap(), maxConcernsPerClass: 1);

        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public class CatalogService
            {
                public void AddImage(string url) { }
                public void GetCatalog() { }
            }
            """);

        var violations = rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }

    [Fact]
    public void ClassWithNoRecognizedConcern_ShouldPass()
    {
        var rule = new SingleConcernRule(SampleMap(), maxConcernsPerClass: 1);

        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public class Utility
            {
                public void Frobnicate() { }
            }
            """);

        var violations = rule.Check(trees, compilation);

        violations.Should().BeEmpty();
    }

    [Fact]
    public void ThreeConcernsMixed_ShouldReportAllThreeInMessage()
    {
        var rule = new SingleConcernRule(SampleMap(), maxConcernsPerClass: 1);

        var (trees, compilation) = RuleTestHarness.Compile("""
            namespace Sample;
            public class ProductService
            {
                public void AddImage(string url) { }
                public void SetMargin(decimal margin) { }
                public void ReceiveStock(int quantity) { }
            }
            """);

        var violations = rule.Check(trees, compilation);

        violations.Should().ContainSingle();
        violations[0].Message.Should().Contain("Catalog");
        violations[0].Message.Should().Contain("Pricing");
        violations[0].Message.Should().Contain("Storage");
    }
}
