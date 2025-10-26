using FizzBuzz.Engine.Rules.Result;
using FluentAssertions;

namespace FizzBuzz.Tests;

/// <summary>
/// Tests demonstrating Railway Oriented Programming (ROP)
/// Based on Scott Wlaschin's F# railway pattern
/// 
/// The Railway Metaphor:
/// - Success Track (Continue): Processing continues, accumulating results
/// - Final Track (Terminal): Processing stops, returns final result
/// 
/// See: https://fsharpforfunandprofit.com/rop/
/// </summary>
[TestFixture]
public class RailwayOrientedProgrammingTests
{
    #region Railway Basics: Two-Track System

    [Test]
    public void Railway_SuccessTrack_ContinuesProcessing()
    {
        // Arrange - Start on Success track
        var train = RuleResult.ContinueWith("Fizz");

        // Act - Stay on Success track through transformations
        var result = train
            .Map(s => s.ToUpper())      // Still on Success track
            .Map(s => s + "Buzz")        // Still on Success track
            .Bind(s => RuleResult.ContinueWith(s + "!"));  // Still on Success track

        // Assert
        result.Should().BeOfType<RuleResult.Continue>();
        ((RuleResult.Continue)result).Output.Should().Be("FIZZBuzz!");
    }

    [Test]
    public void Railway_SwitchToFinalTrack_StopsProcessing()
    {
        // Arrange - Start on Success track
        var train = RuleResult.ContinueWith("Fizz");

        // Act - Switch to Final track, then operations are bypassed
        var result = train
            .Map(s => s.ToUpper())                      // Applied: "FIZZ"
            .Bind(s => RuleResult.StopWith("STOP!"))    // Switch to Final track
            .Map(s => s + " IGNORED");                   // BYPASSED!

        // Assert - Stopped at Final track
        result.Should().BeOfType<RuleResult.Final>();
        ((RuleResult.Final)result).Output.Should().Be("STOP!");
    }

    [Test]
    public void Railway_StartOnFinalTrack_BypassesAll()
    {
        // Arrange - Start on Final track (already at terminal)
        var train = RuleResult.StopWith("Terminal");

        // Act - All operations bypassed
        var result = train
            .Map(s => "IGNORED")
            .Bind(s => RuleResult.ContinueWith("ALSO IGNORED"))
            .Map(s => "STILL IGNORED");

        // Assert - Stays on Final track
        result.Should().BeOfType<RuleResult.Final>();
        ((RuleResult.Final)result).Output.Should().Be("Terminal");
    }

    #endregion

    #region Bind: Railway Switch Points

    [Test]
    public void Bind_CanSwitchFromSuccessToFinal()
    {
        // Arrange
        var train = RuleResult.ContinueWith("check");

        // Act - Bind decides which track to take
        var result = train.Bind(output =>
            output == "check"
                ? RuleResult.StopWith("Found it!")
                : RuleResult.ContinueWith(output));

        // Assert - Switched to Final
        result.Should().BeOfType<RuleResult.Final>();
        ((RuleResult.Final)result).Output.Should().Be("Found it!");
    }

    [Test]
    public void Bind_CanStayOnSuccessTrack()
    {
        // Arrange
        var train = RuleResult.ContinueWith("continue");

        // Act - Bind keeps on Success track
        var result = train.Bind(output =>
            output == "stop"
                ? RuleResult.StopWith("Stopped")
                : RuleResult.ContinueWith(output + "!"));

        // Assert - Still on Success
        result.Should().BeOfType<RuleResult.Continue>();
        ((RuleResult.Continue)result).Output.Should().Be("continue!");
    }

    #endregion

    #region Map: Transform While Staying on Track

    [Test]
    public void Map_TransformsSuccessTrack()
    {
        // Arrange
        var train = RuleResult.ContinueWith("fizz");

        // Act
        var result = train.Map(s => s.ToUpper());

        // Assert - Transformed, still on Success track
        result.Should().BeOfType<RuleResult.Continue>();
        ((RuleResult.Continue)result).Output.Should().Be("FIZZ");
    }

    [Test]
    public void Map_IgnoresFinalTrack()
    {
        // Arrange
        var train = RuleResult.StopWith("terminal");

        // Act - Map doesn't affect Final track
        var result = train.Map(s => s.ToUpper());

        // Assert - Unchanged, still on Final track
        result.Should().BeOfType<RuleResult.Final>();
        ((RuleResult.Final)result).Output.Should().Be("terminal");
    }

    #endregion

    #region Tee: Observation Without Modification

    [Test]
    public void Tee_ExecutesSideEffectOnSuccessTrack()
    {
        // Arrange
        var train = RuleResult.ContinueWith("Fizz");
        var observed = "";

        // Act - Tee observes but doesn't change
        var result = train.Tee(output => observed = output);

        // Assert - Side effect executed, result unchanged
        observed.Should().Be("Fizz");
        result.Should().BeOfType<RuleResult.Continue>();
        ((RuleResult.Continue)result).Output.Should().Be("Fizz");
    }

