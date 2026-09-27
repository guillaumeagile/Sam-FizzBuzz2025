

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

in C#14, enforce the use of pseudo-unions type with record hierarchy	
records are a C# 9 feature.  
https://comcomponent.com/en/blog/2026/06/09/003-dotnet-algebraic-data-types/#10-about-c-15-union-types


### HA4- CUPID Principle: Composable = Extend through composition, not modification

no inheritance, verifed by static code analysis


### HA11 - Records are Value Objects, never entities

a record must never implement `IDentifiable` - identity belongs to entities (HA9), and a record's
job is to be a Value Object: immutable, equality by value, no identity. HA2 already forbids
setters and HA5 already caps public properties at 4, so this rule only adds the one thing they
don't check: record != entity.

This rule also pins the concrete deliverable of this step: a record named `Price` must exist
under `OmniProduct_CoreDomain.Models`. `Price` is the running Value Object example - refactor it
to drop the VAT entity, and keep it composable (Margin, then TransportationFee, then VAT) while
staying an immutable record.
