namespace FizzBuzz.Engine.Rules.Result
{
    /// <summary>
    /// Monadic operations for working with RuleResult collections
    /// These enable functional composition without imperative control flow
    /// </summary>
    public static class RuleResultExtensions
    {
        /// <summary>
        /// Maps a function over the output of Continue results, leaves Final unchanged
        /// This is the functor operation (fmap in Haskell)
        /// </summary>
        public static RuleResult Map(this RuleResult result, Func<string, string> f)
        {
            return result switch
            {
                RuleResult.Continue(var output) => RuleResult.ContinueWith(f(output)),
                RuleResult.Final(var output) => RuleResult.StopWith(output), // Final stops, unchanged
                _ => result
            };
        }

        /// <summary>
        /// Binds/FlatMaps a function that returns a RuleResult
        /// This is the monad bind operation (>>= in Haskell, flatMap in Scala)
        /// </summary>
        public static RuleResult Bind(this RuleResult result, Func<string, RuleResult> f)
        {
            return result switch
            {
                RuleResult.Continue(var output) => f(output),
                RuleResult.Final(var output) => RuleResult.StopWith(output), // Final short-circuits
                _ => result
            };
        }



        /// <summary>
        /// Takes results until (and including) the first Final result
        /// This enables short-circuit evaluation in a functional way
        /// </summary>
        public static IEnumerable<RuleResult> TakeUntilFinal(this IEnumerable<RuleResult> results)
        {
            foreach (var result in results)
            {
                yield return result;
                if (result is RuleResult.Final)
                    yield break; // Stop after Final
            }
        }

        /// <summary>
        /// Extracts outputs from Continue results only
        /// Final results are ignored (they represent a different control flow)
        /// </summary>
        public static IEnumerable<string> ExtractContinueOutputs(this IEnumerable<RuleResult> results)
        {
            return results
                .OfType<RuleResult.Continue>()
                .Select(c => c.Output);
        }

        /// <summary>
        /// Extracts the output from a Final result, or returns None if not Final
        /// </summary>
        public static string? ExtractFinalOutput(this IEnumerable<RuleResult> results)
        {
            var final = results.OfType<RuleResult.Final>().FirstOrDefault();
            return final?.Output;
        }

        /// <summary>
        /// Reduces/Folds Continue results into a single accumulated string
        /// This is the fold/reduce operation from functional programming
        /// </summary>
        public static string FoldOutputs(this IEnumerable<RuleResult> results, string seed = "")
        {
            return results
                .ExtractContinueOutputs()
                .Aggregate(seed, (acc, output) => acc + output);
        }

        /// <summary>
        /// Combines results: if any Final exists, return its output; 
        /// otherwise, concatenate all Continue outputs
        /// This is the main composition function for FizzBuzz evaluation
        /// </summary>
        public static string CombineResults(this IEnumerable<RuleResult> results, string fallback)
        {
            var resultsList = results.ToList();
            
            // Check for Final result - this is the "escape hatch" from the monad
            var finalOutput = resultsList.ExtractFinalOutput();
            if (finalOutput != null)
                return finalOutput;

            // Otherwise, fold all Continue results
            var accumulated = resultsList
                .TakeWhile(r => r is not RuleResult.Final)
                .FoldOutputs();

            return string.IsNullOrEmpty(accumulated) ? fallback : accumulated;
        }
    }
}
