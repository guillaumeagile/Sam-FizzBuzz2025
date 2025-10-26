# Monadic Operations in FizzBuzz - A Gentle Introduction to Functional Programming

## Why Monads? The Problem with Imperative Code

### Before: Imperative (if/else) Version

```csharp
public string Evaluate(int number)
{
    var result = string.Empty;
    
    foreach (var rule in _rules)
    {
        var ruleResult = rule.Evaluate(number);
        
        // Lots of imperative control flow
        if (!string.IsNullOrEmpty(ruleResult.Output))
        {
            result = result + ruleResult.Output;
            
            if (rule.Final)  // More nesting!
            {
                return result;
            }
        }
    }
    
    if (string.IsNullOrEmpty(result))  // Even more ifs!
    {
        return number.ToString();
    }
    else
    {
        return result;
    }
}
```

**Problems**:
- ❌ Multiple levels of nesting
- ❌ Mutable state (`result` changes)
- ❌ Hard to test individual pieces
- ❌ Hard to reason about control flow
- ❌ Cannot compose with other operations

### After: Monadic (Functional) Version

```csharp
public string Evaluate(int number)
{
    return _rules
        .Select(rule => rule.Evaluate(number))      // Map
        .TakeUntilFinal()                           // Short-circuit
        .CombineResults(fallback: number.ToString()); // Reduce
}
```

**Benefits**:
- ✅ No nesting - flat pipeline
- ✅ No mutable state - pure functions
- ✅ Each operation is testable independently
- ✅ Clear data flow: rules → results → output
- ✅ Composable with other LINQ operations

---

## Understanding the Monad Pattern

### What is a Monad?

A monad is a design pattern that allows you to:
1. **Wrap** a value in a context (like `RuleResult`)
2. **Transform** values while keeping the context (`Map`)
3. **Chain** operations that return new contexts (`Bind`)
4. **Short-circuit** when needed (like `Final` stopping evaluation)

### Our Either Monad: `RuleResult`

```csharp
public abstract record RuleResult
{
    public record Continue(string Output) : RuleResult;  // Success case
    public record Final(string Output) : RuleResult;     // Short-circuit case
}
```

This is called an **Either monad** because it represents **either**:
- `Continue` with output (keep processing)
- `Final` with output (stop processing)

---

## The Monadic Operations Explained

### 1. Map (Functor Operation)

**Purpose**: Transform the value inside the context, without changing the context itself.

**Signature**: `RuleResult → (string → string) → RuleResult`

**Example**:
```csharp
var result = RuleResult.ContinueWith("fizz");
var upper = result.Map(s => s.ToUpper());  // ContinueWith("FIZZ")
```

**Key Insight**: Map on `Final` does nothing - it **preserves the short-circuit**:
```csharp
var final = RuleResult.StopWith("42");
var upper = final.Map(s => s.ToUpper());  // Still StopWith("42")
```

**Why?** Because `Final` means "stop processing" - no more transformations!

---

### 2. Bind (Monad Operation / FlatMap)

**Purpose**: Chain operations that themselves return `RuleResult`.

**Signature**: `RuleResult → (string → RuleResult) → RuleResult`

**Example**:
```csharp
var result = RuleResult.ContinueWith("fizz");

var bound = result.Bind(output => 
    output == "fizz" 
        ? RuleResult.StopWith("STOP!") 
        : RuleResult.ContinueWith(output));

// Result: StopWith("STOP!")
```

**Key Insight**: Bind lets you **change the context** (Continue → Final or vice versa).

**Why "Bind" and not "Map"?**
- `Map`: `(A → B)` - Simple transformation
- `Bind`: `(A → Monad<B>)` - Transformation that returns a new monad

---

### 3. TakeUntilFinal (Short-Circuit Operation)

**Purpose**: Process results until hitting a `Final`, then stop.

**Signature**: `IEnumerable<RuleResult> → IEnumerable<RuleResult>`

