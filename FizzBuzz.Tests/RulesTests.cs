using FizzBuzz.Engine.Rules.Concretes;
using FizzBuzz.Engine.Rules.Result;
using FluentAssertions;

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
        var rule = new DefaultRule();
        var result = rule.Evaluate(99);

        result.Should().BeOfType<RuleResult.Continue>();
        result.Output.Should().Be("Fizz");
    }

    [Test]
    public void TheRuleThatStopsBecauseItIsTheAnswerToEverything()
    {
        var rule = new DefaultRule();
        var result = rule.Evaluate(0);

        result.Should().BeOfType<RuleResult.Continue>();
        result.Output.Should().Be("the answer to everything");
    }

}
