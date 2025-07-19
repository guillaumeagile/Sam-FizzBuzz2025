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
        /// </summary>
        public string Evaluate(int number)
        {
            var result = string.Empty;
            
            foreach (var rule in _rules)
            {
                var output = rule.Evaluate(number);
                
                if (!string.IsNullOrEmpty(output))
                {
                    if (rule.Final)
                        return output; // Final rule stops processing
                    
                    result += output;
                }
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
