using FizzBuzz.Engine.Rules.Abstractions;
using FizzBuzz.Engine.Rules.Result;

namespace FizzBuzz.Engine.Rules.Concretes
{
    /// <summary>
    /// A rule that outputs text for an exact number match and stops further processing
    /// </summary>
    public record ExactMatchRule(int TargetNumber, string Output) : RuleBase
    {
        public override RuleResult Evaluate(int number) => 
         throw new NotImplementedException();
    }
}
