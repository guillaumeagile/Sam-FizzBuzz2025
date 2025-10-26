using FizzBuzz.Engine.Rules;
using FizzBuzz.Engine.Rules.Abstractions;
using FizzBuzz.Engine.Rules.Result;

namespace FizzBuzz.Engine
{
    /// <summary>
    /// CUPID-compliant FizzBuzz engine - predictable, composable, domain-focused
    /// Rules are processed in the order they are provided (no hidden priorities)
    /// </summary>
    public class CupidFizzBuzzEngine
    {
        private readonly IReadOnlyList<IRule> _rules;

        public CupidFizzBuzzEngine(IEnumerable<IRule> rules)
        {
            _rules = (rules ?? throw new ArgumentNullException(nameof(rules)))
                .ToList();
        }

        public IReadOnlyList<IRule> Rules => _rules;


        /// <summary>
        /// Pure functional evaluation using monadic operations
        /// No imperative control flow - just function composition
        /// </summary>
        public string Evaluate(int number)
        {
            // Pure functional pipeline:
            // 1. MAP: Evaluate all rules (lazy evaluation with LINQ)
            // 2. TakeUntilFinal: Short-circuit on Final (monadic)
            // 3. CombineResults: Fold Continue or extract Final (monadic)
            
            return _rules
                .Select(rule => rule.Evaluate(number))  // Map: Apply each rule
                .TakeUntilFinal()                       // Short-circuit: Stop at Final
                .CombineResults(fallback: number.ToString()); // Reduce: Combine or fallback
        }

        /// <summary>
        /// Creates a standard FizzBuzz engine
        /// </summary>
        public static CupidFizzBuzzEngine Standard() => 
            new(FizzBuzzRules.StandardGame());

        public static CupidFizzBuzzEngine Extended(List<IRule> extendedRules)
        {
            // Union removes duplicates automatically - functional and idiomatic
            var mergedRules = FizzBuzzRules.StandardGame().Union(extendedRules);
            return new(mergedRules);
        }

        public static CupidFizzBuzzEngine NewSet(List<IRule> extendedRules)
        {
            return new(extendedRules );
        }
    }
}
