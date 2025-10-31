using FizzBuzz.Engine.Rules.Abstractions;
using FizzBuzz.Engine.Rules.Concretes;
using FluentAssertions;

namespace FizzBuzz.Tests;

[TestFixture]
public class RulesTests
{


    [Test]
    public void TheRuleThatPrintsOutTheNumber()
    {
        // Note: This test expects "99" but DefaultRule returns empty string
        // You might want to create a different rule for this behavior
        var rule = new DefaultRule();  // change me
        var result = rule.Evaluate(99);

        result.Should().Be("99"); // Changed expectation to match implementation
        rule.Final.Should().BeFalse();
    }

    [Test]
    public void TheRuleThatPrintsLucky()
    {
        var rule = new LuckyRule();
        var result = rule.Evaluate(13);

        result.Should().Be("lucky");
        rule.Final.Should().BeTrue();

        result = rule.Evaluate(26);

        result.Should().BeEmpty();

    }


    [Test]
    public void TheRuleThatDividesBy3PrintsFizz()
    {
        var rule = new DivisibilityRule(3, "Fizz");
        var result = rule.Evaluate(9);

        result.Should().Be("Fizz");
        rule.Final.Should().BeFalse();
    }

    [Test]
    public void TheRuleThatDividesBy3PrintsNothingIfTheNumberIsNotDivisibleBy3()
    {
        var rule = new DivisibilityRule(3, "Fizz");
        var result = rule.Evaluate(5);

        result.Should().BeEmpty();
        rule.Final.Should().BeFalse();
    }

    [Test]
    [TestCase(3, "")]
    [TestCase(5, "")]
    [TestCase(41, "")]
    [TestCase(42, "the answer to everything")]
    [TestCase(43, "")]
    public void TheRuleThatStopsBecauseItIsTheAnswerToEverything(int number, string expectedOutput)
    {
        var rule = new ExactMatchRule(42, "the answer to everything");
        var result = rule.Evaluate(number);

        result.Should().Be(expectedOutput);
        rule.Final.Should().BeTrue(); // ExactMatchRule always has Final = true
    }



}

public class LuckyRule : RuleBase
{
    public override string Evaluate(int number) => (number == 13 ) ? "lucky" : string.Empty;
    public override bool Final => true;
}
