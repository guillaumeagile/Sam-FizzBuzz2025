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
        /// Pure functional evaluation using Railway Oriented Programming
        /// Based on Scott Wlaschin's railway pattern
        /// 
        /// Railway metaphor:
        /// - Success track (Continue): Keep evaluating rules, accumulate output
        /// - Final track (Terminal): Stop immediately with final output
        /// 
        /// No imperative control flow - just function composition
        /// </summary>
        public string Evaluate(int number)
        {
            // Railway-Oriented Programming pipeline:
            // 1. MAP: Evaluate all rules → creates RuleResult collection
            // 2. FollowTrainUntilTerminal: Process until Final (short-circuit)
            // 3. CompleteJourney: Handle both tracks (Final or accumulated Continue)
            
            return _rules
                .Select(rule => rule.Evaluate(number))      // Map each rule
                .FollowTrainUntilTerminal()                 // Stop at terminal station
                .CompleteJourney(defaultDestination: number.ToString()); // Final or accumulated
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
