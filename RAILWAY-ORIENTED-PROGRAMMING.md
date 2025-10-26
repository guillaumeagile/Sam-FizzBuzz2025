# Railway Oriented Programming in C#
## Based on Scott Wlaschin's F# Pattern

---

## 🚂 The Railway Metaphor

Imagine a train traveling on tracks. At any point, the train can be on one of two tracks:

```
Success Track (Continue):  =====[Station 1]=====>[Station 2]=====>[Station 3]====>
                                     ↓
Final Track (Terminal):              └====[TERMINAL STATION]====> (STOP)
```

- **Success Track (Continue)**: The train continues its journey, visiting each station
- **Final Track (Terminal)**: The train reaches a terminal station and stops

This is **Railway Oriented Programming** - a functional pattern for handling success and failure paths.

---

## 🎯 Why Railway Oriented Programming?

### The Problem: Nested If/Else Hell

Traditional imperative code for handling multiple operations:

```csharp
public string ProcessData(string input)
{
    var result = input;
    
    // Step 1
    if (IsValid(result))
    {
        result = Transform1(result);
        
        // Step 2
        if (IsValid(result))
        {
            result = Transform2(result);
            
            // Step 3
            if (IsValid(result))
            {
                result = Transform3(result);
            }
            else
            {
                return "Error at step 3";
            }
        }
        else
        {
            return "Error at step 2";
        }
    }
    else
    {
        return "Error at step 1";
    }
    
    return result;
}
```

**Problems**:
- ❌ Deep nesting ("pyramid of doom")
- ❌ Error handling mixed with business logic
- ❌ Hard to add/remove steps
- ❌ Difficult to test individual steps

### The Solution: Railway Pattern

```csharp
public string ProcessData(string input)
{
    return RuleResult.ContinueWith(input)
        .Bind(Validate)         // Can switch to Final track
        .Map(Transform1)        // Stays on same track
        .Map(Transform2)        // Stays on same track
        .Bind(Validate)         // Can switch to Final track
        .Map(Transform3)        // Stays on same track
        .Match(
            onContinue: result => result,
            onFinal: error => error);
}
```

**Benefits**:
- ✅ Flat, linear code
- ✅ Error handling automatic (via Final track)
- ✅ Easy to add/remove steps
- ✅ Each step testable independently
- ✅ Composable

---

## 🛤️ The Two Tracks: Continue vs Final

### In Our FizzBuzz Domain

```csharp
// Continue = Keep processing rules (Success track)
RuleResult.Continue("Fizz")  // More rules might add "Buzz"

// Final = Stop processing (Terminal track)
RuleResult.Final("42")       // No more rules evaluated
```

### Railway Metaphor Mapping

| Railway Term | FizzBuzz Term | Meaning |
|-------------|---------------|---------|
| Success Track | `Continue` | Keep evaluating rules |
| Final Track | `Final` | Stop immediately with this output |
| Station | Rule | Point where processing happens |
| Terminal | Exact Match Rule | Stops the entire journey |
| Switch Points | `Bind` | Decision to change tracks |
| Transform | `Map` | Modify payload on current track |

---

## 🔧 Core Railway Operations

### 1. Map: Transform on Current Track

**Purpose**: Transform the value without changing tracks

```csharp
RuleResult.ContinueWith("fizz")
    .Map(s => s.ToUpper())
// Result: Continue("FIZZ")
```

**Railway Diagram**:
```
Continue("fizz") ----[Map: ToUpper]----> Continue("FIZZ")

Final("42")      ----[Map: ToUpper]---X  Final("42")  (bypassed)
```

**Key Point**: Map on Final track does nothing (preserves short-circuit)

---

### 2. Bind: Switch Between Tracks

**Purpose**: Chain operations that can change tracks

```csharp
RuleResult.ContinueWith("check")
    .Bind(value => 
        value == "check" 
            ? RuleResult.StopWith("Found!") 
            : RuleResult.ContinueWith(value))
// Result: Final("Found!")
```

