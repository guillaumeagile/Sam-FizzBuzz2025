using FizzBuzz.Engine.Rules;
using FizzBuzz.Engine.Rules.Abstractions;

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
        /// Old-fashioned if/else pattern for learning purposes
        /// </summary>
        public string Evaluate(int number)
        {
            var result = string.Empty;
            
            foreach (var rule in _rules)
            {
                var output = rule.Evaluate(number);
                
                // If the rule produced output, append it
                if (!string.IsNullOrEmpty(output))
                {
                    result = result + output;
                    
                    // If this is a final rule, stop processing and return immediately
                    if (rule.Final)
                    {
                        return result;
                    }
                }
            }
            
            // If no rules produced output, return the number as string
            if (string.IsNullOrEmpty(result))
            {
                return number.ToString();
            }
            else
            {
                return result;
            }
        }




        public static CupidFizzBuzzEngine NewSet(List<IRule> extendedRules)
        {
            return new(extendedRules );
        }
    }
}
