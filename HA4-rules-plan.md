# HA4 Roslyn rules plan

**Resume at:** HA4.4 (no `is`/`as`/`switch` on a `OneOf` alternative outside `.Match`; agree the exact wording with the user first, then test-first). HA4.1, 4.2, 4.3, 4.5 done (4.4 skipped for now); committed locally, not pushed since `1c124fa`.

Source of truth for the exercise: `OmniProduct-CoreDomain/CUPID-step1-2.md` (HA4 section, 7 steps).
Rules live in `Cupid.Harness/Rules`, tests in `Cupid.Harness.Test` (use `RuleTestHarness.Compile`), registered in `Cupid.Harness/Program.cs` (`step1Dot2Rules`).

## Already covered (HA4.0)

`NoInheritanceRule` (id `HA4`) flags any class/record with a base type other than `object`/Exception/Attribute/DbContext. `OneOfBase<...>` is already caught (it is a class). Interfaces are fine. No work needed; the 7 rules below are additive.

## Proposed split: one rule per exercise step, each verifiable on its own

| Rule | Exercise step | What the analyzer checks | Pass/fail tests (sketch) |
|---|---|---|---|
| HA4.1 | 1. Value objects | (relaxed 2026-10-09) At least one VO exists and every VO is immutable. No type names pinned. VO definition pending, recommended: record/record struct not implementing `IDentifiable` | no VO at all; VO with setter; VO with mutable collection; valid VO |
| HA4.2 | 2. One shelf-life property | (relaxed 2026-10-09) `Product` has at least one property of type `OneOf<X, Y, ...>` (corrected 2026-10-09: at least one, not exactly one), no setter (get-only or init) on each such property. Name not pinned. Optional: type arguments must be VOs | none; setter; `OneOfBase`; one valid; two valid |
| HA4.3 | 3. Validate at boundary | (heuristic, 2026-10-10) at least one VO has a static `Create` or `Build` returning `OneOf<Self, E>` where `E` implements `IValidationError` | no factory; returns VO directly; E not IValidationError; OneOf of another type; other name; IDentifiable record |
| HA4.4 | 4. Behaviour behind `Match` | `Product.CanSell(...)` returns `OneOf<Sellable, PastSellByDate>`; no `is`/`as`/`switch` on `Perishable`/`NonExpiring` anywhere (use `.Match`) | `is Perishable` check; switch on kind; Match used |
| HA4.5 | 3 (follow-up). No throw in constructors | (replaces the old "no expiry in ProductStatus" idea, 2026-10-10) no `throw` statement, `throw` expression or `ThrowIf*` call inside any constructor | throw stmt; `?? throw`; `ThrowIfNull`; record ctor; clean ctor; throw in method; Create factory |
| HA4.6 | 6. No clock in the model | No `DateTime.Now/UtcNow/Today`, `DateOnly.FromDateTime(...Now)`, `DateTimeOffset.Now/UtcNow` in `Models`/`ValueObjects` | `DateTime.Now` in Product; `today` parameter |
| HA4.7 | 7. Services wired | `SellProduct` calls `CanSell` before `Withdraw`; `AddProduct` takes a `ShelfLife` parameter; `GetDisplayLabel` uses `ShelfLife.Match` and mentions "[BEST BEFORE PASSED]" | withdraw without CanSell; AddProduct without ShelfLife |

Self-check ("third kind `Frozen` only needs a new record + one more `OneOf` type") is not a separate rule: HA4.4's no-`is`/`as` check is what makes it hold.

## Decisions

- **2026-10-10:** user asked to add "no exception thrown in constructors" as HA4.5 if no rule covers it; none did. It replaces the previous HA4.5 idea (no expiry in `ProductStatus`), which is dropped. HA4.6 (no clock) and HA4.7 (services) keep their numbers.

- **2026-10-10:** HA4.3 error relation: `E` *implements* `IValidationError` (interface), not a base class, because HA4 forbids inheritance (user chose the interface option). The error types live in the domain under `Errors/` (user: "a special place for error representation"). This is the one addition to the otherwise untouched failing domain.

- **2026-10-09:** the domain source (`OmniProduct-CoreDomain`) is intentionally failing the rules: do NOT modify it. Verify locally with `dotnet test` and `dotnet run --project Cupid.Harness -- --step 1.2 OmniProduct-CoreDomain` (user allows local dotnet; this overrides the cloud-verification default for this work).

