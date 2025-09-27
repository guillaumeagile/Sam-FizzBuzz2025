using System.Collections.Immutable;

namespace FizzBuzz.Engine
{
    /// <summary>
    /// Factory for creating common FizzBuzz rules using domain language
    /// </summary>
    public static class FizzBuzzRules
    {
        public static DivisibilityRule Fizz() => new(3, "Fizz");
        public static DivisibilityRule Buzz() => new(5, "Buzz");
        public static DivisibilityRule Bang() => new(7, "Bang");
        
        public static ExactMatchRule TheAnswer() => 
            new(42, "The answer to the meaning of life, the universe, and everything");
        
        public static DefaultRule Default() => new();

        /// <summary>
        /// Creates a standard FizzBuzz game with Fizz, Buzz, Bang, and TheAnswer rules
        /// </summary>
        public static ImmutableSortedSet<IRule> StandardGame() =>
            ImmutableSortedSet.Create(
                Fizz() as IRule,
                Buzz()
            );



        /// <summary>
        /// Creates a custom divisibility rule
        /// </summary>
        public static DivisibilityRule Divisible(int by, string output) =>
            new(by, output);
    }
}
