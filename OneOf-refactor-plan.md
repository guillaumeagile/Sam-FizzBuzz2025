# OneOf refactor plan

**Resume at:** Step 1 — agree on the domain variants and align the harness contract before changing models.

## Status

- [x] **2026-10-04 — checked** the workshop narrative, `CUPID-step1-2.md`, model classes, their service/test call sites, and the relevant harness rules.
- [x] **2026-10-04 — baseline commit:** `f9b59ed` (`Fan out rule updated  to strcite`).
- [ ] **Next:** decide which concepts are genuine closed alternatives and resolve the harness conflicts below.

## Proposed steps

1. **Agree on the domain shapes.** 

Use OneOf for finite alternatives, not as a replacement for every model/entity. Candidate uses are product lifecycle states (`Active`, `OutOfStock`, `Deprecated`) and price-adjustment cases (`Margin`, `TransportationFee`, `VAT`). 
 
A price can contain an ordered immutable sequence of adjustment cases, preserving composition; the cases are not mutually exclusive across the whole calculation. Keep identity-bearing entities such as `Product`, `Supplier`, `Warehouse`, and `StoredProduct` as entities rather than manufacturing union cases for them.
2. **Align the assessment contract before implementation.**

HA3 currently requires a source-level abstract-record hierarchy, while HA4 rejects inheritance.

HA2 says domain classes must be records; HA9 requires every type of Entities (meaning having a unique Identifier, and not being a Value Object) under `Models.*` to implement `IDentifiable`; 

HA11 forbids records from implementing it. Decide the intended scope and update the step documentation and pinned harness tests only as an intentional rule change. 

For OneOf, HA3 should recognize the selected OneOf representation rather than require a record hierarchy; keep HA4's no-inheritance intent.
3. **Add OneOf to `OmniProduct-CoreDomain`**
 
and define the smallest domain-owned union/value-case types in a namespace that reflects their value semantics, not entity identity. Confirm the package/API and immutable collection representation as part of this step.
4. **Migrate product lifecycle state.** 

Replace the free-form `Product.Status` string with the agreed union, update sale/deprecation transitions and catalog filtering/labels, and add tests for each case and transition.
5. **Migrate price adjustments.** 

Replace embedded price-rule details with immutable adjustment cases and an ordered calculation pipeline. Preserve the specified order (margin, optional transportation fee, then VAT applied only to margin) with focused pricing tests.
6. **Update consumers and verify.** 

Adapt services and tests to exhaustive OneOf matching; run domain tests, harness pinning tests, and the applicable step harnesses. Update the workshop step documentation to match the final contract.

## Decision and blocker

- **Proposed decision, not yet confirmed:** model only genuine alternatives with OneOf; do not convert every class in `Models` into a union.
- **Blocker:** the existing HA2/HA9/HA11 classification and HA3/HA4 requirements conflict with one another and with OneOf's non-inheritance union representation. Agree on the desired assessment semantics before changing the exercise models or harness.

## Findings 2026-10-04 — where else ADTs can emerge (analysis only, nothing decided, no code changed)

Representation options for HA3 (to pick in Step 1):
- **A. OneOf library** (struct wrapper, no inheritance) — the plan's current direction; satisfies HA4, needs HA3 rewritten to recognise `OneOf<...>`.
- **B. Abstract record + sealed leaf records** — what HA3 requires today; conflicts with HA4 unless HA4 exempts a closed hierarchy (private ctor, sealed leaves).
- **C. C# 15 `union` keyword** — removes the HA3/HA4 conflict natively; check toolchain/LangVersion availability before relying on it.
- **D. enum** — only for payload-less cases; not a real ADT.

Candidates found in the current code (smell -> union):
1. `Product.Status` string + literal compares in `ProductLifecycleService.GetActiveProducts` and `ProductCatalog.GetDisplayLabel` -> `ProductStatus` (Active | OutOfStock | Deprecated). Already planned (Step 4).
2. `Price.Margin/Vat` + hard-wired order in `GetResellerPrice` -> `PriceAdjustment` (Margin | TransportationFee | Vat), ordered immutable list. Already planned (Step 5).
3. Every `Get*/Find*` in Services throws `Exception` for an expected case (Pricing, Supplier x2, Storage x2, Catalog, Lifecycle) -> `OneOf<T, NotFound>`. Services-only, no model change; cheapest first slice.
4. `StoredProduct.Withdraw` throws "Not enough stock"; `Product.Sell(int remainingStock)` branches on `== 0` -> `Withdraw` returns `OneOf<Withdrawn, InsufficientStock>`; the sale outcome drives the status transition instead of re-deriving it from stock.
5. `Notification` (free-form Subject/Body strings, built inline in `SellProduct`/`DeprecateProduct`) -> domain events `ProductEvent` (Sold | Deprecated); NotificationService renders them per audience (supplier vs customers).
6. Weaker/speculative: `ProductCatalog.Images` keys ("thumbnail","hero") -> closed `ImageContext`; `Discounts` List<string> -> `Discount` union; `Region`/`Currency` strings -> value objects (not ADTs).

Suggested order: 3 -> 4 -> 1 -> 2 -> 5. Possible new harness rule: flag string-literal comparisons on status-like members and `throw new Exception` for expected domain outcomes.

**Pending decision for the user:** representation A/B/C, and whether to take 3-5 into this refactor or keep scope to 1-2.

### Decision (proposed, 2026-10-04): Result / Option
- No extra library: Result = `OneOf<T, SpecificError>`, Option = `OneOf<T, NotFound>` (or `T?` when forcing the match is not needed). HA3 must recognise OneOf + OneOf.Types.
- Result for expected outcomes (`Withdraw`, lookups) with specific error cases, not generic `Result<T,string>`; exceptions stay for invariant violations.
- Option is low priority; optional fee/VAT = absent from the adjustment list, not an Option.
- Watch: chaining in orchestrators (`AddProduct`, `SellProduct`) may need small Map/Bind extensions.
- Still pending: representation A/B/C and scope (items 3-5).
