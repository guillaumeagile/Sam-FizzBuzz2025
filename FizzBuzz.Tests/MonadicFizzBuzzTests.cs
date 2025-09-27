using FizzBuzz.Engine;
using FluentAssertions;
using static FizzBuzz.Engine.MonadicFizzBuzz;
using static FizzBuzz.Engine.MonadicFizzBuzz.Pure;

namespace FizzBuzz.Tests
{
    [TestFixture]
    public class MonadicFizzBuzzTests
    {
        [Test]
        public void MonadicOperations_ShouldWork()
        {
            // Arrange
            var continue1 = RuleResult.ContinueWith("Fizz");
            var continue2 = RuleResult.ContinueWith("Buzz");
            var final = RuleResult.StopWith("Final");

            // Act & Assert - Bind operation
            var boundResult = continue1.Bind(output => RuleResult.ContinueWith(output + "Test"));
            boundResult.Should().BeOfType<RuleResult.Continue>()
                .Which.Output.Should().Be("FizzTest");

            // Act & Assert - Map operation
            var mappedResult = continue1.Map(output => output.ToUpper());
            mappedResult.Should().BeOfType<RuleResult.Continue>()
                .Which.Output.Should().Be("FIZZ");

            // Act & Assert - Combine operation
            var combined = continue1.Combine(continue2);
            combined.Should().BeOfType<RuleResult.Continue>()
                .Which.Output.Should().Be("FizzBuzz");

            // Final should short-circuit
            var finalCombined = final.Combine(continue1);
            finalCombined.Should().BeOfType<RuleResult.Final>();
        }

        // TODOOOOO TODO TODO
        // completer gtout ca pour faire émerger la fonction Game.Standard()
        // et un Game.Extended()

        [Test]
        public void PatternMatching_ShouldWork()
        {
            // Arrange
            var continueResult = RuleResult.ContinueWith("Test");
            var finalResult = RuleResult.StopWith("Final");

            // Act & Assert
            var continueMatched = continueResult.Match(
                onContinue: output => $"Continue: {output}",
                onFinal: output => $"Final: {output}"
            );
            continueMatched.Should().Be("Continue: Test");

            var finalMatched = finalResult.Match(
                onContinue: output => $"Continue: {output}",
                onFinal: output => $"Final: {output}"
            );
            finalMatched.Should().Be("Final: Final");
        }

        [Test]
        public void PureFunctions_ShouldWork()
        {
            // Act & Assert - Point-free style functions
            IsDivisibleBy3(3).Should().BeTrue();
            IsDivisibleBy3(4).Should().BeFalse();

            Fizz(3).Should().Be("Fizz");
            Fizz(4).Should().Be("");

            Buzz(5).Should().Be("Buzz");
            Bang(7).Should().Be("Bang");

            Answer(42).Should().Be("The answer to the meaning of life, the universe, and everything");
        }

        [Test]
        public void FunctionComposition_ShouldWork()
        {
            // Act & Assert
            Pure.FizzBuzz(1).Should().Be("1");
            Pure.FizzBuzz(3).Should().Be("Fizz");
            Pure.FizzBuzz(5).Should().Be("Buzz");
            Pure.FizzBuzz(15).Should().Be("FizzBuzz");
            Pure.FizzBuzz(42).Should().Be("The answer to the meaning of life, the universe, and everything");
        }

        [Test]
        public void PipelineComposition_ShouldWork()
        {
            // Act & Assert
            PipelineFizzBuzz(1).Should().Be("1");
            PipelineFizzBuzz(3).Should().Be("Fizz");
            PipelineFizzBuzz(5).Should().Be("Buzz");
            PipelineFizzBuzz(15).Should().Be("FizzBuzz");
        }

        [Test]
        public void CurriedFunctions_ShouldWork()
        {
            // Act & Assert - Curried functions
            CurriedFizz(3).Should().Be("Fizz");
            CurriedFizz(4).Should().Be("");

            CurriedBuzz(5).Should().Be("Buzz");
            CurriedBuzz(6).Should().Be("");

            // Test partial application
            var divisibleBy2 = DivisibleBy(2);
            var even = divisibleBy2("Even");
            
            even(2).Should().Be("Even");
            even(3).Should().Be("");
        }

        [Test]
        public void MonadicComposition_ShouldWork()
        {
            // Arrange
            var rules = new[]
            {
                FunctionalFizzBuzz.Divisible(3, "Fizz"),
                FunctionalFizzBuzz.Divisible(5, "Buzz")
            };

            var composer = ComposeRules(rules);

            // Act & Assert
            composer(1).Should().Be("1");
            composer(3).Should().Be("Fizz");
            composer(5).Should().Be("Buzz");
            composer(15).Should().Be("FizzBuzz");
        }

        [Test]
        public void ApplicativeFunctor_ShouldWork()
        {
            // Arrange
            var func = RuleResult.ContinueWith("Fizz");
            var value = RuleResult.ContinueWith("Buzz");
            var final = RuleResult.StopWith("Final");

            // Act & Assert
            var applied = func.Apply(value);
            applied.Should().BeOfType<RuleResult.Continue>()
                .Which.Output.Should().Be("FizzBuzz");

            // Final should short-circuit
            var finalApplied = final.Apply(value);
            finalApplied.Should().BeOfType<RuleResult.Final>();
        }
    }
}
