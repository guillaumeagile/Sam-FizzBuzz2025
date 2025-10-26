# 4Cs Learning Map: CUPID Learning Path
### Based on "Training from the Back of the Room" by Sharon Bowman

---


## 📖 C2: CONCEPTS (Chunked Learning)
**Goal**: Present new information in small, digestible chunks with active learning


### 🎯 CHUNK 2: SHIFT 2 - From OOP to Functional Programming (Paradigm Change)
**Duration**: ?? minutes | **Paradigm**: Functional Programming

---

#### Concept 2.1: Two Concerns → 🔧 Single Function (20 min)
**"A rule is just a function: int → Either<Continue, Final>"**

##### Mini-Lecture (7 min)
- **Problem**: Two separate concerns in interface (Evaluate + Final)
- **FP Principle**: Combine concerns into single function with algebraic data type
- **Solution**: Either/Result monad pattern with RuleResult

##### Code Comparison (2 min)
**Before** (Two concerns):
```csharp
public interface IRule
{
    string Evaluate(int number);  // Concern 1: What output?
    bool Final { get; }          // Concern 2: Should we stop?
}
```

**After** (Single function with Either/Result monad):
```csharp
public interface IRule
{
    RuleResult Evaluate(int number); // One function, clear semantics
}

// Result monad: two possible states
public abstract record RuleResult
{
    public record Continue(string Output) : RuleResult;  // Keep going
    public record Final(string Output) : RuleResult;    // Stop here
}

// Usage:
// RuleResult.Final("42")
// RuleResult.Continue("Fizz")
```

##### Quick Activity (5 min)
**Think-Pair-Share**: How does this design eliminate the need for two separate properties?

#### Concept 2.4: Old Syntax → 🆔 Idiomatic (10 min)
**"Code should feel natural in the language"**

##### Mini-Lecture (3 min)
- **Problem**: Old-fashioned C# patterns (verbose syntax, unnecessary ceremony)
- **CUPID Principle**: Idiomatic = Use modern language features that reduce noise
- **Solution**: records to model nested types (union types alike) + pattern matching to model imperative code


