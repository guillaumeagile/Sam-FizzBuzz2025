namespace FizzBuzz.Engine
{
    /// <summary>
    /// Base class that eliminates duplication in rule implementations
    /// Focused only on common infrastructure, not domain logic
    /// </summary>
    public abstract class RuleBase : IRule
    {
        public virtual bool Final => false;

        public abstract string Evaluate(int number);
    }
}
