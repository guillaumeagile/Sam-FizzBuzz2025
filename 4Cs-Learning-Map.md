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

#### Concept 2.4: Object Methods → 🏠 Pure Functions ( quickie, 5 min)
**"Pure functions are predictable, testable, and composable"**

##### Mini-Lecture (2 min)
- **Problem**: Methods with side effects are unpredictable
- **FP Principle**: Pure functions = same input → same output, no side effects
- **Solution**: Eliminate mutable state and side effects

##### Code Comparison (3 min)
**Before** (Impure - side effects):
```csharp
public class BuzzRule : BaseRule
{
    private int callCount = 0;  // Mutable state!
    
    public override string Evaluate(int number)
    {
        callCount++;  // Side effect!
        return number % 5 == 0 ? "Buzz" : "";
    }
}
```

**After** (Pure function):
```csharp
public class DivisibilityRule : RuleBase
{
    public override RuleResult Evaluate(int number) => 
        number % Divisor == 0 
            ? RuleResult.ContinueWith(Output) 
            : RuleResult.Empty;
    // No side effects, no mutable state, completely predictable
}
```


---

## 🛠️ C3: CONCRETE PRACTICE (Hands-On Activities)
**Goal**: Apply concepts through deliberate practice

### Practice Session 1: CUPID Refactoring (45-60 min)
**Objective**: Transform SOLID violations into CUPID-compliant code

#### Exercise 1: Extract Single Responsibilities (15 min)
**Task**: Refactor BaseRule to follow Unix Philosophy
- Remove domain-specific methods (IsFizz, IsBuzz, IsBang)
- Create specific rule classes (FizzRule, BuzzRule)
- Each class should have ONE clear purpose

**Success Criteria**:
- ✅ Each class has single responsibility
- ✅ No hardcoded domain knowledge in base class
- ✅ Tests pass

#### Exercise 2: Make Rules Composable (15 min)
**Task**: Create DivisibilityRule with configurable parameters
- Replace hardcoded divisors with constructor parameters
- Create rules for divisibility by 13 ("Lucky") and 11 ("Eleven")
- Implement ExactMatchRule for exact number matches (like 42)

**Success Criteria**:
- ✅ Can create any divisibility rule without modifying code
- ✅ Custom rules work correctly
- ✅ Tests pass

#### Exercise 3: Simplify Priority System (15 min)
**Task**: Replace priority system with insertion order
- Remove _priority property
- Use List instead of SortedSet
- Process rules in insertion order

**Success Criteria**:
- ✅ No priority property exists
- ✅ Order is obvious (insertion order)
- ✅ Tests pass

---

### Practice Session 2: Functional Programming (60-90 min)
**Objective**: Transform OOP code into functional style

#### Exercise 4: Implement Result Monad (20 min)
**Task**: Create RuleResult type and refactor IRule interface
- Define RuleResult abstract record with Continue and Final nested records
- Change IRule.Evaluate to return RuleResult
- Update existing rules to return RuleResult

**Success Criteria**:
- ✅ RuleResult type correctly models Result monad
- ✅ All rules return RuleResult
- ✅ Tests pass

#### Exercise 5: Pattern Matching Refactor (15 min)
**Task**: Replace switch/case with pattern matching in engine
- Open FizzBuzzEngine.Evaluate method
- Replace old switch statement with modern pattern matching expression
- Handle all RuleResult cases

**Success Criteria**:
- ✅ Uses pattern matching expression (switch expression)
- ✅ All cases handled
- ✅ Tests pass

#### Exercise 6: Ensure Immutability (15 min)
**Task**: Make all data structures immutable
- Convert mutable properties to read-only
- Use records where appropriate
- Ensure no state can change after creation

**Success Criteria**:
- ✅ No mutable properties
- ✅ Records used for data structures
- ✅ Tests pass

#### Exercise 7: Create Pure Functions (20 min)
**Task**: Eliminate side effects from all rules
- Remove any mutable state from rule classes
- Ensure Evaluate methods are pure functions
- Verify same input always produces same output

**Success Criteria**:
- ✅ No side effects in any rule
- ✅ All functions are pure
- ✅ Tests demonstrate referential transparency

---

### Practice Session 3: Integration & Extension (30-45 min)
**Objective**: Apply learning to new scenarios

#### Exercise 8: Create Custom Rules (20 min)
**Task**: Implement new rules using learned principles
- Create a rule for prime numbers
- Create a rule for perfect squares
- Combine multiple rules in interesting ways

**Success Criteria**:
- ✅ New rules follow CUPID principles
- ✅ New rules are composable
- ✅ Tests demonstrate correctness

#### Exercise 9: Refactor Your Own Code (25 min)
**Task**: Apply CUPID principles to your own codebase
- Identify a class with SOLID violations
- Apply one CUPID principle to improve it
- Share your refactoring with the group

**Success Criteria**:
- ✅ Applied at least one CUPID principle
- ✅ Code is measurably improved
- ✅ Can explain the improvement

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
