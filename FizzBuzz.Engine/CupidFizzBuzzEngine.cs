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

        /// <summary>
        /// Evaluates a number against all rules and returns the result
        /// Rules are processed in the order they were added
        /// Uses modern pattern matching on the Either monad (RuleResult)
        /// </summary>
        public string Evaluate(int number)
        {
            var result = string.Empty;
            
            foreach (var rule in _rules)
            {
                var ruleResult = rule.Evaluate(number);
                
                // Modern pattern matching - much cleaner!
                result = ruleResult switch
                {
                    RuleResult.Final(var output) => output, // Stop and return immediately
                    RuleResult.Continue(var output) when !string.IsNullOrEmpty(output) => result + output,
                    RuleResult.Continue => result, // Empty output, continue
                    _ => result // Should never happen with sealed record hierarchy
                    //but the compiler needs it to be exhaustive
                };

                // If we got a Final result, return immediately
                if (ruleResult is RuleResult.Final)
                    return result;
            }
            
            // Only return number.ToString() if no rules produced output
            return string.IsNullOrEmpty(result) ? number.ToString() : result;
        }

        /// <summary>
        /// Creates a standard FizzBuzz engine
        /// </summary>
        public static CupidFizzBuzzEngine Standard() => 
            new(FizzBuzzRules.StandardGame());
    }
}
