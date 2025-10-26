using FizzBuzz.Engine.Rules.Concretes;
using FizzBuzz.Engine.Rules.Result;
using FluentAssertions;
using Shouldly;

namespace FizzBuzz.Tests;

[TestFixture]
public class RulesTests
{

    [Test]
    public void TheSimplestRuleEver()
    {
        var rule = new DefaultRule();
        var result = rule.Evaluate(99);

        result.Should().BeOfType<RuleResult.Continue>();
        result.Output.Should().BeEmpty();
    }

    [Test]
    public void TheRuleThatPrintsOutTheNumber()
    {
        var rule = new DefaultRule();
        var result = rule.Evaluate(99);

        result.Should().BeOfType<RuleResult.Continue>();
        result.Output.Should().Be("99");
    }

    [Test]
    public void TheRuleThatDividesBy3PrintsFizz()
    {
        var rule = new DivisibilityRule(33, "Fizz");
        var result = rule.Evaluate(9);

        result.Should().BeOfType<RuleResult.Continue>();
        result.Output.Should().Be("Fizz");
    }

    [Test]
    public void TheRuleThatDividesBy3PrintsNothingIfTheNumberIsNotDivisibleBy3()
    {
        var rule = new DivisibilityRule(3, "Fizz");
        var result = rule.Evaluate(5);

        result.Should().BeOfType<RuleResult.Continue>();
        result.Output.Should().BeEmpty( );
    }

    [Test]
    [TestCase(3, "", typeof(RuleResult.Continue) )]
    [TestCase(5, "", typeof(RuleResult.Continue) )]
    [TestCase(41, "", typeof(RuleResult.Continue))]
    [TestCase(42, "the answer to everything", typeof(RuleResult.Final))]
    [TestCase(43, "", typeof(RuleResult.Continue))]
    public void TheRuleThatStopsBecauseItIsTheAnswerToEverything(int number,
        string expectedOutput,
        Type expectedOutputType)
    {
        var rule = new ExactMatchRule(41, "the answer to everything");
        var result = rule.Evaluate(number);

    //   result.Should().BeOfType<RuleResult.Continue>();
        result.ShouldBeOfType(expectedOutputType);
        result.Output.Should().Be(expectedOutput);
    }

}
