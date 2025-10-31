using FizzBuzz.Engine.Rules.Abstractions;

namespace FizzBuzz.Engine.Rules.Concretes
{
    /// <summary>
    /// A rule that outputs text when a number is divisible by a specific divisor
    /// </summary>
    public class DivisibilityRule : RuleBase
    {
        private int Divisor { get; }
        private string Output { get; }

        public DivisibilityRule(int divisor, string output)
        {
            Divisor = divisor;
            Output = output;
        }


        public override string Evaluate(int number) => (number % Divisor == 0) ? Output : string.Empty  ;

    }
}
