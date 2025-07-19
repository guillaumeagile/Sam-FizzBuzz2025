namespace FizzBuzz.Engine
{
    /// <summary>
    /// A rule that outputs text when a number is divisible by a specific divisor
    /// </summary>
    public class DivisibilityRule : RuleBase
    {
        public int Divisor { get; }
        public string Output { get; }

        public DivisibilityRule(int divisor, string output)
        {
            Divisor = divisor;
            Output = output;
        }

        public override RuleResult Evaluate(int number) => 
            number % Divisor == 0 
                ? RuleResult.ContinueWith(Output) 
                : RuleResult.Empty;
    }
}
