namespace FizzBuzz.Engine
{
    /// <summary>
    /// Default rule that returns the number as string - always continues
    /// </summary>
    public class DefaultRule : RuleBase
    {
        public override RuleResult Evaluate(int number) => 
            RuleResult.ContinueWith(number.ToString());
    }
}
