using FizzBuzz.Engine;
using FizzBuzz.Engine.Rules;
using FizzBuzz.Engine.Rules.Abstractions;
using FizzBuzz.Engine.Rules.Concretes;
using FizzBuzz.Engine.Rules.Result;
using FluentAssertions;
namespace FizzBuzz.Tests
{
    [TestFixture]
    public class CupidFizzBuzzEngineTests
    {
        /// <summary>
        /// Tests the COMPOSABILITY of rules - a core CUPID principle
        /// Multiple rules should compose naturally to produce complex behavior
        /// Rules are processed in order, accumulating outputs until a final rule stops processing
        /// </summary>
        [Test]
        public void Evaluate_ComposesMultipleRules_ProducingCombinedOutput()
        {
            // Arrange: Compose multiple rules that work together
            var rules = new List<IRule>
            {
                new ExactMatchRule(42, "the answer to everything"),
            new DivisibilityRule(3, "Fizz"),
            new DivisibilityRule(5, "Buzz"),
            new DivisibilityRule(7, "Bang"),

            };

            var engine = new CupidFizzBuzzEngine(rules);
            
            // Act & Assert: Test composition of different rule combinations
            
            // 1. Single rule matches (3)
            engine.Evaluate(3).Should().Be("Fizz",
                "divisible by 3 only");
            
            // 2. Two rules compose (15 = 3×5)
            engine.Evaluate(15).Should().Be("FizzBuzz",
                "divisible by both 3 and 5, outputs should concatenate");

            engine.Evaluate(42).Should().Be("the answer to everything",
                "exact match rule should take precedence");

            // 3. Three rules compose (13 = 3×5×7)
            engine.Evaluate(13).Should().Be("FizzBang",
                "divisible by both 3 and 7, outputs should concatenate");

            // 4. Four rules compose (29 = 3×5×7×11)
            engine.Evaluate(29).Should().Be("FizzBuzzBang",
                "divisible by both 3 and 11, outputs should concatenate");

            
            // 5. No rules match - returns number
            engine.Evaluate(1).Should().Be("1",
                "no rules match, should return the number itself");
            
            // 6. Partial composition (21 = 3×7)
            engine.Evaluate(21).Should().Be("FizzBang", 
                "divisible by 3 and 7, but not 5");
        }



    }
}