**Railway Diagram**:
```
                  ┌─> Continue(value)  (if not "check")
Continue("check") ┤
                  └─> Final("Found!")  (if "check") ← Switched track!
```

**Difference from Map**:
- **Map**: `string → string` (stays on same track)
- **Bind**: `string → RuleResult` (can switch tracks)

---

### 3. Tee: Observe Without Changing

**Purpose**: Execute side effects (logging, debugging) without modification

```csharp
var log = new List<string>();

RuleResult.ContinueWith("Fizz")
    .Tee(s => log.Add($"Saw: {s}"))  // Logs but doesn't change
    .Map(s => s.ToUpper())
    .Tee(s => log.Add($"Saw: {s}"))  // Logs again

// log = ["Saw: Fizz", "Saw: FIZZ"]
```

**Railway Diagram**:
```
Continue("Fizz") ----[Tee: Log]----> Continue("Fizz")  (unchanged)
                          ↓
                     [Side Effect: Log]
```

---

### 4. DoubleMap: Transform Both Tracks

**Purpose**: Rarely needed, but allows transformation of both Continue and Final

```csharp
result.DoubleMap(
    continueTransform: s => s.ToUpper(),
    finalTransform: s => s.ToLower())
```

---

## 🚉 Collection Railway Operations

### FollowTrainUntilTerminal

**Purpose**: Process results until reaching Final (terminal station)

```csharp
var journey = new[]
{
    RuleResult.ContinueWith("Fizz"),
    RuleResult.ContinueWith("Buzz"),
    RuleResult.StopWith("Terminal"),
    RuleResult.ContinueWith("Never reached")
};

var visited = journey.FollowTrainUntilTerminal();
// Returns: [Continue("Fizz"), Continue("Buzz"), Final("Terminal")]
```

**Railway Diagram**:
```
[Fizz] =====> [Buzz] =====> [Terminal] ===X=== [Never reached]
Station 1     Station 2     FINAL STOP        (Not visited)
```

---

### CompleteJourney

**Purpose**: Handle both outcomes - terminal or accumulated stops

```csharp
// With terminal
new[] { Continue("A"), Continue("B"), Final("STOP") }
    .CompleteJourney(defaultDestination: "Default")
// Returns: "STOP"

// Without terminal
new[] { Continue("A"), Continue("B"), Continue("C") }
    .CompleteJourney(defaultDestination: "Default")
// Returns: "ABC"

// Empty journey
new[] { Empty, Empty }
    .CompleteJourney(defaultDestination: "7")
// Returns: "7"
```

---

## 🔗 Composition: Linking Railway Segments

### Compose (Kleisli Composition)

**Purpose**: Link two railway functions sequentially

```csharp
Func<string, RuleResult> addFizz = s => RuleResult.ContinueWith(s + "Fizz");
Func<string, RuleResult> addBuzz = s => RuleResult.ContinueWith(s + "Buzz");

var composed = addFizz.Compose(addBuzz);
var result = composed("");  // "FizzBuzz"
```

**Railway Diagram**:
```
Input ====[addFizz]====> "Fizz" ====[addBuzz]====> "FizzBuzz"
```

**With Short-Circuit**:
```csharp
Func<string, RuleResult> addFizz = s => RuleResult.ContinueWith(s + "Fizz");
Func<string, RuleResult> stop = s => RuleResult.StopWith("STOP");
Func<string, RuleResult> addBuzz = s => RuleResult.ContinueWith(s + "Buzz");

var composed = addFizz.Compose(stop).Compose(addBuzz);
var result = composed("");  // "STOP" (addBuzz never executed)
```

**Railway Diagram**:
```
"" ====[addFizz]====> "Fizz" ====[stop]===X STOP  [addBuzz] (bypassed)
```

---

### ThroughStations

**Purpose**: Journey through multiple stations in sequence

