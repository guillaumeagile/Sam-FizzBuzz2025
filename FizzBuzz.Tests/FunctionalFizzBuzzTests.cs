using FizzBuzz.Engine;
using FluentAssertions;
using static FizzBuzz.Engine.FunctionalFizzBuzz;

namespace FizzBuzz.Tests
{
    [TestFixture]
    public class FunctionalFizzBuzzTests
    {
        [Test]
        public void PureFunctions_ShouldWork()
        {
            // Arrange - Pure functions, no objects!
            var fizz = Divisible(3, "Fizz");
            var buzz = Divisible(5, "Buzz");

            // Act & Assert
            fizz(3).Should().BeOfType<RuleResult.Continue>()
                .Which.Output.Should().Be("Fizz");
            
            fizz(4).Should().BeOfType<RuleResult.Continue>()
                .Which.Output.Should().Be(string.Empty);

            buzz(5).Should().BeOfType<RuleResult.Continue>()
                .Which.Output.Should().Be("Buzz");
        }

        [Test]
        public void StandardFizzBuzz_ShouldWork()
        {
            // Act & Assert - Using the curried function
            StandardFizzBuzz(1).Should().Be("1");
            StandardFizzBuzz(3).Should().Be("Fizz");
            StandardFizzBuzz(5).Should().Be("Buzz");
            StandardFizzBuzz(15).Should().Be("FizzBuzz");
            StandardFizzBuzz(21).Should().Be("FizzBang");
            StandardFizzBuzz(42).Should().Be("The answer to the meaning of life, the universe, and everything");
        }

        [Test]
        public void FunctionalComposition_ShouldWork()
        {
            // Arrange - Compose functions
            var customEvaluator = CreateEvaluator(
                Divisible(2, "Even"),
                Divisible(4, "Quad")
            );

            // Act & Assert
            customEvaluator(1).Should().Be("1");
            customEvaluator(2).Should().Be("Even");
            customEvaluator(4).Should().Be("EvenQuad");
        }

        [Test]
        public void FunctionalPipeline_ShouldProcessRange()
        {
            // Arrange
            var evaluator = CreateEvaluator(Rules.Fizz, Rules.Buzz);

            // Act
            var results = ProcessRange(1, 15, evaluator).ToList();

            // Assert
            results.Should().HaveCount(15);
            results[2].Should().Be("Fizz");   // 3
            results[4].Should().Be("Buzz");   // 5
            results[14].Should().Be("FizzBuzz"); // 15
        }

        [Test]
        public void HigherOrderFunctions_ShouldCompose()
        {
            // Arrange - Higher-order function composition
            var rules = new Rule[]
            {
                ExactMatch(7, "Lucky Seven"),
                Divisible(7, "Seven")
            };

            // Act & Assert - ExactMatch should win (first rule)
            Evaluate(7, rules).Should().Be("Lucky Seven");
            Evaluate(14, rules).Should().Be("Seven");
        }

        [Test]
        public void PureFunctional_NoSideEffects()
        {
            // Arrange
            var rule = Divisible(3, "Test");

            // Act - Call multiple times
            var result1 = rule(3);
            var result2 = rule(3);
            var result3 = rule(6);

            // Assert - Pure functions always return same result
            result1.Should().BeOfType<RuleResult.Continue>()
                .Which.Output.Should().Be("Test");
            result2.Should().BeOfType<RuleResult.Continue>()
                .Which.Output.Should().Be("Test");
            result3.Should().BeOfType<RuleResult.Continue>()
                .Which.Output.Should().Be("Test");
            
            // Same inputs should produce same outputs (referential transparency)
            ((RuleResult.Continue)result1).Output.Should().Be(((RuleResult.Continue)result2).Output);
        }
    }
}
