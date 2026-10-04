# OneOf refactor plan

**Resume at:** Step 3 — define perishable date semantics and age-pricing behavior; then implement one null-free expected-outcome flow.

## Status

- [x] **2026-10-04 — reviewed** the workshop narrative, Step 1.2 requirements, current models/services/tests, and harness rules.
- [x] **2026-10-04 — added OneOf 3.0.271** to `OmniProduct-CoreDomain`; restore and build succeeded. Existing warnings include NU1903 for SQLitePCLRaw.lib.e_sqlite3 2.1.10 and nullable-property warnings.
- [x] **2026-10-04 — selected OneOf for HA3:** closed alternatives with exhaustive matching, without record inheritance; HA4's no-inheritance rule remains.
- [x] **2026-10-04 — updated HA3** to semantically recognize the actual `OneOf.OneOf<T...>` type, added harness references and pinning tests, and updated Step 1.2 docs/script. `dotnet test Cupid.Harness.Test/Cupid.Harness.Test.csproj --nologo` passed (66/66).
- [x] **2026-10-04 — committed** package/HA3/docs as `f5a14ff`; plan checkpoint as `d0c1a7c`.
- [x] **2026-10-04 — chainability guidance:** OneOf models alternatives, not operation sequencing. Keep entity commands explicit by default; use function composition for immutable values and application services for cross-entity orchestration. Returning `this` is an option only if fluent mutation is an explicit API goal.
- [x] **2026-10-04 — refined null-free result guidance:** Maybe/Option fits a value that may simply be absent; OneOf fits named, distinct operation outcomes. Use narrow `Map`/`Bind` composition where it clarifies real workflows, and keep unexpected failures exceptional.
- [x] **2026-10-04 — product examples:** shelf-life is exactly `Perishable` or `NonExpiring`; age-based appreciation is a separate pricing policy and may coexist with either.
- [x] **2026-10-04 — pulled colleague commit** `6fd9c13` (`for sam`) and integrated its HA3/Result/Option notes and ADT candidates. Merge conflict in this plan was resolved to preserve the current decisions and record the incoming ideas as proposals.
- [ ] **Next:** agree on date semantics (sell-by vs safety expiration and sale/discount consequences) and age-pricing formula/cap; choose one expected-outcome flow to implement. Before broadening model records, resolve HA2/HA9/HA11's entity-versus-value-object scope.

## Agreed direction

1. **Closed alternatives:** HA3 checks use of OneOf; HA4 continues to reject inheritance. Do not turn every class or entity into a union.
2. **Optionality vs failures:** represent ordinary presence/absence with Maybe/Option when appropriate. A named `NotFound` OneOf case is also reasonable when callers must distinguish and handle that expected absence. Represent business outcomes, such as successful withdrawal vs insufficient stock, with specific OneOf cases rather than generic strings/exceptions.
3. **Composition:** OneOf has exhaustive `Match`; it does not itself provide `Map`/`Bind`. Add only small helpers required by an implemented flow. Omit optional price adjustments from the ordered adjustment list rather than wrapping each in an Option.
4. **Perishable and age-appreciating products:** use `OneOf<Perishable, NonExpiring>` for mutually exclusive shelf-life policy. Model appreciation separately so either shelf-life case can have age-based pricing. Derive time-dependent condition/price from an explicit date rather than storing stale derived state.
5. **Entity/value-object boundary:** HA2 currently requires records broadly; HA9 and HA11 express incompatible namespace-based assumptions for entity identity and value-object records. Resolve this assessment design before broad model migration.

## Candidate implementation sequence

1. Pick a narrow expected-outcome flow. Colleague candidates include lookups that currently throw generic exceptions and `StoredProduct.Withdraw`, which throws for insufficient stock. For lookups, decide whether absence is an ordinary Maybe/Option or a named OneOf case based on caller semantics.
2. Give `Withdraw` a specific `InsufficientStock` outcome and let sale orchestration propagate it if that is the chosen first domain example. Keep exceptions for unexpected failures/invariant violations.
3. Replace `Product.Status` strings with typed alternatives.
4. Represent price calculations as an ordered immutable sequence of adjustments (Margin, optional TransportationFee, VAT) and preserve VAT-on-margin semantics.
5. Consider typed domain events for notifications after core outcomes are explicit.

Further possibilities (image contexts, discounts, region/currency value objects) need domain justification before inclusion. Potential future harness work could detect status-like string comparisons or generic exceptions for expected business outcomes; it is not part of the current HA3 change.

## Current decisions and blockers

- **HA3 implementation:** semantic check for the real OneOf package type; record inheritance alone no longer passes. Domain source still needs actual OneOf use to pass HA3.
- **Lookup example:** `ProductLifecycleService.GetProduct` currently uses `FirstOrDefault` and throws a generic exception. A Maybe/Option result plus `Map` demonstrates absence-safe chaining; lookup→sale could instead be a OneOf outcome of success, `ProductNotFound`, and `InsufficientStock`.
- **Shelf-life:** cases are `Perishable` and `NonExpiring`; still define sell-by/expiration meaning, precedence, and sale/discount effects.
- **Appreciation:** separate from shelf-life; formula and cap remain undecided.
- **Blocker:** clarify HA2/HA9/HA11's distinction between identity-bearing entities and immutable value objects before applying their rules to all `Models.*` types.

### Design (proposed, 2026-10-04): Perishable / NonExpiring on Product (HA4: composition, no subclassing)
- `Product` keeps one class; new immutable `ShelfLife` property = `OneOf<Perishable, NonExpiring>` (C# alias; do NOT use `OneOfBase<>`, it is inheritance and breaks HA4).
- Records in ValueObjects: `Perishable(MaxSellingDate, MaxConsumptionDate)` with invariant selling <= consumption via `Create` returning `OneOf<Perishable, InvalidDates>`; `NonExpiring(BestBeforeDate)`.
- `Product.CanSell(today)` -> `OneOf<Sellable, PastSellingDate>`: Perishable blocks after MaxSellingDate; NonExpiring never blocks (BBD only affects the display label).
- Expiry is computed from the date, not stored in `ProductStatus`. Inject `TimeProvider`/`today` instead of `DateTime.Now`.
- `SellProduct` checks `CanSell` before `Withdraw`; `AddProduct` takes a `ShelfLife`; `GetDisplayLabel` matches on it.
- Open: (1) "MaxSellingRate" read as MaxSellingDate — confirm; (2) dates per product vs per stock lot (`StoredProduct`) — decide before Step 4.
- Naming suggestion (2026-10-04, not confirmed): `Perishable(SellByDate, UseByDate)` and `NonExpiring(BestBeforeDate)` instead of `MaxSellingRate/MaxConsumptionDate/BBD` — drop `Max` on dates, use food-labelling terms (use-by = safety, best-before = advisory), no `BBD` abbreviation in code. `ShelfStable` is an alternative to `NonExpiring`. Validate with the domain owner.
- **Done 2026-10-04 (uncommitted):** HA4 explanation + participant exercise (Perishable/NonExpiring, ShelfLife via `OneOf<Perishable, NonExpiring>`, no `OneOfBase`) written into `OmniProduct-CoreDomain/CUPID-step1-2.md`. It uses the suggested names `SellByDate/UseByDate/BestBeforeDate` and `PastSellByDate` — still unconfirmed. Not yet enforced by the harness: HA4 check for `OneOfBase` and HA3 alias recognition need the harness updated.
