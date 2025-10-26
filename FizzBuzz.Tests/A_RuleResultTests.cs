using FizzBuzz.Engine.Rules.Result;
using FluentAssertions;

namespace FizzBuzz.Tests;

public class A_RuleResultTests
{
    [Test]
    public void ContinueWith()
    {
        var res = RuleResult.ContinueWith("output");

        var subject = res.Should().BeOfType<RuleResult.Continue>().Subject;
        subject.Output.Should().Be("output");
    }

    [Test]
    public void StopWith()
    {
        var res = RuleResult.StopWith("output");
        var subject = res.Should().BeOfType<RuleResult.Final>().Subject;
        subject.Output.Should().Be("output");
    }

    [Test]
    public void Empty()
    {
        var res = RuleResult.Empty;
        res.Should().BeOfType<RuleResult.Continue>().Subject.Output.Should().BeEmpty();
    }
}