    [Test]
    public void Tee_IgnoresFinalTrack()
    {
        // Arrange
        var train = RuleResult.StopWith("Terminal");
        var observed = "";

        // Act - Tee doesn't execute on Final
        var result = train.Tee(output => observed = output);

        // Assert - Side effect NOT executed
        observed.Should().BeEmpty();
        result.Should().BeOfType<RuleResult.Final>();
    }

    [Test]
    public void Tee_UsefulForLogging()
    {
        // Arrange
        var logs = new List<string>();
        var train = RuleResult.ContinueWith("Start");

        // Act - Chain with logging
        var result = train
            .Tee(s => logs.Add($"Before: {s}"))
            .Map(s => s.ToUpper())
            .Tee(s => logs.Add($"After: {s}"))
            .Bind(s => RuleResult.ContinueWith(s + "!"));

        // Assert - Logged at each step
        logs.Should().Equal("Before: Start", "After: START");
    }

    #endregion

    #region DoubleMap: Transform Both Tracks

    [Test]
    public void DoubleMap_TransformsBothTracks()
    {
        // Arrange - Success track
        var success = RuleResult.ContinueWith("fizz");
        
        // Act
        var resultSuccess = success.DoubleMap(
            continueTransform: s => s.ToUpper(),
            finalTransform: s => s.ToLower());

        // Assert
        ((RuleResult.Continue)resultSuccess).Output.Should().Be("FIZZ");

        // Arrange - Final track
        var final = RuleResult.StopWith("BUZZ");
        
        // Act
        var resultFinal = final.DoubleMap(
            continueTransform: s => s.ToUpper(),
            finalTransform: s => s.ToLower());

        // Assert
        ((RuleResult.Final)resultFinal).Output.Should().Be("buzz");
    }

    #endregion

    #region Collection Railways: Journey Through Stations

    [Test]
    public void FollowTrainUntilTerminal_StopsAtFinalStation()
    {
        // Arrange - Train journey with terminal
        var journey = new[]
        {
            RuleResult.ContinueWith("Station 1"),
            RuleResult.ContinueWith("Station 2"),
            RuleResult.StopWith("Terminal"),
            RuleResult.ContinueWith("Never reached"),
        };

        // Act
        var visited = journey.FollowTrainUntilTerminal().ToList();

        // Assert - Stopped at terminal
        visited.Should().HaveCount(3);
        visited[2].Should().BeOfType<RuleResult.Final>();
    }

    [Test]
    public void CompleteJourney_ReturnsTerminalIfReached()
    {
        // Arrange - Journey to terminal
        var journey = new[]
        {
            RuleResult.ContinueWith("Fizz"),
            RuleResult.ContinueWith("Buzz"),
            RuleResult.StopWith("Terminal Station"),
        };

        // Act
        var destination = journey.CompleteJourney(defaultDestination: "Default");

        // Assert - Reached terminal
        destination.Should().Be("Terminal Station");
    }

    [Test]
    public void CompleteJourney_AccumulatesIfNoTerminal()
    {
        // Arrange - Journey without terminal
        var journey = new[]
        {
            RuleResult.ContinueWith("Fizz"),
            RuleResult.ContinueWith("Buzz"),
            RuleResult.ContinueWith("Bang"),
        };

        // Act
        var destination = journey.CompleteJourney(defaultDestination: "Default");

        // Assert - Accumulated all stops
        destination.Should().Be("FizzBuzzBang");
    }

    [Test]
    public void CompleteJourney_ReturnsDefaultIfEmpty()
    {
        // Arrange - Empty journey
        var journey = new[]
        {
            RuleResult.Empty,
            RuleResult.Empty,
        };

        // Act
        var destination = journey.CompleteJourney(defaultDestination: "7");

        // Assert - Used default
        destination.Should().Be("7");
    }

    #endregion



    #region ThroughStations: Sequential Processing

    [Test]
    public void ThroughStations_ProcessesAllStations()
    {
        // Arrange - Start journey
        var train = RuleResult.ContinueWith("");

        // Define stations
        Func<string, RuleResult> station1 = s => RuleResult.ContinueWith(s + "A");
        Func<string, RuleResult> station2 = s => RuleResult.ContinueWith(s + "B");
        Func<string, RuleResult> station3 = s => RuleResult.ContinueWith(s + "C");

        // Act - Journey through all
        var result = train.ThroughStations(station1, station2, station3);

        // Assert
        ((RuleResult.Continue)result).Output.Should().Be("ABC");
    }

    [Test]
    public void ThroughStations_StopsAtTerminal()
    {
        // Arrange
        var train = RuleResult.ContinueWith("");

        // Define stations (middle one is terminal)
        Func<string, RuleResult> station1 = s => RuleResult.ContinueWith(s + "A");
        Func<string, RuleResult> terminal = s => RuleResult.StopWith("TERMINAL");
        Func<string, RuleResult> station3 = s => RuleResult.ContinueWith(s + "C");

        // Act - Should stop at terminal
        var result = train.ThroughStations(station1, terminal, station3);

        // Assert - Never reached station3
        result.Should().BeOfType<RuleResult.Final>();
        ((RuleResult.Final)result).Output.Should().Be("TERMINAL");
    }

