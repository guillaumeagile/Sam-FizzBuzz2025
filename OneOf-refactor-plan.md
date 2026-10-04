# OneOf refactor plan

**Resume at:** Step 3 — define the shelf-life ADT as Perishable vs NonExpiring and decide date semantics; keep age-based appreciation as a separate, composable pricing policy.

## Status

- [x] **2026-10-04 — checked** the workshop narrative, `CUPID-step1-2.md`, model classes, their service/test call sites, and the relevant harness rules.
- [x] **2026-10-04 — baseline commit:** `f9b59ed` (`Fan out rule updated  to strcite`).
- [x] **2026-10-04 — package installed:** `OneOf` 3.0.271 was added to `OmniProduct-CoreDomain.csproj`, restored, and the domain project built. Package and HA3 changes committed as `f5a14ff` (`Align HA3 with OneOf discriminated unions`). Existing build warnings: NU1903 for SQLitePCLRaw.lib.e_sqlite3 2.1.10 and nullable-property warnings.
- [x] **2026-10-04 — decision:** per HA3's goal of representing closed alternatives for exhaustive matching, use OneOf as the union representation rather than an abstract-record hierarchy. HA4's no-inheritance rule remains intact.
- [x] **2026-10-04 — package checked:** NuGet package `OneOf`, stable version `3.0.271`; NuGet describes it as a discriminated union with exhaustive `.Match(...)` and lists .NET 10 compatibility. Package page: https://www.nuget.org/packages/OneOf.
- [x] **2026-10-04 — HA3 aligned with OneOf:** replaced the abstract-record hierarchy check with a semantic check for the actual `OneOf.OneOf<T...>` package type; added the package reference/metadata reference to the harness; updated the step guide and harness-script description. Pinned tests cover imported and fully qualified usage, a lookalike type, a record hierarchy without OneOf, and no union. `dotnet test Cupid.Harness.Test/Cupid.Harness.Test.csproj --nologo` passed (66/66). Committed as `f5a14ff`.
- [x] **2026-10-04 — chainability recommendation:** keep OneOf for closed alternatives, not sequencing. For identity-bearing, mutable entities, prefer explicit domain commands (`void` or a domain result) to avoid fluent calls obscuring state transitions. If fluent chaining is an intentional API goal, entity methods may return the same entity (`this`) after enforcing invariants. Use function composition/`Pipe` for immutable transformations, and builders for creation-time setup; use application services to orchestrate operations crossing domain concerns. This is a recommendation, not an implementation decision.
- [x] **2026-10-04 — refined null-free result decision:** use a Maybe/Option type for an optional lookup (`Product` found or absent); absence alone is not an operation error and need not carry a separate error case. Reserve `OneOf` for closed operations with distinct success/failure alternatives, such as sale success, product not found, or insufficient stock. Compose each with `Map`/`Bind` as appropriate; do not silently convert unexpected exceptions.
- [x] **2026-10-04 — evident example selected:** start with `ProductLifecycleService.GetProduct`, whose `FirstOrDefault` can yield null and which currently turns that into a generic exception. Change its return to a Maybe/Option of `Product`, then chain `Map(product => product.Name)` and explicitly match/handle present vs absent. For lookup followed by a command such as sale, use an operation result union with `ProductNotFound` and `InsufficientStock` cases instead of treating every step as Maybe. Do not include supplier notification/network/persistence failures in the first example.
- [x] **2026-10-04 — domain scenario added:** products can have shelf-life dates and age-dependent appreciation. These are independent capabilities, not mutually exclusive product kinds: an age-appreciating product may also be perishable. Model shelf-life policy and pricing policy as separate values, each with explicit cases; derive current condition/price from policy plus an injected `asOf` date rather than storing nullable state or stale calculated state.
- [x] **2026-10-04 — shelf-life alternatives clarified:** every product has one of two explicit shelf-life cases: `Perishable` with its applicable date data, or `NonExpiring`. Age-based appreciation remains a separate policy and may coexist with either case.
- [ ] **Next:** define perishable date semantics (sell-by vs safety expiration and sale/discount consequences) and age-pricing formula/cap; then select a Maybe/Option representation and test lookup chaining. Before broadening records/value cases in `Models.*`, resolve HA2/HA9/HA11's entity-versus-value-object boundary.

## Proposed steps

1. **Agree on the domain shapes.** 

Use OneOf for finite alternatives, not as a replacement for every model/entity. Candidate uses are product lifecycle states (`Active`, `OutOfStock`, `Deprecated`) and price-adjustment cases (`Margin`, `TransportationFee`, `VAT`). 
 
