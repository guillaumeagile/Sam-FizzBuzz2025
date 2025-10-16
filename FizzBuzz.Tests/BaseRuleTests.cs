using FizzBuzz.Engine;
using FluentAssertions;

namespace FizzBuzz.Tests;

[TestFixture]
public class BaseRuleTests
{
    // Concrete implementation of BaseRule for testing purposes
    private class TestRule : BaseRule
    {
        public TestRule(int priority = 1)
        {
            Priority = priority;
        }

        public override string Evaluate(int number)
        {
            return number.ToString();
        }
    }


    [Test]
    public void CompareTo_WithSamePriority_ReturnsZero()
    {
        // Arrange
        var rule1 = new TestRule(5);
        var rule2 = new TestRule(5);

        // Act
        var result = rule1.CompareTo(rule2);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public void CompareTo_WithLowerPriorityRule_ReturnsNegative()
    {
        // Arrange
        var rule1 = new TestRule(3);
        var rule2 = new TestRule(5);

        // Act
        var result = rule1.CompareTo(rule2);

        // Assert
        result.Should().BeLessThan(0);
    }

    [Test]
    public void CompareTo_WithHigherPriorityRule_ReturnsPositive()
    {
        // Arrange
        var rule1 = new TestRule(7);
        var rule2 = new TestRule(3);

        // Act
        var result = rule1.CompareTo(rule2);

        // Assert
        result.Should().BeGreaterThan(0);
    }


    [Test] // ask yourself why this test is failing
    public void CompareTo_WithNullArgument_ReturnsOne()
    {
        // Arrange
        var rule = new TestRule();

        // Act
        var result = rule.CompareTo(null);
        //shall we accept null or not ?
        //we must be explicit about it, if we accept null, we must return 1

        // Assert
        result.Should().Be(1);

        // WE NEED A DISCUSSION HERE TO START THE WORKSHOP

        // do we have to raise an exception ?
        //
        // short answer : NO, because we are comparing two objects, and C# standards say
        // Standard .NET Behavior: According to .NET conventions, any object is considered greater than null.
        // When comparing with null, the method should return 1 (or any positive number).
        // with NULLABLES we can be explicit about it, if we not raise an exception, we must return 1


        // INDEEP: Idiomatic of C# allows us to accept null, at the level of the CompareTo method
        // but our Product Owner said that it should be impossible to insert a null rule in the list
        //
        // FROM THE PERSPECTIVE OF THE CLIENT, SHE WANTS AN ELABORATE FIZZBUZZ WITH CUSTOM RULES
        // as many, and with the right order, and with possibility to terminal rules.

        // NOW WE ARE READY TO MOVE TO CUPID !!!!!!
    }
}
