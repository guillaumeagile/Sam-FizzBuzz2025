namespace FizzBuzz.Engine
{
    /// <summary>
    /// Default rule that returns the number as string - lowest priority fallback
    /// </summary>
    public class DefaultRule : RuleBase
    {
        public DefaultRule() : base(int.MaxValue) // Always last
        {
        }

        public override string Evaluate(int number) => number.ToString();
    }
}
