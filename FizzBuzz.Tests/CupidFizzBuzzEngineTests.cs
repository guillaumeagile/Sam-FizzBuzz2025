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

            };

            var engine = new CupidFizzBuzzEngine(rules);
            
            // Act & Assert: Test composition of different rule combinations
            
            // 1. Single rule matches (3)
            engine.Evaluate(3).Should().Be("Fizz",
                "divisible by 3 only");
            
            // 2. Two rules compose (15 = 3×5)
            engine.Evaluate(15).Should().Be("FizzBuzz",
                "divisible by both 3 and 5, outputs should concatenate");
            
            // 3. Three rules compose (105 = 3×5×7)
            engine.Evaluate(105).Should().Be("FizzBuzzBang",
                "divisible by 3, 5, and 7 - all three rules compose");
            
            // 4. Final rule stops composition (30 = 3×5)
            engine.Evaluate(30).Should().Be("Special!",
                "exact match rule is final, stops before Fizz/Buzz can accumulate");
            
            // 5. No rules match - returns number
            engine.Evaluate(1).Should().Be("1",
                "no rules match, should return the number itself");
            
            // 6. Partial composition (21 = 3×7)
            engine.Evaluate(21).Should().Be("FizzBang", 
                "divisible by 3 and 7, but not 5");
        }



    }
}
