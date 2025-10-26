using FizzBuzz.Engine.Rules.Result;
using FluentAssertions;

namespace FizzBuzz.Tests;

/// <summary>
/// Tests demonstrating monadic operations on RuleResult
/// These show how functional programming enables cleaner composition
/// </summary>
[TestFixture]
public class RuleResultMonadicTests
{
    #region Map (Functor) Tests

    [Test]
    public void Map_OnContinue_TransformsOutput()
    {
        // Arrange
        var result = RuleResult.ContinueWith("fizz");

        // Act - Map transforms the output
        var mapped = result.Map(s => s.ToUpper());

        // Assert
        mapped.Should().BeOfType<RuleResult.Continue>();
        ((RuleResult.Continue)mapped).Output.Should().Be("FIZZ");
    }

    [Test]
    public void Map_CanChainMultipleTransformations()
    {
        // Arrange
        var result = RuleResult.ContinueWith("fizz");

        // Act - Chain multiple Maps (functor composition)
        var transformed = result
            .Map(s => s.ToUpper())
            .Map(s => $"[{s}]")
            .Map(s => s + "!");

        // Assert
        ((RuleResult.Continue)transformed).Output.Should().Be("[FIZZ]!");
    }

    #endregion

    #region Bind (Monad) Tests

    [Test]
    public void Bind_OnContinue_AppliesFunction()
    {
        // Arrange
        var result = RuleResult.ContinueWith("fizz");

        // Act - Bind lets us return a new RuleResult
        var bound = result.Bind(output => 
            output == "fizz" 
                ? RuleResult.StopWith("FINAL FIZZ") 
                : RuleResult.ContinueWith(output));

        // Assert
        bound.Should().BeOfType<RuleResult.Final>();
        ((RuleResult.Final)bound).Output.Should().Be("FINAL FIZZ");
    }

    [Test]
    public void Bind_OnFinal_ShortCircuits()
    {
        // Arrange
        var result = RuleResult.StopWith("42");
        var wasExecuted = false;

        // Act - Bind on Final should NOT execute the function
        var bound = result.Bind(output =>
        {
            wasExecuted = true;
            return RuleResult.ContinueWith("SHOULD NOT SEE THIS");
        });

        // Assert - Function was not executed, Final unchanged
        wasExecuted.Should().BeFalse();
        bound.Should().BeOfType<RuleResult.Final>();
        ((RuleResult.Final)bound).Output.Should().Be("42");
    }

    [Test]
    public void Bind_CanConvertContinueToFinal()
    {
        // Arrange
        var result = RuleResult.ContinueWith("special");

        // Act - Bind can change the result type
        var bound = result.Bind(output =>
            output == "special"
                ? RuleResult.StopWith("STOPPED")
                : RuleResult.Empty);

        // Assert
        bound.Should().BeOfType<RuleResult.Final>();
    }

    #endregion

    #region Collection Operations Tests

    [Test]
    public void TakeUntilFinal_StopsAtFirstFinal()
    {
        // Arrange
        var results = new[]
        {
            RuleResult.ContinueWith("Fizz"),
            RuleResult.ContinueWith("Buzz"),
            RuleResult.StopWith("42"),
            RuleResult.ContinueWith("Should not see this"),
        };

        // Act
        var taken = results.TakeUntilFinal().ToList();

        // Assert - Should have 3 items (including Final, but not after)
        taken.Should().HaveCount(3);
        taken[0].Should().BeOfType<RuleResult.Continue>();
        taken[1].Should().BeOfType<RuleResult.Continue>();
        taken[2].Should().BeOfType<RuleResult.Final>();
    }

    [Test]
    public void TakeUntilFinal_WithoutFinal_TakesAll()
    {
        // Arrange
        var results = new[]
        {
            RuleResult.ContinueWith("Fizz"),
            RuleResult.ContinueWith("Buzz"),
            RuleResult.ContinueWith("Bang"),
        };

        // Act
        var taken = results.TakeUntilFinal().ToList();

        // Assert - All 3 items taken
        taken.Should().HaveCount(3);
        taken.Should().AllBeOfType<RuleResult.Continue>();
    }

    [Test]
    public void ExtractContinueOutputs_FiltersOnlyContinue()
    {
        // Arrange
        var results = new[]
        {
            RuleResult.ContinueWith("Fizz"),
            RuleResult.StopWith("42"),
            RuleResult.ContinueWith("Buzz"),
        };

        // Act
        var outputs = results.ExtractContinueOutputs().ToList();

        // Assert - Only Continue outputs
        outputs.Should().Equal("Fizz", "Buzz");
    }

    [Test]
    public void ExtractFinalOutput_ReturnsFirstFinal()
    {
        // Arrange
        var results = new[]
        {
            RuleResult.ContinueWith("Fizz"),
            RuleResult.StopWith("42"),
            RuleResult.StopWith("Should not see"),
        };

        // Act
        var finalOutput = results.ExtractFinalOutput();

        // Assert
        finalOutput.Should().Be("42");
    }

    [Test]
    public void ExtractFinalOutput_WithNoFinal_ReturnsNull()
    {
        // Arrange
        var results = new[]
        {
            RuleResult.ContinueWith("Fizz"),
            RuleResult.ContinueWith("Buzz"),
        };

        // Act
        var finalOutput = results.ExtractFinalOutput();

        // Assert
        finalOutput.Should().BeNull();
    }

