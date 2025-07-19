namespace FizzBuzz.Engine
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

        public override RuleResult Evaluate(int number) => 
            number == TargetNumber 
                ? RuleResult.StopWith(Output) 
                : RuleResult.Empty;
    }
}
