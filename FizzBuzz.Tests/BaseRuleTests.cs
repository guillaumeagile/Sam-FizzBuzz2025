using FizzBuzz.Engine;
using FluentAssertions;

namespace FizzBuzz.Tests
{
    [TestFixture]
    public class BaseRuleTests
    {
        // Concrete implementation of BaseRule for testing purposes
        private class TestRule : BaseRule
        {
            public TestRule(int priority = 1)
            {
                base.Priority = priority;
            }

            public override string Evaluate(int number)
            {
                return number.ToString();
            }
        }

        [Test]
        public void CompareTo_WithNullArgument_ReturnsOne()
        {
            // Arrange
            var rule = new TestRule();

            // Act
            var result = rule.CompareTo(null);

            // Assert
            result.Should().Be(1);
        }

        [Test]
        public void CompareTo_WithSamePriority_ReturnsZero()
        {
            // Arrange
            var rule1 = new TestRule(priority: 5);
            var rule2 = new TestRule(priority: 5);

            // Act
            var result = rule1.CompareTo(rule2);

            // Assert
            result.Should().Be(0);
        }

        [Test]
        public void CompareTo_WithLowerPriorityRule_ReturnsNegative()
        {
            // Arrange
            var rule1 = new TestRule(priority: 3);
            var rule2 = new TestRule(priority: 5);

            // Act
            var result = rule1.CompareTo(rule2);

            // Assert
            result.Should().BeLessThan(0);
        }

        [Test]
        public void CompareTo_WithHigherPriorityRule_ReturnsPositive()
        {
            // Arrange
            var rule1 = new TestRule(priority: 7);
            var rule2 = new TestRule(priority: 3);

            // Act
            var result = rule1.CompareTo(rule2);

            // Assert
            result.Should().BeGreaterThan(0);
        }
    }
}
