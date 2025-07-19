namespace FizzBuzz.Engine
{
    /// <summary>
    /// Base class that eliminates duplication in rule implementations
    /// Focused only on common infrastructure, not domain logic
    /// </summary>
    public abstract class RuleBase : IRule
    {
        public int Priority { get; }
        public virtual bool Final => false;

        protected RuleBase(int priority)
        {
            Priority = priority;
        }

        public abstract string Evaluate(int number);

        public int CompareTo(IRule? other) => 
            other == null ? 1 : Priority.CompareTo(other.Priority);
    }
}