**Example**:
```csharp
var results = new[]
{
    RuleResult.ContinueWith("Fizz"),
    RuleResult.ContinueWith("Buzz"),
    RuleResult.StopWith("42"),
    RuleResult.ContinueWith("Never seen"),
};

var taken = results.TakeUntilFinal();
// Returns: [Continue("Fizz"), Continue("Buzz"), Final("42")]
```

**Why Important?** This replaces the imperative `if (rule.Final) return result;` with a pure function!

---

### 4. CombineResults (Fold/Reduce Operation)

**Purpose**: Combine all results into a single output string.

**Signature**: `IEnumerable<RuleResult> → string → string`

**Logic**:
1. If any `Final` exists → return its output (ignore everything else)
2. Otherwise → concatenate all `Continue` outputs
3. If no output → return fallback

**Example 1: Final Takes Precedence**
```csharp
var results = new[]
{
    RuleResult.ContinueWith("Fizz"),
    RuleResult.StopWith("42"),
    RuleResult.ContinueWith("Ignored"),
};

var output = results.CombineResults(fallback: "7");
// Returns: "42"
```

**Example 2: Concatenate Continue**
```csharp
var results = new[]
{
    RuleResult.ContinueWith("Fizz"),
    RuleResult.ContinueWith("Buzz"),
};

var output = results.CombineResults(fallback: "15");
// Returns: "FizzBuzz"
```

**Example 3: Fallback**
```csharp
var results = new[]
{
    RuleResult.Empty,
    RuleResult.Empty,
};

var output = results.CombineResults(fallback: "7");
// Returns: "7"
```

---

## Comparing Imperative vs Functional

### Imperative: Explicit Control Flow

```csharp
var result = string.Empty;

foreach (var rule in _rules)
{
    var ruleResult = rule.Evaluate(number);
    
    if (ruleResult is RuleResult.Final final)
        return final.Output;
    
    if (ruleResult is RuleResult.Continue cont && !string.IsNullOrEmpty(cont.Output))
        result += cont.Output;
}

return string.IsNullOrEmpty(result) ? number.ToString() : result;
```

**Characteristics**:
- Mutable state (`result`)
- Explicit loops
- Nested conditionals
- Early returns

### Functional: Data Pipeline

```csharp
return _rules
    .Select(rule => rule.Evaluate(number))
    .TakeUntilFinal()
    .CombineResults(fallback: number.ToString());
```

**Characteristics**:
- No mutable state
- Declarative pipeline
- No conditionals (hidden in monadic operations)
- Single expression

---

## Real FizzBuzz Examples

### Example 1: Number 15 (Fizz + Buzz)

**Rules**: Fizz(3), Buzz(5)

```csharp
// What happens:
_rules.Select(rule => rule.Evaluate(15))
// [Continue("Fizz"), Continue("Buzz")]

.TakeUntilFinal()
// [Continue("Fizz"), Continue("Buzz")] (no Final)

.CombineResults(fallback: "15")
// "FizzBuzz"
```

### Example 2: Number 42 (Exact Match)

**Rules**: Fizz(3), ExactMatch(42), Buzz(5)

```csharp
// What happens:
_rules.Select(rule => rule.Evaluate(42))
// [Continue("Fizz"), Final("The answer"), Continue("Buzz")]

.TakeUntilFinal()
// [Continue("Fizz"), Final("The answer")] (stops after Final)

.CombineResults(fallback: "42")
// "The answer" (Final takes precedence, Fizz ignored)
```

### Example 3: Number 7 (No Match)

**Rules**: Fizz(3), Buzz(5)

```csharp
// What happens:
_rules.Select(rule => rule.Evaluate(7))
// [Continue(""), Continue("")]

.TakeUntilFinal()
// [Continue(""), Continue("")] (no Final)

.CombineResults(fallback: "7")
// "7" (no output, use fallback)
```

---

