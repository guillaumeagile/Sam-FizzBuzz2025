

refactor Price , to extract (and remove) the VAT entity


composable:  so that Retail Accounting is able to calculate retail price based on Margin

note that Net Price will calculate by applying several rules,  first apply the Margin, then may be TransportationFee, then VAT if applyable



## Assesment

pass the harness verification

-> invoquer   cupid-step-1-2.harness.sh =>  code review + mutation testing + analyser roslyn + linter

## Harness (HA)

### HA1 - C# idioms

use FxCop to enforce C#14 style 


### HA2 - immutable data structures

enforce the usage of records and all imutable collections
no setter on any property


### HA3 - ADT (algebraic data structures)

Represent closed alternatives with `OneOf<T0, T1, ...>` from the `OneOf` NuGet package. Its `.Match(...)` API forces each alternative to be handled, without relying on an inheritance hierarchy. This also satisfies HA4's no-inheritance constraint.

Use these unions for expected outcomes, such as `OneOf<Product, ProductNotFound>` for a lookup or a union of success and named domain failures for an operation. Do not encode expected absence as `null`. Chain operations with narrowly scoped `Bind`/`Map` helpers that propagate each failure case; `OneOf` supplies exhaustive matching but does not itself provide monadic chaining. Keep unexpected failures exceptional rather than silently converting them into domain results.

The harness requires at least one actual `OneOf<T0, T1, ...>` usage in the analyzed source. Record hierarchies alone do not satisfy HA3.


You will gain OneOf . but also Result and Option, that are sum types, which are ADTs, and OneOf already expresses both:

- Option<T> is OneOf<T, None> or OneOf<T, NotFound>.
- Result<T, E> is OneOf<T, E> or OneOf<T, Error>.

OneOf.Types ships None, NotFound, Success, Error<T> and others.


#### HA3 - a

use OneOf for....


#### HA3 - b

use Result to avoid exceptions

- That means StoredProduct.Withdraw and the lookups that currently throw new Exception.
- Use a specific error union such as OneOf<Withdrawn, InsufficientStock> rather than a generic Result<T, string> or Result<T, Exception>.
- The failure cases are then a closed set that the compiler checks exhaustively.
- Keep exceptions for bugs and violated invariants, not for business outcomes.

Option: low priority

- C# 14 nullable reference types (Supplier?) already cover most of it.
- Use OneOf<Supplier, NotFound> only for lookups where absence is a normal answer and you want the caller forced to handle it. FindSupplierForRegion and GetStock are examples.
- The "TransportationFee if any, VAT if applicable" case needs no Option. An absent adjustment is simply missing from the ordered PriceAdjustment list, which is the composable design you want.







### HA4- CUPID Principle: Composable = Extend through composition, not modification

no inheritance, verified by static code analysis; model alternatives with OneOf rather than record inheritance.


### HA11 - Records are Value Objects, never entities

a record must never implement `IDentifiable` - identity belongs to entities (HA9), and a record's
job is to be a Value Object: immutable, equality by value, no identity. HA2 already forbids
setters and HA5 already caps public properties at 4, so this rule only adds the one thing they
don't check: record != entity.

This rule also pins the concrete deliverable of this step: a record named `Price` must exist
under `OmniProduct_CoreDomain.Models`. `Price` is the running Value Object example - refactor it
to drop the VAT entity, and keep it composable (Margin, then TransportationFee, then VAT) while
staying an immutable record.