```csharp
RuleResult.ContinueWith("Start")
    .ThroughStations(
        s => RuleResult.ContinueWith(s + "→A"),
        s => RuleResult.ContinueWith(s + "→B"),
        s => RuleResult.ContinueWith(s + "→C"))
// Result: Continue("Start→A→B→C")
```

**With Terminal**:
```csharp
RuleResult.ContinueWith("Start")
    .ThroughStations(
        s => RuleResult.ContinueWith(s + "→A"),
        s => RuleResult.StopWith("TERMINAL"),
        s => RuleResult.ContinueWith(s + "→C"))  // Never reached
// Result: Final("TERMINAL")
```

---

## 📊 Pattern Matching: Handling Both Tracks

### Match: Convert to Any Type

**Purpose**: Handle both tracks explicitly and return any type

```csharp
var message = result.Match(
    onContinue: output => $"Success: {output}",
    onFinal: output => $"Stopped at: {output}");
```

### Either: Execute Side Effects

**Purpose**: Execute different actions based on track

```csharp
result.Either(
    onContinue: output => Console.WriteLine($"Continuing: {output}"),
    onFinal: output => Console.WriteLine($"Final: {output}"));
```

---

## 🎮 Real FizzBuzz Examples

### Example 1: Standard FizzBuzz (Number 15)

**Imperative Version**:
```csharp
var result = "";
foreach (var rule in rules)
{
    var output = rule.Evaluate(15);
    if (!string.IsNullOrEmpty(output))
        result += output;
}
return string.IsNullOrEmpty(result) ? "15" : result;
```

**Railway Version**:
```csharp
return rules
    .Select(rule => rule.Evaluate(15))       // [Continue("Fizz"), Continue("Buzz")]
    .FollowTrainUntilTerminal()               // No terminal
    .CompleteJourney(defaultDestination: "15"); // "FizzBuzz"
```

**Railway Diagram**:
```
Fizz(15) =====> Continue("Fizz")
                     ↓
Buzz(15) =====> Continue("Buzz")
                     ↓
              [Accumulate] =====> "FizzBuzz"
```

---

### Example 2: Exact Match (Number 42)

**Imperative Version**:
```csharp
var result = "";
foreach (var rule in rules)
{
    var output = rule.Evaluate(42);
    if (rule.IsFinal)
        return output;
    result += output;
}
return result;
```

**Railway Version**:
```csharp
return rules
    .Select(rule => rule.Evaluate(42))        // [Continue("Fizz"), Final("The answer")]
    .FollowTrainUntilTerminal()                // Stops at Final
    .CompleteJourney(defaultDestination: "42"); // "The answer"
```

**Railway Diagram**:
```
Fizz(42) ======> Continue("Fizz")
                      ↓
ExactMatch(42) ====> Final("The answer") ===X=== [STOP]
                           ↓
                    Return "The answer"
```

---

### Example 3: Chained Transformations

**Railway Style**:
```csharp
RuleResult.ContinueWith("fizz")
    .Map(s => s.ToLower())              // "fizz"
    .Tee(s => Logger.Log(s))            // Log: "fizz"
    .Bind(s => s == "fizz"              // Decision point
        ? RuleResult.ContinueWith(s + "Buzz")
        : RuleResult.StopWith("ERROR"))
    .Map(s => s.ToUpper())              // "FIZZBUZZ"
    .Match(
        onContinue: s => s,
        onFinal: s => s);
// Result: "FIZZBUZZ"
```

**Railway Diagram**:
```
"fizz" --[Map: ToLower]--> "fizz" --[Tee: Log]--> "fizz"
                                                      ↓
                                    [Bind: Check "fizz"]
                                        ┌─────────┴─────────┐
                              Continue("fizzBuzz")    Final("ERROR")
                                        ↓
                              [Map: ToUpper]
                                        ↓
                              Continue("FIZZBUZZ")
                                        ↓
                                 [Match: Return]
                                        ↓
                                   "FIZZBUZZ"
```

