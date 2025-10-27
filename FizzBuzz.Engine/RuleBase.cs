namespace FizzBuzz.Engine
{
    /// <summary>
    /// Base class that provides common functionality for rules
    /// Now much simpler with the Either monad pattern
    /// </summary>
    public abstract class RuleBase : IRule
    {
        public abstract RuleResult Evaluate(int number);
    }
}
