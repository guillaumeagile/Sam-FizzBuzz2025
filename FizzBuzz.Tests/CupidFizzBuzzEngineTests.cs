using FizzBuzz.Engine;
using FluentAssertions;

namespace FizzBuzz.Tests
{
    [TestFixture]
    public class CupidFizzBuzzEngineTests
    {
        [Test]
        public void StandardEngine_ShouldHandleBasicNumbers()
        {
            // Arrange
            var engine = CupidFizzBuzzEngine.Standard();

            // Act & Assert
            engine.Evaluate(1).Should().Be("1");
            engine.Evaluate(3).Should().Be("Fizz");
            engine.Evaluate(5).Should().Be("Buzz");
            engine.Evaluate(7).Should().Be("Bang");
            engine.Evaluate(15).Should().Be("FizzBuzz");
            engine.Evaluate(21).Should().Be("FizzBang");
            engine.Evaluate(42).Should().Be("The answer to the meaning of life, the universe, and everything");
        }

        [Test]
        public void CustomEngine_ShouldBeComposable()
        {
            // Arrange - Domain-focused rule creation
            var rules = new IRule[]
            {
                FizzBuzzRules.Divisible(by: 2, output: "Even"),
                FizzBuzzRules.Divisible(by: 4, output: "Quad")
            };
            
            var engine = new CupidFizzBuzzEngine(rules);

            // Act & Assert
            engine.Evaluate(1).Should().Be("1");
            engine.Evaluate(2).Should().Be("Even");
            engine.Evaluate(4).Should().Be("EvenQuad");
            engine.Evaluate(8).Should().Be("EvenQuad");
        }

        [Test]
        public void ExactMatchRule_ShouldStopProcessing()
        {
            // Arrange - Order matters: ExactMatchRule first will override Fizz
            var rules = new IRule[]
            {
                new ExactMatchRule(3, "Special Three"), // First = higher priority
                FizzBuzzRules.Fizz()
            };
            
            var engine = new CupidFizzBuzzEngine(rules);

            // Act & Assert
            engine.Evaluate(3).Should().Be("Special Three"); // Not "Fizz"
            engine.Evaluate(6).Should().Be("Fizz"); // Normal fizz rule
        }
    }
}
