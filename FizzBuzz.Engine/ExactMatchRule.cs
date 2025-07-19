namespace FizzBuzz.Engine
{
    /// <summary>
    /// A rule that outputs text for an exact number match and stops further processing
    /// </summary>
    public class ExactMatchRule : RuleBase
    {
        public int TargetNumber { get; }
        public string Output { get; }
        public override bool Final => true; // Always final - stops processing

        public ExactMatchRule(int targetNumber, string output, int priority) : base(priority)
        {
            TargetNumber = targetNumber;
            Output = output;
        }

        public override string Evaluate(int number) => 
            number == TargetNumber ? Output : string.Empty;
    }
}
