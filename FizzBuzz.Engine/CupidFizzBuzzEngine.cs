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
        /// Functional version using filter/map/reduce pattern
        /// Demonstrates pure functional programming approach
        /// </summary>
        public string Evaluate(int number)
        {
            // Step 1: COLLECT - Evaluate all rules and collect results
            var results = _rules
                .Select(rule => rule.Evaluate(number))
                .ToList();

            // Step 2: TAKE UNTIL Final - Stop collecting when we hit a Final result
            var resultsUntilFinal = results
                .TakeWhile(r => r is not RuleResult.Final)
                .ToList();

            // Check if we found a Final result
            var finalResult = results
                .FirstOrDefault(r => r is RuleResult.Final);

            //  "FILTER-LIKE" If we have a Final result, return only that
            if (finalResult is RuleResult.Final final)
                return final.Output;

            // Step 3: MAP to Output - Keep only Continue results
            var outputs = resultsUntilFinal
                .OfType<RuleResult.Continue>()
                .Select(c => c.Output);

            // Step 4: REDUCE - Aggregate all outputs into single string
            var result = outputs.Aggregate(
                seed: string.Empty,
                func: (acc, output) => acc + output);

            // Fallback to number if no output produced
            return string.IsNullOrEmpty(result) ? number.ToString() : result;
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
