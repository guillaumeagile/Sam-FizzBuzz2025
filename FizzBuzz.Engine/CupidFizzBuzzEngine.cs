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
        /// Evaluates a number against all rules and returns the result
        /// Rules are processed in the order they were added
        /// Uses modern pattern matching on the Either monad (RuleResult)
        /// </summary>
        public string Evaluate(int number)
        {
            var printableResult = string.Empty;
            
            foreach (var rule in _rules)
            {
                var ruleResult = rule.Evaluate(number);
                
                // Modern pattern matching - much cleaner!
                // it's here where the engine decides to stop (on Final) or to continue when the result is not empty
                // we use  pattern matching with deconstruction here, because we want to keep the result of the previous rule
                printableResult = ruleResult switch
                {
                    RuleResult.Final(var output) => output, // we keep the output, because after that we exit immediately

                    RuleResult.Continue(var output) when !string.IsNullOrEmpty(output) => printableResult + output,

                    RuleResult.Continue => printableResult, // Empty output, just continue with the current result

                    _ => printableResult // Should never happen with sealed record hierarchy
                    //but the compiler needs it to be exhaustive (C#14 is not fully FP language)
                };

                // If we got a Final result, we exit immediately
                if (ruleResult is RuleResult.Final)
                    return printableResult;
            }
            
            // Only return number.ToString() if no rules produced output
            return string.IsNullOrEmpty(printableResult) ? number.ToString() : printableResult;
        }




        public static CupidFizzBuzzEngine NewSet(List<IRule> extendedRules)
        {
            return new(extendedRules );
        }
    }
}