**Example : Records with Nested Types - Simulating Discriminated Unions (C# 9+)**

**Before** (Old-fashioned classes):
```csharp
// Verbose class hierarchy with boilerplate
public abstract class RuleResult
{
    public class Continue : RuleResult
    {
        public string Output { get; }
        
        public Continue(string output)
        {
            Output = output;
        }
    }
    
    public class Final : RuleResult
    {
        public string Output { get; }
        
        public Final(string output)
        {
            Output = output;
        }
    }
}
```

**After** (Modern C# - Records as Discriminated Union Pattern):
```csharp
// Concise, immutable by default, with structural equality
// This simulates discriminated unions from F#/Haskell
public abstract record RuleResult
{
    public record Continue(string Output) : RuleResult;
    public record Final(string Output) : RuleResult;
}

// Usage: RuleResult.Final("42")
```

**Why it's better**: Records eliminate boilerplate, provide immutability by default, and give you structural equality for free.

This pattern simulates **discriminated unions** (a.k.a. sum types) from functional languages like F#, Haskell, or Scala.

**Note**: Even C# 14 doesn't have true discriminated unions (though they're proposed for future versions). This record pattern is the closest idiomatic approach to model "Either/Or" types in modern C#.

( see foot notes )





---

#### Concept 2.2: Mutable State → 🔮 Immutable Data (15 min)
**"Immutable data prevents bugs and surprises"**

##### Mini-Lecture (5 min)
- **Problem**: Mutable properties can change after creation
- **FP Principle**: Immutable data eliminates entire categories of bugs
- **Solution**: Records and read-only properties

##### Code Comparison (5 min)
**Before** (Mutable):
```csharp
public class DivisibilityRule : BaseRule
{
    public int Divisor { get; set; }     // Can change!
    public string Output { get; set; }   // Can change!
}
```

**After** (Immutable):
```csharp
public abstract record RuleResult
{
    public record Continue(string Output) : RuleResult;  // Immutable
    public record Final(string Output) : RuleResult;    // Immutable
}

public class DivisibilityRule : RuleBase
{
    public int Divisor { get; }      // Read-only
    public string Output { get; }    // Read-only
    
    public DivisibilityRule(int divisor, string output)
    {
        Divisor = divisor;
        Output = output;
    }
}
```

##### Quick Activity (5 min)
**Discussion**: What bugs can immutability prevent?

---

#### Concept 2.3: Imperative → 🆔 Pattern Matching (15 min)
**"Declarative code is more readable than imperative"**

##### Mini-Lecture (5 min)
- **Problem**: Old-fashioned switch/case statements
- **FP Principle**: Pattern matching is more expressive
- **Solution**: Modern C# pattern matching expressions

##### Code Comparison (5 min)
**Before** (Imperative):
```csharp
switch (ruleResult)
{
    case RuleResult.Final(var output):
        return output;
    case RuleResult.Continue(var output) when !string.IsNullOrEmpty(output):
        result += output;
        break;
    case RuleResult.Continue:
        break;
}
```

**After** (Declarative and functional like):
```csharp
result = ruleResult switch
{
    RuleResult.Final(var output) => output,
    RuleResult.Continue(var output) when !string.IsNullOrEmpty(output) => result + output,
    RuleResult.Continue => result,
    _ => result
};
```

Hints to read that code better:
- **Switch**:  upon `ruleResult` a pattern type is trying to be matched
- **Pattern Type**: `RuleResult.Final(var output)` or `RuleResult.Continue(var output)`
- **Deconstruction**: `var output` 
This is Deconstruction in Pattern Matching:
  The pattern ```RuleResult.Final(var output)``` extracts the Output property from the Final record
  and captures the deconstructed value.
  
  - It's similar to:  ```if (ruleResult is RuleResult.Final final) { var output = final.Output; }```
- **Guard**: `when !string.IsNullOrEmpty(output)`
- **Default**: `_ => result`
- **Result Expression**: is what you read on the right-hand side of the `=>` (the value to return) . 
   - Sometimes called the **Arm body** - The body of the switch arm




##### Quick Activity (5 min)
**Compare**: Which version is easier to read? Why?

---




---

## 🛠️ C3: CONCRETE PRACTICE (Hands-On Activities)
**Goal**: Apply concepts through deliberate practice


### Practice Session 2: Journey to Pure Functional Programming (90-120 min)
**Objective**: Transform imperative code into pure functional style using monadic operations
**Goal**: Achieve "No imperative control flow - just function composition"

---

#### Exercise 4: Implement the Either/Result Monad (20 min)
**Task**: Create `RuleResult` type with Continue and Final cases
- Define abstract record `RuleResult` with nested records
- Implement `Continue(string Output)` for accumulating results
- Implement `Final(string Output)` for short-circuit results
- Add factory methods: `ContinueWith()`, `StopWith()`, `Empty`

**Code to Write** (in `RuleResult.cs`):
```csharp
public abstract record RuleResult
{
    public record Continue(string Output) : RuleResult;
    public record Final(string Output) : RuleResult;
    
    public static RuleResult ContinueWith(string output) => new Continue(output);
    public static RuleResult StopWith(string output) => new Final(output);
    public static readonly RuleResult Empty = new Continue("");
}
```

**Why This Matters**: 
- Replaces two separate concerns (string + bool) with single type
- Either monad: result is **either** Continue **or** Final
- Type system enforces handling both cases

**Success Criteria**:
- ✅ RuleResult defined as discriminated union pattern
- ✅ Immutable by default (records)
- ✅ Factory methods work
- ✅ Basic tests pass

**Reference**: `RuleResult.cs`, `A_RuleResultTests.cs`

---

#### Exercise 5: Implement Monadic Operations (35 min)
**Task**: Build the functional toolkit - Map, Bind, and collection operations

**Step 5.1: Map (Functor) - 10 min**
Transform the value while preserving the context:
```csharp
public static RuleResult Map(this RuleResult result, Func<string, string> f)
{
    return result switch
    {
        RuleResult.Continue(var output) => RuleResult.ContinueWith(f(output)),
        RuleResult.Final(var output) => RuleResult.StopWith(output), // Unchanged
        _ => result
    };
}
```

**Key Insight**: Map on Final does nothing - preserves short-circuit!

**Step 5.2: Bind (Monad) - 10 min**
Chain operations that return new RuleResult:
```csharp
public static RuleResult Bind(this RuleResult result, Func<string, RuleResult> f)
{
    return result switch
    {
        RuleResult.Continue(var output) => f(output),
        RuleResult.Final(var output) => RuleResult.StopWith(output), // Short-circuits
        _ => result
    };
}
```

**Key Insight**: Bind can change from Continue to Final (or vice versa)

**Step 5.3: TakeUntilFinal - 10 min**
Process collection until hitting Final:
```csharp
public static IEnumerable<RuleResult> TakeUntilFinal(this IEnumerable<RuleResult> results)
{
    foreach (var result in results)
    {
        yield return result;
        if (result is RuleResult.Final)
            yield break; // Stop processing
    }
}
```

**Key Insight**: Replaces imperative `if (final) return` with functional operation

**Step 5.4: CombineResults - 5 min**
The main composition - handle both Continue and Final:
```csharp
public static string CombineResults(this IEnumerable<RuleResult> results, string fallback)
{
    var resultsList = results.ToList();
    
    // Check for Final (escape hatch)
    var finalOutput = resultsList.ExtractFinalOutput();
    if (finalOutput != null)
        return finalOutput;
    
    // Fold all Continue results
    var accumulated = resultsList
        .TakeWhile(r => r is not RuleResult.Final)
        .FoldOutputs();
    
    return string.IsNullOrEmpty(accumulated) ? fallback : accumulated;
}
```

**Success Criteria**:
- ✅ Map transforms Continue, bypasses Final
- ✅ Bind can switch between Continue/Final
- ✅ TakeUntilFinal stops at first Final
- ✅ CombineResults handles both cases
- ✅ All helper methods implemented (ExtractFinalOutput, FoldOutputs)

**Reference**: `RuleResultExtensions.cs`

---

#### Exercise 6: Refactor Rules to Return RuleResult (20 min)
**Task**: Update all rules to use the new monadic type

**Before** (Old interface):
```csharp
public interface IRule
{
    string Evaluate(int number);
    bool Final { get; }
}
```

**After** (Monadic interface):
```csharp
public interface IRule
{
    RuleResult Evaluate(int number); // Single function!
}
```

**Update Each Rule**:
1. **DivisibilityRule**: Returns `Continue` with output or `Empty`
2. **ExactMatchRule**: Returns `Final` (stops processing)
3. **Other rules**: Follow the pattern

**Example - DivisibilityRule**:
```csharp
public RuleResult Evaluate(int number)
{
    return number % Divisor == 0
        ? RuleResult.ContinueWith(Output)
        : RuleResult.Empty;
}
```

**Success Criteria**:
- ✅ All rules return RuleResult
- ✅ No more separate Final property
- ✅ Single responsibility: one function
- ✅ Tests updated and passing

---

#### Exercise 7: Transform Engine - Imperative → Functional (25 min)
**Task**: This is the key transformation - removing ALL imperative control flow

**Level 0: Starting Point (Imperative)**
```csharp
public string Evaluate(int number)
{
    var result = string.Empty;  // ❌ Mutable state
    
    foreach (var rule in _rules)  // ❌ Imperative loop
    {
        var ruleResult = rule.Evaluate(number);
        
        if (ruleResult is RuleResult.Final final)  // ❌ Nested if
            return final.Output;
        
        if (ruleResult is RuleResult.Continue cont && !string.IsNullOrEmpty(cont.Output))
            result += cont.Output;
    }
    
    return string.IsNullOrEmpty(result) ? number.ToString() : result;  // ❌ Ternary
}
```

**Problems**: Mutable state, loops, nested ifs, hard to test

---

**Level 1: Extract to LINQ (Better)**
```csharp
public string Evaluate(int number)
{
    var results = _rules
        .Select(rule => rule.Evaluate(number))  // ✅ LINQ instead of loop
        .ToList();
    
    // Still imperative after this...
    var finalResult = results.FirstOrDefault(r => r is RuleResult.Final);
    if (finalResult is RuleResult.Final final)
        return final.Output;
    
    var output = string.Join("", results
        .OfType<RuleResult.Continue>()
        .Select(c => c.Output));
    
    return string.IsNullOrEmpty(output) ? number.ToString() : output;
}
```

**Better**: No loop, but still has imperative if statements

---

**Level 2: Use Pattern Matching (Good)**
```csharp
public string Evaluate(int number)
{
    var results = _rules
        .Select(rule => rule.Evaluate(number))
        .TakeWhile(r => r is not RuleResult.Final)  // ✅ Functional short-circuit
        .ToList();
    
    var finalResult = _rules
        .Select(rule => rule.Evaluate(number))
        .OfType<RuleResult.Final>()
        .FirstOrDefault();
    
    if (finalResult != null)  // ❌ Still has if
        return finalResult.Output;
    
    var output = results
        .OfType<RuleResult.Continue>()
        .Select(c => c.Output)
        .Aggregate("", (acc, o) => acc + o);
    
    return string.IsNullOrEmpty(output) ? number.ToString() : output;
}
```

**Good**: More functional, but duplicates Select and still has if

---

**Level 3: Pure Functional with Monadic Operations (Best!)**
```csharp
public string Evaluate(int number)
{
    // Pure functional pipeline:
    // 1. MAP: Evaluate all rules (lazy evaluation with LINQ)
    // 2. TakeUntilFinal: Short-circuit on Final (monadic)
    // 3. CombineResults: Fold Continue or extract Final (monadic)
    
    return _rules
        .Select(rule => rule.Evaluate(number))  // Map: Apply each rule
        .TakeUntilFinal()                       // Short-circuit: Stop at Final
        .CombineResults(fallback: number.ToString()); // Reduce: Combine or fallback
}
```

**Perfect**: 
- ✅ No mutable state
- ✅ No imperative control flow (no if, no loops, no return)
- ✅ Just function composition
- ✅ Single expression
- ✅ Declarative - describes WHAT not HOW

**Success Criteria**:
- ✅ Engine.Evaluate is single expression
- ✅ No mutable variables
- ✅ No if statements
- ✅ No explicit loops
- ✅ All tests pass
- ✅ FizzBuzz works correctly

**Reference**: `CupidFizzBuzzEngine.cs`

---

#### Exercise 8: Comprehensive Testing (20 min)
**Task**: Write tests demonstrating monadic properties

**Test Categories**:

1. **Functor Laws (Map)**:
```csharp
[Test]
public void Map_OnContinue_TransformsOutput()
{
    var result = RuleResult.ContinueWith("fizz");
    var mapped = result.Map(s => s.ToUpper());
    
    ((RuleResult.Continue)mapped).Output.Should().Be("FIZZ");
}
```

2. **Monad Laws (Bind)**:
```csharp
[Test]
public void Bind_OnFinal_ShortCircuits()
{
    var result = RuleResult.StopWith("42");
    var wasExecuted = false;
    
    var bound = result.Bind(output => {
        wasExecuted = true;
        return RuleResult.ContinueWith("IGNORED");
    });
    
    wasExecuted.Should().BeFalse(); // Not executed!
}
```

3. **Collection Operations**:
```csharp
[Test]
public void TakeUntilFinal_StopsAtFirstFinal()
{
    var results = new[]
    {
        RuleResult.ContinueWith("Fizz"),
        RuleResult.StopWith("42"),
        RuleResult.ContinueWith("Never seen"),
    };
    
    var taken = results.TakeUntilFinal().ToList();
    taken.Should().HaveCount(2); // Stops after Final
}
```

4. **Real FizzBuzz Scenarios**:
```csharp
[Test]
public void RealScenario_FizzBuzz_Number15()
{
    var results = new[]
    {
        RuleResult.ContinueWith("Fizz"),
        RuleResult.ContinueWith("Buzz"),
    };
    
    var output = results.CombineResults(fallback: "15");
    output.Should().Be("FizzBuzz");
}
```

**Success Criteria**:
- ✅ Map tests verify functor behavior
- ✅ Bind tests verify monad behavior
- ✅ Collection tests verify short-circuit
- ✅ Integration tests verify FizzBuzz scenarios
- ✅ 15+ tests passing

**Reference**: `RuleResultMonadicTests.cs`

---

#### Exercise 9: Documentation Study (10 min)
**Task**: Understand the functional programming concepts
- Read `MONADIC-OPERATIONS.md` - Complete guide
- Study the progression from imperative to functional
- Understand Map vs Bind
- See real FizzBuzz examples with monadic operations

**Key Concepts to Master**:
- 🎯 **Either Monad**: Continue or Final (two possible states)
- 🔄 **Functor (Map)**: Transform while preserving context
- 🔗 **Monad (Bind)**: Chain operations that return new context
- 📉 **Fold/Reduce**: Combine collection into single value
- 🚫 **Short-circuit**: Stop early when condition met

**Success Criteria**:
- ✅ Can explain what a monad is
- ✅ Can explain Map vs Bind difference
- ✅ Understand short-circuit evaluation
- ✅ Can draw data flow diagram

**Reference**: `MONADIC-OPERATIONS.md`

---

#### 🎓 Key Learning Outcomes

By completing this session, you will have achieved:

**The Transformation**:
```
Imperative (Before)          →  Functional (After)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Mutable variables            →  Immutable data
Loops (foreach)              →  LINQ (Select, Where)
If statements                →  Pattern matching in monads
Early returns                →  Short-circuit operations
Multiple responsibilities    →  Single expression
Hard to test                 →  Each operation testable
Imperative control flow      →  Function composition
```

**Achieved**:
1. ✅ **No Mutable State**: Everything is immutable
2. ✅ **No Imperative Control**: No if, no loops, no return
3. ✅ **Just Composition**: Functions composed into pipeline
4. ✅ **Monadic Operations**: Map, Bind, Fold working
5. ✅ **Short-Circuit**: Final stops processing automatically
6. ✅ **Pure Functions**: Same input → same output

**CUPID Compliance**:
- 🏠 **Composable**: Monadic operations chain naturally
- 🔧 **Unix Philosophy**: Each operation does ONE thing
- 🔮 **Predictable**: Pure functions, no side effects
- 🆔 **Idiomatic**: Modern C# functional features
- 🚀 **Domain-Based**: Types model domain (Continue/Final)

---

#### 💡 Key Insights

**The Power of Monads**:
- Hide complexity in well-tested operations (Map, Bind)
- Expose simplicity in domain code (single pipeline)
- Automatic short-circuit (no manual if checks)
- Composability (chain operations freely)

**The Functional Mindset**:
- Think in transformations, not mutations
- Think in pipelines, not procedures
- Think in data flow, not control flow
- Think in expressions, not statements

**Success Criteria**:
- ✅ Achieved pure functional evaluation
- ✅ No imperative control flow
- ✅ Just function composition
- ✅ Understanding of monadic operations


---

## 🎓 C4: CONCLUSIONS (Reflection & Integration)
**Goal**: Summarize learning, reflect on insights, plan next steps

### Reflection Activities (20-30 min)

#### Activity 1: Learning Harvest (10 min)
**Individual Reflection**: Write answers to these questions:
1. What was the most surprising thing you learned today?
2. Which CUPID principle resonates most with you? Why?
3. What's one thing you'll do differently in your code tomorrow?

**Group Share**: Volunteers share one insight (2 min each)

---

#### Activity 2: Before/After Comparison (10 min)
**Pair Activity**: Compare your starting code with final code
- What improved the most?
- What was hardest to change?
- What would you do differently next time?

**Key Insights to Highlight**:
- 🏠 **Composable**: Pure functions compose naturally
- 🔧 **Unix Philosophy**: Each function does ONE thing well
- 🔮 **Predictable**: Immutable data + pure functions = no surprises
- 🆔 **Idiomatic**: Modern C# features make code expressive
- 🚀 **Domain-based**: Types model domain concepts clearly

---

#### Activity 3: CUPID vs SOLID (5 min)
**Quick Discussion**: When would you use CUPID over SOLID?

**Key Takeaway**: 
- SOLID = Good for traditional OOP
- CUPID = Better for modern, functional-leaning code
- Functional Programming + CUPID = Perfect harmony!

---

### Action Planning (10 min)

#### Activity 4: Personal Action Plan
**Individual Task**: Complete this action plan

**I will apply these learnings by**:
1. **Tomorrow**: [One small thing you'll do]
   - Example: "Refactor one class to have single responsibility"

2. **This Week**: [One medium thing you'll practice]
   - Example: "Make one service class use immutable data"

3. **This Month**: [One big thing you'll accomplish]
   - Example: "Refactor entire module to be CUPID-compliant"

**Share with accountability partner**: Exchange plans with one person

---

### Summary & Closing (5 min)

#### Key Learnings Recap
You've learned to transform "TERRIBLE OO DESIGN" into elegant, functional code through:

**SHIFT 1: SOLID → CUPID (OOP Improvement)**
- ✅ Single Responsibility → Unix Philosophy
- ✅ Open/Closed → Composable
- ✅ Complex Priority → Predictable
- ✅ Old Syntax → Idiomatic

**SHIFT 2: OOP → Functional Programming**
- ✅ Two Concerns → Single Function (Result monad)
- ✅ Mutable State → Immutable Data
- ✅ Imperative → Pattern Matching
- ✅ Object Methods → Pure Functions

#### Final Thought
**Remember**: Code is not just about making computers work - it's about making it easy for humans to understand, modify, and extend. CUPID principles + Functional Programming help you write code that's a joy to work with! 🎯

---

## 📚 Appendix: 4Cs Teaching Tips

### Timing Guidelines
- **C1 Connections**: 10-15 minutes (15% of session)
- **C2 Concepts**: 60-120 minutes (40% of session)
- **C3 Concrete Practice**: 90-120 minutes (35% of session)
- **C4 Conclusions**: 20-30 minutes (10% of session)

### Facilitation Tips

#### For Connections
- Use open-ended questions
- Encourage sharing experiences
- Make it safe to admit confusion
- Connect to real-world scenarios

#### For Concepts
- Keep lectures under 10 minutes
- Show code comparisons side-by-side
- Use "Think-Pair-Share" frequently
- Check understanding before moving on

#### For Concrete Practice
- Provide clear success criteria
- Circulate and provide feedback
- Encourage pair programming
- Celebrate small wins

#### For Conclusions
- Allow quiet reflection time
- Make action plans specific and achievable
- Create accountability partnerships
- End on a positive, empowering note

### Adaptation for Different Formats

#### Half-Day Workshop (4 hours)
- C1: 15 min
- C2: Focus on Shift 1 only (60 min)
- C3: Exercises 1-3 only (60 min)
- C4: 20 min

#### Full-Day Workshop (8 hours)
- C1: 15 min
- C2: Both shifts (120 min)
- C3: All exercises (150 min)
- C4: 30 min
- Breaks: 45 min total

#### Multi-Day Course (2-3 days)
- Day 1: Shift 1 (SOLID → CUPID)
- Day 2: Shift 2 (OOP → FP)
- Day 3: Integration & advanced topics

---

## 🎯 Learning Outcomes Assessment

### Self-Assessment Checklist
After completing this learning path, you should be able to:

**Understanding** (Knowledge):
- [ ] Explain the 5 CUPID principles
- [ ] Describe the difference between CUPID and SOLID
- [ ] Define what makes a function "pure"
- [ ] Explain the Either/Result monad pattern

**Application** (Skills):
- [ ] Refactor a class to follow Unix Philosophy
- [ ] Create composable rules with parameters
- [ ] Implement immutable data structures
- [ ] Write pure functions without side effects

**Analysis** (Critical Thinking):
- [ ] Identify SOLID violations in code
- [ ] Recognize opportunities for functional refactoring
- [ ] Evaluate code against CUPID principles
- [ ] Compare OOP vs FP approaches

**Creation** (Mastery):
- [ ] Design new features using CUPID principles
- [ ] Build composable, functional systems
- [ ] Create custom rules following learned patterns
- [ ] Apply learning to your own codebase

---

**Remember**: Learning is not linear. Revisit concepts, practice regularly, and be patient with yourself. The journey from "terrible OO design" to functional excellence is a marathon, not a sprint! 🚀


## 📚 Resources

### on Discriminated Unions

- https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/discriminated-unions
- https://fsharpforfunandprofit.com/posts/discriminated-unions/

## 👣 Foot notes

### on Discriminated Unions

#### What Makes It Similar to Discriminated Unions:
- Closed Set of Cases: The abstract base record RuleResult with sealed nested records creates a finite set of possible values (Continue or Final)
- Type Safety: Pattern matching ensures you handle all cases at compile time
- Immutability: Records are immutable by default, like functional discriminated unions
- Structural Equality: Records provide value-based equality, not reference equality

However, It's Not Perfect:

#### Limitations compared to true discriminated unions (F#/Haskell):

 - Not truly sealed: Without the sealed keyword on the base record, someone could theoretically create a third case
- More verbose: Requires explicit inheritance syntax
- No exhaustiveness checking: C# won't warn you if you forget a case in pattern matching (unless you use switch expressions carefully)


A better pattern:
```csharp
public abstract record RuleResult
{
    private RuleResult() { } // Private constructor prevents external inheritance
    
    public sealed record Continue(string Output) : RuleResult;
    public sealed record Final(string Output) : RuleResult;
}
```