---

## 🎯 Railway vs Traditional Error Handling

### Traditional Try/Catch

```csharp
public string Process(string input)
{
    try
    {
        var step1 = Step1(input);
        var step2 = Step2(step1);
        var step3 = Step3(step2);
        return step3;
    }
    catch (Exception ex)
    {
        return $"Error: {ex.Message}";
    }
}
```

**Problems**:
- ❌ Exceptions are expensive
- ❌ Control flow via exceptions
- ❌ Can't see error path in type signature

### Railway Style

```csharp
public string Process(string input)
{
    return RuleResult.ContinueWith(input)
        .Bind(Step1)  // Returns RuleResult (Continue or Final)
        .Bind(Step2)  // Automatically short-circuits on Final
        .Bind(Step3)  // Automatically short-circuits on Final
        .Match(
            onContinue: result => result,
            onFinal: error => error);
}

// Each step explicitly returns RuleResult
RuleResult Step1(string input) =>
    IsValid(input)
        ? RuleResult.ContinueWith(Transform(input))
        : RuleResult.StopWith("Error in Step1");
```

**Benefits**:
- ✅ Error path explicit in type
- ✅ No exceptions needed
- ✅ Composable
- ✅ Flat, linear code

---

## 📚 Scott Wlaschin's Original Railway Functions

Our C# implementation compared to F# original:

| F# (Wlaschin) | Our C# | Purpose |
|---------------|--------|---------|
| `bind` | `Bind` | Chain functions returning Result |
| `map` | `Map` | Transform success value |
| `tee` | `Tee` | Side effects without modification |
| `>>=` (operator) | `.Bind()` | Bind operator |
| `<!>` (operator) | `.Map()` | Map operator |
| `tryCatch` | `Try` | Exception to Result |
| `switch` | `Switch` | Convert normal function to railway |
| `doubleMap` | `DoubleMap` | Map both tracks |
| `plus` | (not needed) | Combine parallel railways |

---

## 🎓 Key Takeaways

### 1. **Two Tracks Always**
- Success/Continue: Processing continues
- Failure/Final: Processing stops

### 2. **Operations Respect Tracks**
- **Map**: Transforms Success track, bypasses Failure
- **Bind**: Can switch between tracks
- **Tee**: Observes without changing

### 3. **Composition is Key**
Chain operations without worrying about track switching - it happens automatically!

### 4. **Error Handling Built-In**
No need for try/catch or null checks - the Final track handles it

### 5. **Type Safety**
The type system enforces handling both tracks (via Match or Either)

---

## 🚀 Benefits Over Traditional Approaches

### vs If/Else Chains
- ✅ No nesting
- ✅ Linear, readable code
- ✅ Easy to add/remove steps

### vs Exceptions
- ✅ No performance penalty
- ✅ Error path explicit
- ✅ Composable

### vs Null Checking
- ✅ Type safety
- ✅ No null reference errors
- ✅ Explicit handling

---

## 📖 Further Reading

### Scott Wlaschin's Resources
- [Railway Oriented Programming (Blog)](https://fsharpforfunandprofit.com/rop/)
- [Railway Oriented Programming (Video)](https://vimeo.com/97344498)
- [Domain Modeling Made Functional (Book)](https://pragprog.com/titles/swdddf/)

### Related Patterns
- **Either Monad**: Our Continue/Final is an Either
- **Result Type**: Similar pattern in Rust, Swift
- **Option/Maybe Monad**: Handling null values
- **Validation**: Accumulating multiple errors

---

## 🎉 Conclusion

Railway Oriented Programming transforms complex error-prone code into elegant, composable pipelines.

**Before ROP**: Nested ifs, exceptions, null checks  
**After ROP**: Flat pipelines, type-safe, composable

Welcome aboard the functional programming railway! 🚂
