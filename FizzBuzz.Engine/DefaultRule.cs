namespace FizzBuzz.Engine
{
    /// <summary>
    /// Default rule that returns the number as string - always continues
    /// Using record for value-based equality (functional programming)
    /// </summary>
    public record DefaultRule : RuleBase
    {
        public override RuleResult Evaluate(int number) => 
            RuleResult.ContinueWith(number.ToString());
    }
}
