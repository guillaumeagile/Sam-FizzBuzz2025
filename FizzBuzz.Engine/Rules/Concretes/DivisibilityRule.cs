using FizzBuzz.Engine.Rules.Abstractions;
using FizzBuzz.Engine.Rules.Result;

namespace FizzBuzz.Engine.Rules.Concretes
{
    /// <summary>
    /// A rule that outputs text when a number is divisible by a specific divisor
    /// Using record for value-based equality (functional programming)
    /// </summary>
    public record DivisibilityRule(int Divisor, string Output) : RuleBase
    {
        public override RuleResult Evaluate(int number) => 
         throw new System.NotImplementedException();
    }
}
