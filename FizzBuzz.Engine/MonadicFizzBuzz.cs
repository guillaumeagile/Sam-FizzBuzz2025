namespace FizzBuzz.Engine
{
    /// <summary>
    /// Ultra-functional FizzBuzz with monadic operations and function composition
    /// This is as functional as it gets in C#!
    /// </summary>
    public static class MonadicFizzBuzz
    {
        /// <summary>
        /// Monadic bind operation for RuleResult
        /// </summary>
        public static RuleResult Bind(this RuleResult result, Func<string, RuleResult> func) =>
            result switch
            {
                RuleResult.Continue(var output) => func(output),
                RuleResult.Final(var output) => result, // Short-circuit on Final
                _ => result
            };

        /// <summary>
        /// Map operation for RuleResult
        /// </summary>
        public static RuleResult Map(this RuleResult result, Func<string, string> func) =>
            result switch
            {
                RuleResult.Continue(var output) => RuleResult.ContinueWith(func(output)),
                RuleResult.Final(var output) => RuleResult.StopWith(func(output)),
                _ => result
            };

        /// <summary>
        /// Combine two RuleResults
        /// </summary>
        public static RuleResult Combine(this RuleResult first, RuleResult second) =>
            first switch
            {
                RuleResult.Final => first, // Final always wins
                RuleResult.Continue(var output1) => second switch
                {
                    RuleResult.Final => second,
                    RuleResult.Continue(var output2) => RuleResult.ContinueWith(output1 + output2),
                    _ => first
                },
                _ => second
            };

        /// <summary>
        /// Functional rule composition using monadic operations
        /// </summary>
        public static Func<int, string> ComposeRules(params FunctionalFizzBuzz.Rule[] rules) =>
            number => rules
                .Select(rule => rule(number))
                .Aggregate(RuleResult.Empty, (acc, result) => acc.Combine(result))
                .Match(
                    onContinue: output => string.IsNullOrEmpty(output) ? number.ToString() : output,
                    onFinal: output => output
                );

        /// <summary>
        /// Pattern matching for RuleResult (visitor pattern)
        /// </summary>
        public static T Match<T>(this RuleResult result, Func<string, T> onContinue, Func<string, T> onFinal) =>
            result switch
            {
                RuleResult.Continue(var output) => onContinue(output),
                RuleResult.Final(var output) => onFinal(output),
                _ => throw new ArgumentException("Unknown RuleResult type")
            };

        /// <summary>
        /// Applicative functor - apply a function wrapped in RuleResult
        /// </summary>
        public static RuleResult Apply(this RuleResult funcResult, RuleResult valueResult) =>
            (funcResult, valueResult) switch
            {
                (RuleResult.Final, _) => funcResult,
                (_, RuleResult.Final) => valueResult,
                (RuleResult.Continue(var func), RuleResult.Continue(var value)) => 
                    RuleResult.ContinueWith(func + value),
                _ => RuleResult.Empty
            };

        /// <summary>
        /// Kleisli composition - compose monadic functions
        /// </summary>
        public static Func<int, RuleResult> Compose(
            this Func<int, RuleResult> f, 
            Func<string, RuleResult> g) =>
            input => f(input).Bind(g);

        /// <summary>
        /// Ultra-functional FizzBuzz using pure function composition
        /// </summary>
        public static class Pure
        {
            // Point-free style functions
            public static readonly Func<int, bool> IsDivisibleBy3 = n => n % 3 == 0;
            public static readonly Func<int, bool> IsDivisibleBy5 = n => n % 5 == 0;
            public static readonly Func<int, bool> IsDivisibleBy7 = n => n % 7 == 0;
            public static readonly Func<int, bool> IsTheAnswer = n => n == 42;

            // Conditional output functions
            public static readonly Func<int, string> Fizz = n => IsDivisibleBy3(n) ? "Fizz" : "";
            public static readonly Func<int, string> Buzz = n => IsDivisibleBy5(n) ? "Buzz" : "";
            public static readonly Func<int, string> Bang = n => IsDivisibleBy7(n) ? "Bang" : "";
            public static readonly Func<int, string> Answer = n => IsTheAnswer(n) ? "The answer to the meaning of life, the universe, and everything" : "";

            // Function composition
            public static readonly Func<int, string> FizzBuzz = n =>
            {
                var answer = Answer(n);
                if (!string.IsNullOrEmpty(answer)) return answer; // Early termination
                //           Fizz(n) |> Buzz(n) |> Bang(n)
                var result = Fizz(n) + Buzz(n) + Bang(n);
                return string.IsNullOrEmpty(result) ? n.ToString() : result;
            };

            // Pipeline composition using LINQ
            public static readonly Func<int, string> PipelineFizzBuzz = n =>
                new[] { Fizz, Buzz, Bang }
                    .Select(f => f(n))
                    .Where(s => !string.IsNullOrEmpty(s))
                    .DefaultIfEmpty(n.ToString())
                    .Aggregate((a, b) => a + b);

            // Curried functions for partial application
            public static Func<string, Func<int, string>> DivisibleBy(int divisor) =>
                output => number => number % divisor == 0 ? output : "";

            // Partially applied functions
            public static readonly Func<int, string> CurriedFizz = DivisibleBy(3)("Fizz");
            public static readonly Func<int, string> CurriedBuzz = DivisibleBy(5)("Buzz");
        }
    }
}
