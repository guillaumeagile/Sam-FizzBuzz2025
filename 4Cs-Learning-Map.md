# 4Cs Learning Map: CUPID Learning Path
### Based on "Training from the Back of the Room" by Sharon Bowman

---


## 📖 C2: CONCEPTS (Chunked Learning)
**Goal**: Present new information in small, digestible chunks with active learning

### 🚀 CHUNK 1: SHIFT 1 - From SOLID to CUPID (OOP Improvement)
**Duration**: 60-90 minutes | **Paradigm**: Stay in OOP

---

#### Concept 1.1: Single Responsibility → 🔧 Unix Philosophy (15 min)
**"Do one thing well"**

##### Mini-Lecture (5 min)
- **Problem**: BaseRule has multiple responsibilities
  - Comparison logic (CompareTo)
  - Evaluation logic (Evaluate)
  - Domain knowledge (IsFizz, IsBuzz, IsBang)
  - Priority management

- **CUPID Principle**: Unix Philosophy = Do ONE thing well
  - Each class should have a single, focused purpose
  - Domain-specific knowledge belongs in domain-specific classes

##### Code Comparison (5 min)
**Before** (Multiple responsibilities):
```csharp
public abstract class BaseRule : IRule
{
    public int CompareTo(IRule other) { ... }           // Comparison
    public virtual string Evaluate(int number) { ... }  // Evaluation
    protected static bool IsFizz(int number) { ... }    // Domain: Fizz=3
    protected static bool IsBuzz(int number) { ... }    // Domain: Buzz=5
}
```

**After** (Single responsibility):
```csharp
public class FizzRule : BaseRule
{
    public override string Evaluate(int number) => 
        number % 3 == 0 ? "Fizz" : "";
}

public class BuzzRule : BaseRule
{
    public override string Evaluate(int number) => 
        number % 5 == 0 ? "Buzz" : "";
}
```

##### Quick Activity (5 min)
**Think-Pair-Share**: 
- Why is separating concerns better?
- What happens when you need to add a new rule?

---

#### Concept 1.2: Open/Closed → 🏠 Composable (15 min)
**"Rules should compose naturally without modification"**

##### Mini-Lecture (5 min)
- **Problem**: Hardcoded divisors (3, 5, 7) make extension impossible
- **CUPID Principle**: Composable = Extend through composition, not modification
- **Solution**: Make divisors configurable parameters

##### Code Comparison (5 min)
**Before** (Hardcoded, not composable):
```csharp
protected static bool IsFizz(int number) => number % 3 == 0;  // 3 hardcoded!
protected static bool IsBuzz(int number) => number % 5 == 0;  // 5 hardcoded!
```

**After** (Composable):
```csharp
public class DivisibilityRule : BaseRule
{
    public int Divisor { get; }
    public string Output { get; }

    public DivisibilityRule(int divisor, string output)
    {
        Divisor = divisor;
        Output = output;
    }

    public override string Evaluate(int number) => 
        number % Divisor == 0 ? Output : "";
}

// Now compose ANY rule:
var fizz = new DivisibilityRule(3, "Fizz");
var buzz = new DivisibilityRule(5, "Buzz");
var custom = new DivisibilityRule(11, "Eleven");
```

##### Quick Activity (5 min)
**Challenge**: How would you create a rule for divisibility by 13 that outputs "Lucky"?

---

#### Concept 1.3: Complex Priority → 🔮 Predictable (15 min)
**"Behavior should be obvious - no hidden complexity"**

##### Mini-Lecture (5 min)
- **Problem**: Hidden priority system with magic numbers
- **CUPID Principle**: Predictable = Obvious behavior, no surprises
- **Solution**: Use insertion order instead of priority

##### Code Comparison (5 min)
**Before** (Hidden priority):
```csharp
public class FizzRule : BaseRule
{
    public FizzRule() => base._priority = 1;  // Magic number!
}

// Engine sorts by priority - hidden behavior!
_rules = new SortedSet<IRule>(rules);
```

**After** (Predictable order):
```csharp
public class DivisibilityRule : BaseRule
{
    // No priority property needed
}

// Order = insertion order - obvious!
public CupidFizzBuzzEngine(IEnumerable<IRule> rules)
{
    _rules = rules.ToList(); // First added = first processed
}
```

##### Quick Activity (5 min)
**Discussion**: Which is easier to understand? Why does predictability matter?

---

#### Concept 1.4: Old Syntax → 🆔 Idiomatic (10 min)
**"Code should feel natural in the language"**

##### Mini-Lecture (3 min)
- **Problem**: Old-fashioned C# patterns (verbose syntax, unnecessary ceremony)
- **CUPID Principle**: Idiomatic = Use modern language features that reduce noise
- **Solution**: Target-typed `new`, expression-bodied members, records, primary constructors

##### Code Comparison (4 min)

**Example 1: Target-typed `new` (C# 9+)**

**Before** (Old-fashioned):
```csharp
// Verbose constructor calls with explicit type names
public static CupidFizzBuzzEngine Standard()
{
    return new CupidFizzBuzzEngine(FizzBuzzRules.StandardGame());
}

public static CupidFizzBuzzEngine Extended(List<IRule> extendedRules)
{
    var mergedRules = FizzBuzzRules.StandardGame().Union(extendedRules);
    return new CupidFizzBuzzEngine(mergedRules);
}
```

**After** (Modern C# - Target-typed `new`):
```csharp
// Concise, idiomatic factory methods using target-typed new() expressions
public static CupidFizzBuzzEngine Standard() => 
    new(FizzBuzzRules.StandardGame());

public static CupidFizzBuzzEngine Extended(List<IRule> extendedRules)
{
    var mergedRules = FizzBuzzRules.StandardGame().Union(extendedRules);
    return new(mergedRules);  // Type inferred from return type
}
```

**Why it's better**: The compiler knows the return type, so repeating `CupidFizzBuzzEngine` is redundant noise.

---

**Example 2: Records with Nested Types - Simulating Discriminated Unions (C# 9+)**

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

**Example 3: Expression-bodied Members (C# 6+)**

**Before** (Old-fashioned):
```csharp
public class DivisibilityRule : RuleBase
{
    public override RuleResult Evaluate(int number)
    {
        if (number % Divisor == 0)
        {
            return RuleResult.ContinueWith(Output);
        }
        else
        {
            return RuleResult.Empty;
        }
    }
}
```

**After** (Modern C# - Expression-bodied + ternary):
```csharp
public class DivisibilityRule : RuleBase
{
    public override RuleResult Evaluate(int number) => 
        number % Divisor == 0 
            ? RuleResult.ContinueWith(Output) 
            : RuleResult.Empty;
}
```

**Why it's better**: More concise, reads like a mathematical expression, eliminates unnecessary ceremony.

##### Quick Activity (3 min)
**Spot the Difference**: What makes the "after" code more idiomatic?

---
## Next chapter :
### 🎯 CHUNK 2: SHIFT 2 - From OOP to Functional Programming (Paradigm Change)
**Duration**: ?? minutes | **Paradigm**: Functional Programming