    #endregion

    #region Try: Exception Handling (Derailment Recovery)

    [Test]
    public void Try_DivisionByZero_CapturesError()
    {
        // Arrange - Operation that will throw
        Func<RuleResult> divideByZero = () =>
        {
            var result = 42 / int.Parse("0");
            return RuleResult.ContinueWith(result.ToString());
        };

        // Act - Try prevents crash
        var result = RailwayExtensions.Try(divideByZero);

        // Assert - Error captured on Final track
        result.Should().BeOfType<RuleResult.Final>();
        ((RuleResult.Final)result).Output.Should().StartWith("Error:");
    }

    [Test]
    public void Try_InRailwayPipeline_StopsOnException()
    {
        // Arrange - Pipeline with dangerous operation
        Func<RuleResult> pipeline = () =>
            RuleResult.ContinueWith("Start")
                .Map(s => s + " -> Step1")
                .Bind(s => RailwayExtensions.Try(() =>
                {
                    if (s.Contains("Step1"))
                        throw new Exception("Derailment at Step1!");
                    return RuleResult.ContinueWith(s);
                }))
                .Map(s => s + " -> Step2");  // Never reached

        // Act
        var result = pipeline();

        // Assert - Stopped at exception, Step2 never executed
        result.Should().BeOfType<RuleResult.Final>();
        ((RuleResult.Final)result).Output.Should().Contain("Derailment at Step1!");
        ((RuleResult.Final)result).Output.Should().NotContain("Step2");
    }

    #endregion

    #region Pattern Matching: Railway Destinations

    [Test]
    public void Match_HandlesSuccessTrack()
    {
        // Arrange
        var train = RuleResult.ContinueWith("Success");

        // Act
        var result = train.Match(
            onContinue: s => $"Continued: {s}",
            onFinal: s => $"Final: {s}");

        // Assert
        result.Should().Be("Continued: Success");
    }

    [Test]
    public void Match_HandlesFinalTrack()
    {
        // Arrange
        var train = RuleResult.StopWith("Terminal");

        // Act
        var result = train.Match(
            onContinue: s => $"Continued: {s}",
            onFinal: s => $"Final: {s}");

        // Assert
        result.Should().Be("Final: Terminal");
    }

    [Test]
    public void Either_ExecutesDifferentSideEffects()
    {
        // Arrange
        var continueExecuted = false;
        var finalExecuted = false;

        // Act - Success track
        RuleResult.ContinueWith("test").Either(
            onContinue: _ => continueExecuted = true,
            onFinal: _ => finalExecuted = true);

        // Assert
        continueExecuted.Should().BeTrue();
        finalExecuted.Should().BeFalse();

        // Reset
        continueExecuted = false;

        // Act - Final track
        RuleResult.StopWith("test").Either(
            onContinue: _ => continueExecuted = true,
            onFinal: _ => finalExecuted = true);

        // Assert
        continueExecuted.Should().BeFalse();
        finalExecuted.Should().BeTrue();
    }

    #endregion

    #region Real-World Railway: FizzBuzz Examples

    [Test]
    public void RealWorld_FizzBuzz_Number15_RailwayStyle()
    {
        // Arrange - Rules evaluated for 15
        var journey = new[]
        {
            RuleResult.ContinueWith("Fizz"),  // 15 % 3 == 0
            RuleResult.ContinueWith("Buzz"),  // 15 % 5 == 0
        };

        // Act - Railway journey
        var output = journey
            .FollowTrainUntilTerminal()  // No terminal, keeps going
            .CompleteJourney(defaultDestination: "15");

        // Assert
        output.Should().Be("FizzBuzz");
    }

    [Test]
    public void RealWorld_ExactMatch_Number42_RailwayStyle()
    {
        // Arrange - ExactMatch rule hits terminal
        var journey = new[]
        {
            RuleResult.ContinueWith("Fizz"),  // Would match
            RuleResult.StopWith("The answer"), // Terminal!
            RuleResult.ContinueWith("Buzz"),  // Never reached
        };

        // Act - Railway journey stops at terminal
        var output = journey
            .FollowTrainUntilTerminal()  // Stops at "The answer"
            .CompleteJourney(defaultDestination: "42");

        // Assert - Terminal takes precedence
        output.Should().Be("The answer");
    }

    [Test]
    public void RealWorld_ChainedTransformations_RailwayStyle()
    {
        // Arrange - Start with Fizz
        var train = RuleResult.ContinueWith("Fizz");

        // Act - Chain transformations railway style
        var result = train
            .Map(s => s.ToLower())           // "fizz"
            .Tee(s => Console.WriteLine(s))  // Log: "fizz"
            .Bind(s => s == "fizz"
                ? RuleResult.ContinueWith(s + "Buzz")
                : RuleResult.Empty)          // "fizzBuzz"
            .Map(s => s.ToUpper());          // "FIZZBUZZ"

        // Assert
        ((RuleResult.Continue)result).Output.Should().Be("FIZZBUZZ");
    }

    #endregion
}
