### Step 1.1: Single Responsibility → Unix Philosophy
**"One class, one reason to change"**

**Problem**: `ProductService` handles catalog, pricing, stock, supplier notification, and transport — all at once.

**Plan**:
- Identify every distinct concern currently living in `ProductService` and `Product`
- Extract each concern into its own class with a single focus
- Use the domain vocabulary to name each class (not `ProductCatalogManager` — just `Catalog`)



## ** Exercise **

extract a Product smallest entity that fits the Storage BC

in the Storage sub domain, keep only properties that are usefull for stocking the merchandise in a Warehouse

same for  catalog, pricing, supplier notification, and transport

## Assesment

run your code through our harness to verify you succeed this part:

```
./cupid-step-1-1.harness.sh
```

Deterministic and Roslyn-based (see `Cupid.Harness`), not AI-guided - same code in, same
pass/fail out, every time. Rule implementations and their pinning tests live in
`Cupid.Harness/Rules/` and `Cupid.Harness.Test/`.


### HA5 - no god class

no class shall have more than 6 public properties

### HA6 - Single Concern

a class's public methods must belong to at most one bounded-context concern (Catalog, Pricing,
Storage, Supplier, Sales, Lifecycle, Notification, Transport - see
`Cupid.Harness/ubiquitous-language-map.json`). A method is assigned to a concern by matching its
name against that concern's vocabulary. A class whose public API spans two or more concerns is
mixing responsibilities - the exact `ProductService`/`Product` smell this exercise targets.

### HA7 - Fan-out

a class may directly reference (via fields, constructor/method parameters, or local variables) at
most 3 distinct domain types (types declared in this codebase, not BCL/framework types). A class
wiring together many unrelated domain types is orchestrating too many concerns even if it stays
under the HA5 property cap or doesn't trip HA6's vocabulary check.

### HA8 - No Persistence in Domain

a domain model must not know how it is stored. Flags, on any type that is not itself EF
infrastructure (a `DbContext` subclass):
- EF Core data-annotation attributes (`[Table]`, `[Column]`, `[Key]`, `[NotMapped]`,
  `[ForeignKey]`, ...) used directly on the domain type
- hand-rolled "shadow properties" that flatten a real domain value for the ORM (any property
  ending in `Csv` or `Json`)
- sync/hydrate methods that exist only to keep those shadows in agreement with the real domain
  fields (`SyncEfColumns`, `HydrateFromEfColumns`, or any `Sync*`/`Hydrate*`/`*ToEntity`/
  `*FromEntity` method)

This is the persistence-concern heuristic: instead of folding "persistence" into HA6's vocabulary
map (which would only catch it as "one more concern among others"), HA8 makes it non-negotiable -
a domain type either stays free of ORM vocabulary entirely, or the harness fails, regardless of
how many other concerns it mixes in. Move the mapping into the `DbContext` (fluent API) or a
dedicated infrastructure/mapping type instead.
