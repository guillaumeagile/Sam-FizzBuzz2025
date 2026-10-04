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
