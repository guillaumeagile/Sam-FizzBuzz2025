namespace FizzBuzz.Engine
{
    /// <summary>
    /// A rule that outputs text for an exact number match and stops further processing
    /// Using record for value-based equality (functional programming)
    /// </summary>
    public record ExactMatchRule(int TargetNumber, string Output) : RuleBase
    {
        public override RuleResult Evaluate(int number) => 
            number == TargetNumber 
                ? RuleResult.StopWith(Output) 
                : RuleResult.Empty;
    }
}
