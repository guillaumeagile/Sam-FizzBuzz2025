namespace FizzBuzz.Engine.Rules.Result
{
    /// <summary>
    /// Railway Oriented Programming (ROP) extensions for RuleResult
    /// Based on Scott Wlaschin's F# railway pattern
    /// 
    /// Two tracks:
    /// - Success track: Continue with accumulated output
    /// - Final track: Stop with final output (short-circuit)
    /// 
    /// See: https://fsharpforfunandprofit.com/rop/
    /// </summary>
    public static class RailwayExtensions
    {
        // ========================================
        // CORE RAILWAY FUNCTIONS
        // ========================================

        /// <summary>
        /// Bind: Chain functions that return RuleResult
        /// If on Success track (Continue), apply function
        /// If on Final track, bypass (stay on Final track)
        /// 
        /// Railway analogy: Switch points that can divert to Final track
        /// </summary>
        public static RuleResult Bind(this RuleResult input, Func<string, RuleResult> switchFunction)
        {
            return input switch
            {
                RuleResult.Continue(var output) => switchFunction(output),
                RuleResult.Final(var output) => input, // Stay on Final track
                _ => input
            };
        }

        /// <summary>
        /// Map: Transform value on Success track
        /// If on Final track, bypass
        /// 
        /// Railway analogy: Transform the payload while staying on same track
        /// </summary>
        public static RuleResult Map(this RuleResult input, Func<string, string> transform)
        {
            return input switch
            {
                RuleResult.Continue(var output) => RuleResult.ContinueWith(transform(output)),
                RuleResult.Final(var output) => input, // Stay on Final track
                _ => input
            };
        }

        /// <summary>
        /// Tee: Execute a side effect without changing the result
        /// Useful for logging, debugging
        /// 
        /// Railway analogy: Observation point - look but don't touch
        /// </summary>
        public static RuleResult Tee(this RuleResult input, Action<string> sideEffect)
        {
            if (input is RuleResult.Continue cont)
                sideEffect(cont.Output);
            
            return input;
        }

        /// <summary>
        /// Switch: Convert a normal function into a railway function
        /// Takes a function that returns plain value, wraps result in Continue
        /// 
        /// Railway analogy: Put a value onto the Success track
        /// </summary>
        public static Func<string, RuleResult> Switch(Func<string, string> normalFunction)
        {
            return input => RuleResult.ContinueWith(normalFunction(input));
        }

        /// <summary>
        /// DoubleMap: Map over both tracks (Continue and Final)
        /// Rare, but useful when you need to transform both cases
        /// </summary>
        public static RuleResult DoubleMap(
            this RuleResult input, 
            Func<string, string> continueTransform,
            Func<string, string> finalTransform)
        {
            return input switch
            {
                RuleResult.Continue(var output) => RuleResult.ContinueWith(continueTransform(output)),
                RuleResult.Final(var output) => RuleResult.StopWith(finalTransform(output)),
                _ => input
            };
        }

        // ========================================
        // RAILWAY COMBINATORS FOR COLLECTIONS
        // ========================================

        /// <summary>
        /// Collect results until one switches to Final track
        /// Railway analogy: Follow the train until it reaches a terminal station
        /// </summary>
        public static IEnumerable<RuleResult> FollowTrainUntilTerminal(this IEnumerable<RuleResult> results)
        {
            foreach (var result in results)
            {
                yield return result;
                if (result is RuleResult.Final)
                    yield break; // Reached terminal - stop the train
            }
        }

        /// <summary>
        /// Reduce all Continue results into single accumulated value
        /// Railway analogy: Collect all cargo from Success track
        /// </summary>
        private static string CollectSuccessOutputs(this IEnumerable<RuleResult> results, string seed = "")
        {
            return results
                .OfType<RuleResult.Continue>()
                .Select(c => c.Output)
                .Aggregate(seed, (acc, output) => acc + output);
        }

        /// <summary>
        /// Get the final output if train reached terminal, otherwise null
        /// Railway analogy: Check if train reached terminal station
        /// </summary>
        private static string? GetTerminalOutput(this IEnumerable<RuleResult> results)
        {
            var terminal = results.OfType<RuleResult.Final>().FirstOrDefault();
            return terminal?.Output;
        }

        /// <summary>
        /// Main railway composition: Follow tracks, handle both outcomes
        /// Railway analogy: Complete journey - either reach terminal or collect all stops
        /// </summary>
        public static string CompleteJourney(this IEnumerable<RuleResult> results, string defaultDestination)
        {
            var resultsList = results.ToList();
            
            // Did we reach a terminal station?
            var terminal = resultsList.GetTerminalOutput();
            if (terminal != null)
                return terminal;

            // Otherwise, collect all outputs from Success track
            var collected = resultsList
                .TakeWhile(r => r is not RuleResult.Final)
                .CollectSuccessOutputs();

            return string.IsNullOrEmpty(collected) ? defaultDestination : collected;
        }

        // ========================================
        // RAILWAY COMPOSITION HELPERS
        // ========================================



        /// <summary>
        /// Try/Catch adapter: Convert exceptions into Final track
        /// Railway analogy: Derailment handler - catches crashes and switches to Final
        /// </summary>
        public static RuleResult Try(Func<RuleResult> dangerousFunc)
        {
            try
            {
                return dangerousFunc();
            }
            catch (Exception ex)
            {
                return RuleResult.StopWith($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply a list of railway functions in sequence
        /// Railway analogy: Journey through multiple stations
        /// </summary>
        public static RuleResult ThroughStations(
            this RuleResult initial, 
            params Func<string, RuleResult>[] stations)
        {
            var current = initial;
            foreach (var station in stations)
            {
                current = current.Bind(station);
                // If we hit Final track, stop (short-circuit)
                if (current is RuleResult.Final)
                    break;
            }
            return current;
        }

        // ========================================
        // PATTERN MATCHING (Railway Style)
        // ========================================

        /// <summary>
        /// Match: Pattern match on both tracks with explicit handlers
        /// Railway analogy: Different destinations need different handling
        /// </summary>
        public static T Match<T>(
            this RuleResult result,
            Func<string, T> onContinue,
            Func<string, T> onFinal)
        {
            return result switch
            {
                RuleResult.Continue(var output) => onContinue(output),
                RuleResult.Final(var output) => onFinal(output),
                _ => throw new InvalidOperationException("Unknown RuleResult type")
            };
        }

        /// <summary>
        /// Either: Execute side effects based on which track we're on
        /// Railway analogy: Signal different events for Success vs Terminal
        /// </summary>
        public static RuleResult Either(
            this RuleResult result,
            Action<string> onContinue,
            Action<string> onFinal)
        {
            switch (result)
            {
                case RuleResult.Continue cont:
                    onContinue(cont.Output);
                    break;
                case RuleResult.Final final:
                    onFinal(final.Output);
                    break;
            }
            return result;
        }
    }
}
