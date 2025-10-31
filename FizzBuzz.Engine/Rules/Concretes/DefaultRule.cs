using FizzBuzz.Engine.Rules.Abstractions;

namespace FizzBuzz.Engine.Rules.Concretes
{
    /// <summary>
    /// Default rule that returns empty string - always continues
    /// (In a real implementation, this might return the number as string)
    /// </summary>
    public class DefaultRule : RuleBase
    {
        public override string Evaluate(int number) => number.ToString();


    }
}
