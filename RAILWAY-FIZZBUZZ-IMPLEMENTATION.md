# FizzBuzz as Railway Oriented Programming
## Scott Wlaschin's Pattern Applied to FizzBuzz

---

## 🎯 The Insight: FizzBuzz is a Railway Problem!

Our FizzBuzz problem has **two distinct control flows**:

1. **Continue accumulating**: "Fizz" + "Buzz" → "FizzBuzz"
2. **Stop immediately**: ExactMatch(42) → "The answer" (ignore everything else)

This is **exactly** what Railway Oriented Programming models!

---

## 🚂 The FizzBuzz Railway

### Visual Representation

```
Number 15:
┌────────┐     ┌────────┐     ┌────────┐
│ Fizz   │ ──→ │ Buzz   │ ──→ │ Result │
│ Rule   │     │ Rule   │     │        │
└────────┘     └────────┘     └────────┘
Continue       Continue       "FizzBuzz"
   ↓              ↓
 "Fizz"  +      "Buzz"


Number 42:
┌────────┐     ┌──────────────┐
│ Fizz   │ ──→ │ ExactMatch   │ ===X=== [STOP]
│ Rule   │     │ (42)         │
└────────┘     └──────────────┘
Continue       Final("The answer")
   ↓
"Fizz"         Return immediately
(ignored)
```

---

## 🏗️ Architecture Mapping

### Our Types vs Railway Pattern

| Railway Concept | Our Implementation | Purpose |
|----------------|-------------------|---------|
| **Success Track** | `RuleResult.Continue` | Keep processing rules |
| **Failure Track** | `RuleResult.Final` | Stop and return result |
| **Railway Type** | `RuleResult` (abstract record) | Either monad |
| **Bind** | `Bind(Func<string, RuleResult>)` | Chain operations |
| **Map** | `Map(Func<string, string>)` | Transform output |
| **Journey** | `IEnumerable<RuleResult>` | Collection of stations |
| **Terminal Station** | `Final` result | Stop point |
| **Complete Journey** | `CompleteJourney(fallback)` | Final output |

---

## 📊 Code Evolution: From Imperative to Railway

### Level 0: Original Imperative (Bad)

```csharp
public string Evaluate(int number)
{
    var result = string.Empty;
    
    foreach (var rule in _rules)
    {
        var output = rule.Evaluate(number);
        
        if (!string.IsNullOrEmpty(output))
        {
            result = result + output;
            
            if (rule.Final)  // Nested ifs!
            {
                return result;
            }
        }
    }
    
    if (string.IsNullOrEmpty(result))
        return number.ToString();
    else
        return result;
}
```

**Problems**: Mutable state, nested ifs, hard to test

---

### Level 1: Functional with Pattern Matching (Better)

```csharp
public string Evaluate(int number)
{
    var result = string.Empty;
    
    foreach (var rule in _rules)
    {
        var ruleResult = rule.Evaluate(number);
        
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
```

**Better**: Pattern matching, but still imperative loop and if statement

---

### Level 2: Monadic (Good)

```csharp
public string Evaluate(int number)
{
    return _rules
        .Select(rule => rule.Evaluate(number))
        .TakeUntilFinal()
        .CombineResults(fallback: number.ToString());
}
```

**Good**: Functional pipeline, but names aren't expressive of domain

---

### Level 3: Railway-Oriented Programming (Best!)

```csharp
/// <summary>
/// Railway Oriented Programming - based on Scott Wlaschin's pattern
/// Two tracks: Continue (success) or Final (terminal)
/// </summary>
public string Evaluate(int number)
{
    return _rules
        .Select(rule => rule.Evaluate(number))      // Evaluate at each station
        .FollowTrainUntilTerminal()                 // Journey until terminal
        .CompleteJourney(defaultDestination: number.ToString()); // Handle outcome
}
```

**Best**: Expressive domain language, railway metaphor clear

---

## 🎮 Railway Operations in Action

### Map: Transform on Success Track

```csharp
[Test]
public void Map_StaysOnSuccessTrack()
{
    var result = RuleResult.ContinueWith("fizz")
        .Map(s => s.ToUpper())
        .Map(s => s + "!");
    
    // Result: Continue("FIZZ!")
    // Still on Success track
}
```

**Railway Diagram**:
```
"fizz" ──[ToUpper]──> "FIZZ" ──[+ "!"]──> "FIZZ!"
  ↓                     ↓                    ↓
Continue            Continue              Continue
```

---

### Bind: Switch Between Tracks