    #endregion

    #region FoldOutputs Tests

    [Test]
    public void FoldOutputs_ConcatenatesAllContinue()
    {
        // Arrange
        var results = new[]
        {
            RuleResult.ContinueWith("Fizz"),
            RuleResult.ContinueWith("Buzz"),
            RuleResult.ContinueWith("Bang"),
        };

        // Act
        var folded = results.FoldOutputs();

        // Assert
        folded.Should().Be("FizzBuzzBang");
    }

    [Test]
    public void FoldOutputs_WithSeed_StartsWithSeed()
    {
        // Arrange
        var results = new[]
        {
            RuleResult.ContinueWith("Fizz"),
            RuleResult.ContinueWith("Buzz"),
        };

        // Act
        var folded = results.FoldOutputs(seed: "Start:");

        // Assert
        folded.Should().Be("Start:FizzBuzz");
    }

    [Test]
    public void FoldOutputs_IgnoresFinal()
    {
        // Arrange
        var results = new[]
        {
            RuleResult.ContinueWith("Fizz"),
            RuleResult.StopWith("42"),
            RuleResult.ContinueWith("Buzz"),
        };

        // Act - Fold only processes Continue
        var folded = results.FoldOutputs();

        // Assert - Final is ignored in fold
        folded.Should().Be("FizzBuzz");
    }

    [Test]
    public void FoldOutputs_WithEmptyResults_ReturnsSeed()
    {
        // Arrange
        var results = Array.Empty<RuleResult>();

        // Act
        var folded = results.FoldOutputs(seed: "default");

        // Assert
        folded.Should().Be("default");
    }

    #endregion

    #region CombineResults Tests

    [Test]
    public void CombineResults_WithFinal_ReturnsFinalOutput()
    {
        // Arrange
        var results = new[]
        {
            RuleResult.ContinueWith("Fizz"),
            RuleResult.StopWith("42"),
            RuleResult.ContinueWith("Should not see"),
        };

        // Act
        var combined = results.CombineResults(fallback: "fallback");

        // Assert - Final takes precedence
        combined.Should().Be("42");
    }

    [Test]
    public void CombineResults_WithoutFinal_ConcatenatesContinue()
    {
        // Arrange
        var results = new[]
        {
            RuleResult.ContinueWith("Fizz"),
            RuleResult.ContinueWith("Buzz"),
            RuleResult.ContinueWith("Bang"),
        };

        // Act
        var combined = results.CombineResults(fallback: "fallback");

        // Assert
        combined.Should().Be("FizzBuzzBang");
    }

    [Test]
    public void CombineResults_WithNoOutput_ReturnsFallback()
    {
        // Arrange
        var results = new[]
        {
            RuleResult.Empty,
            RuleResult.Empty,
        };

        // Act
        var combined = results.CombineResults(fallback: "7");

        // Assert
        combined.Should().Be("7");
    }



    #endregion

    #region Integration Tests - Real FizzBuzz Scenarios

    [Test]
    public void RealScenario_FizzBuzz_Number15()
    {
        // Arrange - Simulating rules for number 15 (divisible by 3 and 5)
        var results = new[]
        {
            RuleResult.ContinueWith("Fizz"),  // 15 % 3 == 0
            RuleResult.ContinueWith("Buzz"),  // 15 % 5 == 0
        };

        // Act
        var output = results.CombineResults(fallback: "15");

        // Assert
        output.Should().Be("FizzBuzz");
    }

    [Test]
    public void RealScenario_ExactMatch_Number42()
    {
        // Arrange - Simulating ExactMatchRule for 42
        var results = new[]
        {
            RuleResult.ContinueWith("Fizz"),     // Would match, but...
            RuleResult.StopWith("The answer"),   // ExactMatch stops everything
            RuleResult.ContinueWith("Buzz"),     // Never evaluated
        };

        // Act
        var output = results.CombineResults(fallback: "42");

        // Assert - Final short-circuits
        output.Should().Be("The answer");
    }

    [Test]
    public void RealScenario_NoMatches_Number7()
    {
        // Arrange - Number 7 with only Fizz(3) and Buzz(5) rules
        var results = new[]
        {
            RuleResult.Empty,  // 7 % 3 != 0
            RuleResult.Empty,  // 7 % 5 != 0
        };

        // Act
        var output = results.CombineResults(fallback: "7");

        // Assert - Falls back to number
        output.Should().Be("7");
    }

    [Test]
    public void RealScenario_FunctionalComposition_Demonstrates_Monadic_Power()
    {
        // Arrange - Show how monadic operations compose
        var results = new[]
        {
            RuleResult.ContinueWith("Fizz"),
            RuleResult.ContinueWith("Buzz"),
        };

        // Act - Chain operations functionally
        var output = results
            .TakeUntilFinal()              // Short-circuit on Final
            .Where(r => r is not RuleResult.Final)  // Filter out Final for now
            .Select(r => r.Map(s => s.ToLower()))   // Map over each result
            .FoldOutputs();                         // Reduce to string

        // Assert
        output.Should().Be("fizzbuzz");
    }

    #endregion
}
