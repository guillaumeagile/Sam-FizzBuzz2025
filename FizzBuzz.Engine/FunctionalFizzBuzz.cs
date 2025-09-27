namespace FizzBuzz.Engine
{
    /// <summary>
    /// Pure functional FizzBuzz implementation
    /// No classes, no state, just pure functions and composition
    /// </summary>
    public static class FunctionalFizzBuzz
    {
        /// <summary>
        /// A rule is just a function: int -> RuleResult
        /// </summary>
        public delegate RuleResult Rule(int number);

        /// <summary>
        /// Creates a divisibility rule using a closure
        /// </summary>
        public static Rule Divisible(int divisor, string output) =>
            number => number % divisor == 0 
                ? RuleResult.ContinueWith(output) 
                : RuleResult.Empty;

        /// <summary>
        /// Creates an exact match rule using a closure
        /// </summary>
        public static Rule ExactMatch(int target, string output) =>
            number => number == target 
                ? RuleResult.StopWith(output) 
                : RuleResult.Empty;

        /// <summary>
        /// Standard FizzBuzz rules as pure functions
        /// </summary>
        public static class Rules
        {
            public static readonly Rule Fizz = Divisible(3, "Fizz");
            public static readonly Rule Buzz = Divisible(5, "Buzz");
            public static readonly Rule Bang = Divisible(7, "Bang");
            public static readonly Rule TheAnswer = ExactMatch(42, "The answer to the meaning of life, the universe, and everything");
        }

        /// <summary>
        /// Evaluates a number against a sequence of rules using functional composition
        /// Note: This version doesn't support early termination for Final results
        /// Use EvaluateWithEarlyTermination for that behavior
        /// </summary>
        public static string Evaluate(int number, params Rule[] rules) =>
            EvaluateWithEarlyTermination(number, rules);

        /// <summary>
        /// Alternative: Early termination version using functional approach
        /// </summary>
        public static string EvaluateWithEarlyTermination(int number, params Rule[] rules)
        {
            var result = string.Empty;
            
            foreach (var rule in rules)
            {
                var ruleResult = rule(number);
                result = ruleResult switch
                {
                    RuleResult.Final(var output) => output,
                    RuleResult.Continue(var output) when !string.IsNullOrEmpty(output) => result + output,
                    RuleResult.Continue => result,
                    _ => result
                };

                if (ruleResult is RuleResult.Final)
                    return result;
            }

            return string.IsNullOrEmpty(result) ? number.ToString() : result;
        }

        /// <summary>
        /// Curried version for partial application
        /// </summary>
        public static Func<int, string> CreateEvaluator(params Rule[] rules) =>
            number => EvaluateWithEarlyTermination(number, rules);

        /// <summary>
        /// Standard FizzBuzz evaluator
        /// </summary>
        public static readonly Func<int, string> StandardFizzBuzz = 
            CreateEvaluator(Rules.Fizz, Rules.Buzz);

        public static readonly Func<int, string> ExtendedFizzBuzz =
            CreateEvaluator(Rules.Fizz, Rules.Buzz, Rules.Bang, Rules.TheAnswer);

        /*  version F# like
      fizzBuw=
            treatFizzBuzz
            -> orElse
            treatFizz
                -> treatFuzz
*/


        /// <summary>
        /// Functional pipeline for processing ranges
        /// </summary>
        public static IEnumerable<string> ProcessRange(int start, int count, Func<int, string> evaluator) =>
            Enumerable.Range(start, count).Select(evaluator);
    }
}
