namespace FizzBuzz.Engine.Rules.Result
{
    //TODO: test this as UnitOfWork

    /// <summary>
    /// Represents the result of a rule evaluation - either continue or stop processing
    /// This is our 'Either like' monad: Either<Continue, Final>
    /// </summary>
    public abstract record RuleResult
    {
        /// <summary>
        /// Continue processing with this output (may be empty)
        /// </summary>
        public record Continue(string Output) : RuleResult;
        
        /// <summary>
        /// Stop processing and return this final result
        /// </summary>
        public record Final(string Output) : RuleResult;

        /// <summary>
        /// Helper to create a Continue result
        /// </summary>
        public static RuleResult ContinueWith(string output) =>  throw new System.NotImplementedException();

        /// <summary>
        /// Helper to create a Final result
        /// </summary>
        public static RuleResult StopWith(string output) =>  throw new System.NotImplementedException();

        /// <summary>
        /// Helper for empty continue (no output, keep processing)
        /// </summary>
        public static RuleResult Empty => throw new System.NotImplementedException() ;
    }
}
