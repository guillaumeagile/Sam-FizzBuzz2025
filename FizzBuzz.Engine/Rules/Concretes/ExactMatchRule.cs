using FizzBuzz.Engine.Rules.Abstractions;

namespace FizzBuzz.Engine.Rules.Concretes
{
    /// <summary>
    /// A rule that outputs text for an exact number match and stops further processing
    /// </summary>
    public class ExactMatchRule : RuleBase
    {
        private int TargetNumber { get; }
        private string Output { get; }

        public ExactMatchRule(int targetNumber, string output)
        {
            TargetNumber = targetNumber;
            Output = output;
        }

        public override string Evaluate(int number) => throw new System.NotImplementedException();

        // This rule stops processing when it matches
        public new bool? Final => true ;
    }
}
