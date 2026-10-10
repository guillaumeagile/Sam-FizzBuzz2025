# HA12 Roslyn rules plan (step 1.1): identifiers are rich objects, ULID or UUID v7+

**Resume at:** done 2026-10-10: HA12 + HA12.1 implemented, registered in `step1Dot1Rules`, documented, 148 tests pass, committed locally (not pushed). Next only if asked: push, domain starter fix, PR.

## Decisions (user, 2026-10-10)

1. "At least v7" is checked by **shape**, not by value: type is `Guid` or `Ulid` (or a record wrapping one). Creation through `Guid.NewGuid()` (v4), `Guid.Empty`, `new Guid()` / `default(Guid)` is flagged in id context; `Guid.CreateVersion7()` / `Ulid.NewUlid()` are fine.
2. Typed wrapper required (`record ProductId(Guid Value)`). A bare `Guid`/`Ulid` passes HA12 but raises a `[WARN]` (HA12.1).
3. Scope: `IDentifiable` types and any `Id` / `*Id` property or parameter outside `Services` namespaces (model, events).
4. The domain is NOT modified (`IDentifiable.Id` stays `string`); the rule must fail against the current bad code.

## Rules

| Rule | Check | Severity |
|---|---|---|
| HA12 | every `Id`/`*Id` property or parameter has type Guid/Ulid or a record wrapping a single Guid/Ulid; no `Guid.NewGuid()`, `Guid.Empty`, `new Guid()`, `default(Guid)` in id context | error |
| HA12.1 | an `Id` typed as a bare `Guid`/`Ulid` should be wrapped in a record | warning |

## Status

- [x] (2026-10-10) tests HA12, rule HA12, tests HA12.1, rule HA12.1, register in `step1Dot1Rules`, docs (`CUPID-step1-1.md`, `cupid-step-1-1.harness.sh`, `AGENTS.md`), local verify, commit.

Real domain: `[FAIL] HA12` (string Ids in IDentifiable, Notification, Product, ProductCatalog, ...), `[PASS] HA12.1`. Helper shared in `Rules/IdentifierShape.cs`.