- **2026-10-09 DECIDED:** VO = `record`/`record struct` that does not implement `IDentifiable` (option B, structural, no namespace). HA4.2 also requires every `OneOf` type argument of those properties to be a VO.
- **2026-10-09:** HA4.2 is "at least one" OneOf property on `Product`, not exactly one.
- **2026-10-09:** user asked HA4.1 and HA4.2 to be less restrictive (no pinned type/property names). Open question: how to detect a VO. Existing rules: HA9 = types under `Models.*` implement `IDentifiable`; HA11 = records never implement it; `Price` sits in `ValueObjects` folder but HA11 looks for it under `Models` (known HA9/HA11 conflict). Options: A namespace, B structural (record not IDentifiable, recommended), C marker interface/attribute. Resolved: B.
- HA4.3-HA4.7 still pin names (`Create`, `InvalidDates`, `CanSell`, ...); to be relaxed the same way after the VO decision.
- **2026-10-09:** the user wrote "split HA7"; read as HA4 (the step list is HA4's). Split proposed, not yet confirmed.
- **2026-10-09:** go slow, one rule at a time, test-first (red test, then rule, then green), each verified before the next.
- Names `SellByDate/UseByDate/BestBeforeDate/PastSellByDate/Sellable/InvalidDates` are still unconfirmed with the domain owner (see `OneOf-refactor-plan.md`). The rules pin them, so confirm before HA4.1.

## Status

- [x] **2026-10-09 — analysed** HA4 exercise, existing `NoInheritanceRule`, test harness, Program.cs registration. No code changed.
- [ ] Confirm the 7-rule split and the names above.
- [ ] HA4.1 — **done 2026-10-09, verified locally** (uncommitted; `dotnet test Cupid.Harness.Test` 75/75 passed = 66 + 9 new; harness on `OmniProduct-CoreDomain --step 1.2` gives `[PASS] HA4.1` because `Price` is an immutable record VO): `Cupid.Harness/Rules/ValueObjectsAreImmutableRule.cs` (id `HA4.1`, `IsValueObject` internal helper reusable by HA4.2), 9 tests in `Cupid.Harness.Test/ValueObjectsAreImmutableRuleTests.cs`, registered in `Program.cs` `step1Dot2Rules`. 
- [x] HA4.2 — **done 2026-10-09, verified locally**: `Cupid.Harness/Rules/ProductHasOneOfPropertyRule.cs` (id `HA4.2`; type named `Product`, at least one real `OneOf<...>` property, no `set`, type args are VOs; `OneOfBase` subclass does not count), 9 tests in `Cupid.Harness.Test/ProductHasOneOfPropertyRuleTests.cs`, registered in `Program.cs`. `dotnet test` 84/84. On the real domain `[FAIL] HA4.2` (Product has no OneOf property), which is the intended state.
- [x] HA4.3 — **done 2026-10-10, verified locally**: `Cupid.Harness/Rules/ValueObjectFactoryRule.cs` (id `HA4.3`), 9 tests in `ValueObjectFactoryRuleTests.cs`, registered in `Program.cs`. `dotnet test` 93/93. Real domain: `[FAIL] HA4.3` (no factory yet), intended. Added to the domain in a dedicated `OmniProduct-CoreDomain/Errors/` folder: `IValidationError` (interface, `string Message`) and `record ValidationError(string Message) : IValidationError`. Domain builds; other rule results unchanged with or without these files.
- [ ] HA4.4
- [x] HA4.5 — **done 2026-10-10, verified locally**: checked first, no existing rule forbade throwing in constructors (HA10 only bans services in entity constructors; HA3 text only mentions it). `Cupid.Harness/Rules/NoThrowInConstructorRule.cs` (id `HA4.5`), 7 tests in `NoThrowInConstructorRuleTests.cs`, registered. `dotnet test` 100/100. Real domain: `[PASS] HA4.5` today (no constructor throws yet).
- [ ] HA4.6
- [ ] HA4.7

## Blockers

- Verification of build/tests must run via `claude --cloud` on a pushed branch with a PR (global rule). Current branch: `kanDDDinsky2026-rebirth/CUPID/step1.2-new-rules`. This plan file is Markdown-only and left uncommitted.
- Rules HA4.1 to HA4.7 pin names that do not exist in the domain code yet, so the participant exercise (not the harness) creates the types; the harness tests use in-memory snippets.
