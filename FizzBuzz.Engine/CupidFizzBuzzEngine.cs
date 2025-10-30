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
        public CupidFizzBuzzEngine(IEnumerable<IRule> rules)
        {
            Rules = (rules ?? throw new ArgumentNullException(nameof(rules)))
                .ToList();
        }

        public IReadOnlyList<IRule> Rules { get; }

        public string Evaluate(int number)
        {
            var result = string.Empty;

            foreach (var rule in Rules)
            {
                var ruleResult = rule.Evaluate(number);
                // Legacy, OOP-style type checking
                if (ruleResult is RuleResult.Final final)
                {  // Stop immediately on final
                    return final.Output;
                }

                if (ruleResult is RuleResult.Continue cont && !string.IsNullOrEmpty(cont.Output))
                {  // Accumulate non-empty outputs
                    result += cont.Output;
                }
            }

            // Only return number.ToString() if no rules produced output
            return string.IsNullOrEmpty(result) ? number.ToString() : result;
        }
        // ## Which SOLID rule is violated and why
        // ### Open/Closed Principle (OCP)  — primary violation
        // The engine ( CupidFizzBuzzEngine.Evaluate ) is a high-level module that should depend on abstractions only.

        // By type-checking against concrete nested classes
        // RuleResult.Final
        //  and
        // RuleResult.Continue
        // , it depends on low-level details of the result representation.
        // Any internal representation change of the result leaks into the engine, coupling them tightly.

        // ### Why it can drift into LSP problems

        // If the abstraction’s contract is underspecified and clients assume concrete shapes:
        // Example: you later introduce a new RuleResult subtype (e.g., Error) without changing the IRule/
        // RuleResult contract that callers rely on.
        // Then existing clients (like your engine) that only handle
        // Continue / Final   will misbehave.

        // This is primarily an OCP problem, but it also manifests as an LSP break
        // because a valid “subtype” of the abstract result can no longer be substituted without breaking clients’ expectations.





/*
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
                // it's here where the engine decides to stop (on Final) or to continue when the result is not empty
                // we use  pattern matching with deconstruction here, because we want to keep the result of the previous rule
                result = ruleResult switch
                {
                    RuleResult.Final(var output) => output, // we keep the output, because after that we exit immediately

                    RuleResult.Continue(var output) when !string.IsNullOrEmpty(output) => result + output,

                    RuleResult.Continue => result, // Empty output, just continue with the current result

                    _ => result // Should never happen with sealed record hierarchy
                    //but the compiler needs it to be exhaustive (C#14 is not fully FP language)
                };

                // If we got a Final result, we exit immediately
                if (ruleResult is RuleResult.Final)
                    return result;
            }

            // Only return number.ToString() if no rules produced output
            return string.IsNullOrEmpty(result) ? number.ToString() : result;
        }
        */

        /*  Why pattern matching shines with records (and deconstruction)

           Immutable, data-first shape
           Records are immutable by default and model “just data.”
           Pattern matching works best on stable shapes; no side effects, no mutation surprises.

           Positional records synthesize a Deconstruct(out …) automatically.

           => uses the positional “deconstruct” to bind output.

           With legacy classes, you don’t get auto-deconstruction;
                     => you’d need to add Deconstruct manually or write more verbose property access.

           Now, you can write Concise and expressive switch expressions

         */





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