## Key Functional Programming Concepts

### 1. Pure Functions

A function is **pure** if:
- Same input → same output (referential transparency)
- No side effects (doesn't mutate state)

```csharp
// Pure
string ToUpper(string s) => s.ToUpper();

// Impure
string _state = "";
string AddToState(string s) { _state += s; return _state; }  // Mutates!
```

### 2. Function Composition

Combining small functions into larger ones:

```csharp
// Small functions
Func<int, RuleResult> evaluate = n => rule.Evaluate(n);
Func<RuleResult, string> extract = r => r.Output;

// Composed
Func<int, string> composed = n => extract(evaluate(n));

// Or with LINQ:
var result = rules.Select(evaluate).Select(extract);
```

### 3. Immutability

Data never changes after creation:

```csharp
// Immutable (record)
public record RuleResult(string Output);

// Usage
var r1 = new RuleResult("Fizz");
// Cannot do: r1.Output = "Buzz";  // Compile error!

// Instead create new instance
var r2 = r1 with { Output = "Buzz" };
```

### 4. Higher-Order Functions

Functions that take or return other functions:

```csharp
// Takes a function as parameter
RuleResult Map(RuleResult r, Func<string, string> f)

// Returns a function
Func<int, bool> IsDivisibleBy(int divisor) => 
    number => number % divisor == 0;
```

---

## Testing Benefits

### Testable Monadic Operations

Each operation is independently testable:

```csharp
[Test]
public void Map_TransformsOutput()
{
    var result = RuleResult.ContinueWith("fizz");
    var upper = result.Map(s => s.ToUpper());
    
    upper.Should().Be(RuleResult.ContinueWith("FIZZ"));
}

[Test]
public void TakeUntilFinal_StopsAtFinal()
{
    var results = new[] { Continue("A"), Final("B"), Continue("C") };
    var taken = results.TakeUntilFinal();
    
    taken.Should().Equal(Continue("A"), Final("B"));
}
```

### Composable Tests

Test combinations of operations:

```csharp
[Test]
public void FullPipeline_Integration()
{
    var results = new[] { Continue("Fizz"), Continue("Buzz") };
    
    var output = results
        .TakeUntilFinal()
        .CombineResults(fallback: "15");
    
    output.Should().Be("FizzBuzz");
}
```

---

## CUPID Principles Achieved

### 🏠 Composable
- Monadic operations compose naturally
- Can chain Map, Bind, Filter operations
- Works with standard LINQ

### 🔧 Unix Philosophy
- Each operation does ONE thing
- TakeUntilFinal: short-circuits
- CombineResults: reduces
- Map: transforms

### 🔮 Predictable
- Pure functions: same input → same output
- No side effects
- Immutable data
- No hidden state

### 🆔 Idiomatic
- Uses modern C# features (records, pattern matching)
- LINQ-style composition
- Reads like English: "Take until final, combine results"

### 🚀 Domain-Based
- `RuleResult` models domain concept (continue vs stop)
- Operations use domain language
- Types prevent invalid states

---

## Further Learning

### Books
- "Functional Programming in C#" by Enrico Buonanno
- "Domain Modeling Made Functional" by Scott Wlaschin

### Concepts to Explore
- **Option/Maybe monad**: Handling null safely
- **Either monad**: Handling errors functionally
- **Validation monad**: Accumulating errors
- **Reader monad**: Dependency injection
- **State monad**: Managing state functionally

### Functional Languages
- **F#**: Functional-first .NET language
- **Haskell**: Pure functional programming
- **Scala**: Hybrid OOP/FP on JVM

---

## Conclusion

Monadic operations transform complex imperative code into simple, composable pipelines.

**Before**: Nested ifs, mutable state, hard to test
**After**: Pure functions, clear flow, easy to test

The key insight: **Hide complexity in well-tested monadic operations, expose simplicity in your domain code.**

Welcome to functional programming! 🎉
