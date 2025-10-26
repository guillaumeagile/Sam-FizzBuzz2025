using FizzBuzz.Engine.Rules.Abstractions;
using FizzBuzz.Engine.Rules.Result;

namespace FizzBuzz.Engine.Rules.Concretes
{
    /// <summary>
    /// Default rule that returns the number as string - always continues
    /// Using record for value-based equality (functional programming)
    /// </summary>
    public record DefaultRule : RuleBase
    {
        public override RuleResult Evaluate(int number) => 
           throw new System.NotImplementedException();
    }
}