```csharp
[Test]
public void Bind_CanSwitchToFinalTrack()
{
    var result = RuleResult.ContinueWith("42")
        .Bind(output => 
            output == "42" 
                ? RuleResult.StopWith("The answer!")
                : RuleResult.ContinueWith(output));
    
    // Result: Final("The answer!")
    // Switched to Final track
}
```

**Railway Diagram**:
```
"42" ──[Bind: Check]──┬──> Continue(output)  (if not "42")
   Continue           └──> Final("The answer!")  (if "42")
                              ↓
                        [Terminal Station]
```

---

### Complete Journey: Handle Both Outcomes

```csharp
[Test]
public void CompleteJourney_HandlesBothTracks()
{
    // Scenario 1: Reach terminal
    var journey1 = new[]
    {
        RuleResult.ContinueWith("Fizz"),
        RuleResult.StopWith("Terminal")
    };
    journey1.CompleteJourney("default").Should().Be("Terminal");
    
    // Scenario 2: No terminal
    var journey2 = new[]
    {
        RuleResult.ContinueWith("Fizz"),
        RuleResult.ContinueWith("Buzz")
    };
    journey2.CompleteJourney("default").Should().Be("FizzBuzz");
}
```

---

## 🔄 Railway Composition Examples

### Example 1: Chaining Transformations

```csharp
public void ProcessWithRailway(int number)
{
    var result = RuleResult.ContinueWith(number.ToString())
        .Map(AddPrefix)           // Transform
        .Bind(ValidateLength)     // Can fail
        .Map(ToUpper)             // Transform
        .Tee(Log)                 // Observe
        .Bind(FinalCheck);        // Can stop
    
    return result.Match(
        onContinue: output => $"Success: {output}",
        onFinal: output => $"Stopped: {output}");
}

RuleResult ValidateLength(string s) =>
    s.Length > 10 
        ? RuleResult.StopWith("Too long!")
        : RuleResult.ContinueWith(s);
```

---

### Example 2: Multiple Stations Journey

```csharp
public void ThroughMultipleRules(int number)
{
    return RuleResult.ContinueWith("")
        .ThroughStations(
            s => ApplyFizzRule(number, s),
            s => ApplyBuzzRule(number, s),
            s => ApplyBangRule(number, s),
            s => CheckExactMatch(number, s)  // Might stop here
        );
}

RuleResult CheckExactMatch(int number, string accumulated) =>
    number == 42 
        ? RuleResult.StopWith("The answer!")
        : RuleResult.ContinueWith(accumulated);
```

---

## 🎯 Real FizzBuzz Railway Scenarios

### Scenario 1: Standard FizzBuzz (15)

```csharp
Rules: [Fizz(3), Buzz(5)]
Number: 15

// Railway journey:
Evaluate(15):
    Fizz.Evaluate(15)  → Continue("Fizz")    [Station 1]
    Buzz.Evaluate(15)  → Continue("Buzz")    [Station 2]
    
    FollowTrainUntilTerminal() → [Continue("Fizz"), Continue("Buzz")]
    CompleteJourney("15") → "FizzBuzz"
```

**Railway Diagram**:
```
Start ═══[Fizz]═══> "Fizz" ═══[Buzz]═══> "Buzz" ═══[End]═══> "FizzBuzz"
          ↓                      ↓                     ↓
       Continue              Continue              Accumulate
```

---

### Scenario 2: Exact Match (42)

```csharp
Rules: [Fizz(3), ExactMatch(42), Buzz(5)]
Number: 42

// Railway journey:
Evaluate(42):
    Fizz.Evaluate(42)        → Continue("Fizz")       [Station 1]
    ExactMatch.Evaluate(42)  → Final("The answer")    [Terminal!]
    Buzz.Evaluate(42)        → NOT EVALUATED          [Bypassed]
    
    FollowTrainUntilTerminal() → [Continue("Fizz"), Final("The answer")]
    CompleteJourney("42") → "The answer"
```

**Railway Diagram**:
```
Start ═══[Fizz]═══> "Fizz" ═══[ExactMatch]═══X═══ [TERMINAL]
          ↓                         ↓
       Continue                  Final
                               "The answer"
                                    ↓
                            [Buzz: BYPASSED]
```

---

### Scenario 3: No Matches (7)

```csharp
Rules: [Fizz(3), Buzz(5)]
Number: 7

// Railway journey:
Evaluate(7):
    Fizz.Evaluate(7)  → Continue("")  [Empty output]
    Buzz.Evaluate(7)  → Continue("")  [Empty output]
    
    FollowTrainUntilTerminal() → [Continue(""), Continue("")]
    CompleteJourney("7") → "7"  [Used fallback]
```

