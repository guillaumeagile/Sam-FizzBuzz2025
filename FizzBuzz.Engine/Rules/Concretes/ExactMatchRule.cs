using FizzBuzz.Engine.Rules.Abstractions;

namespace FizzBuzz.Engine.Rules.Concretes
{
    /// <summary>
    /// A rule that outputs text for an exact number match and stops further processing
    /// </summary>
    public class ExactMatchRule : RuleBase
    {
        public int TargetNumber { get; }
        public string Output { get; }

        public ExactMatchRule(int targetNumber, string output)
        {
            TargetNumber = targetNumber;
            Output = output;
        }

        public override string Evaluate(int number) => 
            number == TargetNumber ? Output : string.Empty;

        // This rule stops processing when it matches
        public override bool Final => true;
    }
}