A price can contain an ordered immutable sequence of adjustment cases, preserving composition; the cases are not mutually exclusive across the whole calculation. Keep identity-bearing entities such as `Product`, `Supplier`, `Warehouse`, and `StoredProduct` as entities rather than manufacturing union cases for them.

Represent the shelf-life distinction as a closed ADT: `OneOf<Perishable, NonExpiring>`. Every product has exactly one of these cases; `Perishable` carries its agreed date data and `NonExpiring` carries no expiration data. Keep age-based appreciation as a separate pricing policy that may apply to either shelf-life case. Derive time-sensitive condition and price from the policy plus an explicit date. Define date consequences before implementation.
2. **Align the assessment contract before implementation.** (HA3 complete; entity/value-object scope remains pending.)

HA3 currently requires an abstract-record hierarchy, while HA4 rejects inheritance. The decision is to keep HA4's composition/no-inheritance intent and change HA3 to recognize OneOf's closed union representation, which serves HA3's exhaustive closed-alternative goal without inheritance. Update HA3's documentation and pinning tests intentionally; do not weaken HA4.

The separate HA2/HA9/HA11 boundary still needs resolution: entity types need identity and should not be forced to be value-object records, while value objects should remain immutable records and must not implement `IDentifiable`. Update the assessment scope/tests to make that distinction explicit rather than applying entity and value-object requirements indiscriminately to every type in `Models.*`.
3. **Add `OneOf` 3.0.271 to `OmniProduct-CoreDomain`** (package complete)
 
and define the smallest domain-owned union/value-case types in a namespace that reflects their value semantics, not entity identity. Use a Maybe/Option representation for optional absence, and OneOf for named operation outcomes with multiple distinct cases. Add narrowly scoped `Bind`/`Map` helpers only where they make successful-path chaining clearer, and verify absent values or failures propagate without being swallowed.
4. **Migrate product lifecycle state.** 

Replace the free-form `Product.Status` string with the agreed union, update sale/deprecation transitions and catalog filtering/labels, and add tests for each case and transition.
5. **Migrate price adjustments.** 

Replace embedded price-rule details with immutable adjustment cases and an ordered calculation pipeline. Preserve the specified order (margin, optional transportation fee, then VAT applied only to margin) with focused pricing tests.
6. **Update consumers and verify.** 

Adapt services and tests to exhaustive OneOf matching; run domain tests, harness pinning tests, and the applicable step harnesses. Update the workshop step documentation to match the final contract.

## Decision and pending assessment work

- **Decision:** HA3's stated goal is a closed ADT/pseudo-union with exhaustive matching. OneOf represents that union directly without inheritance, so use OneOf for HA3 and preserve HA4's no-inheritance rule. Do not convert every class in `Models` into a union.
- **Chainability guidance:** OneOf models alternatives, not operation chains. Keep entity transitions explicit by default; only return `this` for fluent mutation if that API is an explicit design requirement. Prefer composable functions for immutable values and services for cross-entity orchestration.
- **Null-free chaining decision:** use Maybe/Option for a value that may be absent and OneOf for explicit, distinct operation outcomes. Compose using appropriate `Map`/`Bind` operations so absence or failures short-circuit. Do not use `null` as an outcome or turn unexpected exceptions into success-shaped values; only add the minimal composition helpers needed by the domain.
- **Recommended first demonstration:** replace `GetProduct`'s nullable lookup/generic exception with a Maybe/Option result and chain `Map` to a property, explicitly handling present vs absent. For a later lookup→sale chain, return a OneOf result distinguishing success, `ProductNotFound`, and `InsufficientStock`.
- **Perishable/ageing product modeling:** shelf-life is exactly one of `Perishable` or `NonExpiring`; represent it as a closed OneOf ADT. Age-appreciation is a separate pricing policy and may coexist with either shelf-life case. Calculate condition and price from an explicit `asOf` date; avoid nullable policy objects and stored time-derived states. Business decisions still needed: meaning and precedence of sell-by vs expiration dates, effects on sale/discount, and appreciation formula/cap.
- **HA3 implementation:** HA3 now checks semantic use of the `OneOf` package type; record inheritance no longer satisfies it. HA4 continues independently to prohibit inheritance. The domain source currently has no OneOf use, so its HA3 check will remain failing until a real domain flow is migrated. Package and assessment changes are in commit `f5a14ff`.
- **Pending:** resolve HA2/HA9/HA11's entity-versus-value-object scope. Domain-model migration should wait until HA3's representation is pinned by tests and the assessment classification is made coherent.
