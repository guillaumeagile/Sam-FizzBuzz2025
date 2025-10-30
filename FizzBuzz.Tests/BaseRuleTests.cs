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

        //shall we accept null or not ?
        //we must be explicit about it, if we accept null, we must
        // A => throw an Exception
         Assert.Throws<ArgumentNullException>(() => rule.CompareTo(null));

        // B => return 0
       // var result = rule.CompareTo(null);
       //  result.Should().Be(0);

       // C => return 0
       // result.Should().Be(1);


        // WE NEED A DISCUSSION HERE TO START THE WORKSHOP


        // SHORT ANSWER: NO EXCEPTIONS, because we are comparing two objects, and C# standards say
        //  According to .NET conventions, any object is considered greater than null.
        // When comparing with null, the method should return 1 (or any positive number).
        // with NULLABLES we can be explicit about it, if we not raise an exception, we must return 1


        // IN DEPTH: Idiomatic of C# 14 allows us to accept null, at the level of the CompareTo method
        // but our Product Owner said that it should be impossible to insert a null rule in the list
        //

        // FROM THE PERSPECTIVE OF THE CLIENT, SHE WANTS AN ELABORATE FIZZBUZZ WITH CUSTOM RULES
        // as many, and with the right order, and with the possibility to terminal rules.

        // NOW, WE ARE READY TO MOVE TO CUPID !!!!!!
    }
}