**Railway Diagram**:
```
Start ═══[Fizz]═══> "" ═══[Buzz]═══> "" ═══[End]═══> "7" (fallback)
          ↓                 ↓                ↓
       Continue         Continue        No output
```

---

## 🧪 Testing Railway Properties

### Property 1: Map on Final Does Nothing

```csharp
[Property]
public void MapOnFinal_AlwaysBypassess(string output, Func<string, string> transform)
{
    var final = RuleResult.StopWith(output);
    var mapped = final.Map(transform);
    
    // Property: Final output unchanged
    mapped.Should().BeOfType<RuleResult.Final>();
    ((RuleResult.Final)mapped).Output.Should().Be(output);
}
```

---

### Property 2: Bind Short-Circuits

```csharp
[Property]
public void BindOnFinal_NeverExecutesFunction(string output)
{
    var executed = false;
    
    var final = RuleResult.StopWith(output);
    final.Bind(s => 
    {
        executed = true;
        return RuleResult.ContinueWith(s);
    });
    
    // Property: Function never executed
    executed.Should().BeFalse();
}
```

---

### Property 3: Journey Stops at First Terminal

```csharp
[Property]
public void Journey_StopsAtFirstTerminal(List<RuleResult> beforeTerminal, RuleResult terminal, List<RuleResult> afterTerminal)
{
    var journey = beforeTerminal
        .Append(terminal)
        .Concat(afterTerminal);
    
    var visited = journey.FollowTrainUntilTerminal().ToList();
    
    // Property: Stops right after terminal
    visited.Last().Should().Be(terminal);
    visited.Should().NotContain(afterTerminal);
}
```

---

## 🎓 Key Railway Concepts Applied

### 1. **Two-Track System**
✅ `Continue` (Success) and `Final` (Terminal) represent our two tracks

### 2. **Automatic Error Propagation**
✅ `Final` automatically short-circuits - no manual `if (error) return` needed

### 3. **Composability**
✅ Railway functions compose with `Bind` and `Compose`

### 4. **Type Safety**
✅ Compiler ensures we handle both tracks via `Match`

### 5. **Railway Operators**
✅ `Map`, `Bind`, `Tee` for transforming and chaining

---

## 📚 Comparison: Railway vs Other Patterns

| Pattern | Our Implementation | Difference |
|---------|-------------------|------------|
| **Option/Maybe** | Would be `Some/None` | We have two "success" cases |
| **Either/Result** | ✅ Continue/Final | Exactly this! |
| **Try/Success** | Would be `Success/Failure` | Final isn't a failure |
| **Validation** | Would accumulate errors | We stop at first Final |

**Key Insight**: Our `Continue/Final` is an **Either monad** where:
- **Right** (Success) = Continue (accumulate)
- **Left** (Short-circuit) = Final (stop)

---

## 🚀 Benefits Achieved

### Before Railway Pattern
```csharp
// ❌ Mutable state
var result = "";

// ❌ Nested conditions
if (!string.IsNullOrEmpty(output))
{
    result += output;
    if (rule.Final)
        return result;
}

// ❌ Hard to test
// ❌ Hard to reason about
```

### After Railway Pattern
```csharp
// ✅ No mutable state
return _rules
    .Select(rule => rule.Evaluate(number))
    .FollowTrainUntilTerminal()
    .CompleteJourney(number.ToString());

// ✅ Flat, linear
// ✅ Composable
// ✅ Easy to test
// ✅ Easy to reason about
```

---

## 🎉 Conclusion

Our FizzBuzz implementation demonstrates **Railway Oriented Programming** in C#:

- ✅ **Two tracks**: Continue (accumulate) and Final (stop)
- ✅ **Railway operations**: Map, Bind, Tee, Compose
- ✅ **Expressive domain language**: FollowTrainUntilTerminal, CompleteJourney
- ✅ **Type-safe**: Compiler enforces handling both tracks
- ✅ **Testable**: Each operation independently verified
- ✅ **CUPID-compliant**: Composable, Unix, Predictable, Idiomatic, Domain-based

**Scott Wlaschin's insight**: Complex control flow becomes simple data flow when you model it as a railway!

---

## 📖 Resources

- [Railway Oriented Programming (Blog)](https://fsharpforfunandprofit.com/rop/)
- [Railway Oriented Programming (Video)](https://vimeo.com/97344498)
- [Domain Modeling Made Functional](https://pragprog.com/titles/swdddf/)
- Our implementation: `RailwayExtensions.cs` and tests

---

**Welcome to the Railway! 🚂**
