namespace FizzBuzz.Engine.Rules.Abstractions
{
    /// <summary>
    /// Base class that provides common functionality for rules
    /// Simplified for CUPID learning path
    /// </summary>
    public abstract class RuleBase : IRule
    {
        public abstract string Evaluate(int number);
        
        // Most rules continue processing, only specific rules (like ExactMatchRule) will stop
        public virtual bool Final => false;
    }
}
