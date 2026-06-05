# CUTE DDD Learning Path: From Legacy Monolith to Domain Excellence

## Overview

This learning path transforms a legacy object-oriented codebase into a CUTE DDD-compliant domain model through **3 major shifts**, teaching one concept at a time.

The approach mirrors the FizzBuzz exercise: start with bad design, refactor step by step, let better design emerge from the domain.

### Starting Point: Legacy OmniProduct Monolith

- All concerns bundled in a single `Product` entity (catalog, transport, stock, suppliers, pricing)
- No domain language — just CRUD models with ORM annotations
- Anemic domain model: entities as data bags, all logic in services
- Null-heavy code, exceptions as control flow
- No bounded contexts: everything is coupled to everything

### End Goal: CUTE DDD Domain Model

- Bounded contexts with clear responsibilities
- Ubiquitous language that bridges business and code
- Testable domain model (unit-testable without infrastructure)
- Expressive types that make illegal states unrepresentable

---

## CUTE DDD Principles (reminder)

#### **C** - **Contextual**
Design decisions based on specific domain context — each bounded context has its own model.

#### **U** - **Ubiquitous**
Shared language understood by all stakeholders — code reads like the business speaks.

#### **T** - **Testable**
Easy to test at all levels — the domain model does not depend on the ORM.

#### **E** - **Expressive**
Clearly communicates business intent — types and names reveal what the domain rules are.

---

## SHIFT 1: From SOLID to CUPID — Fix the OO Design

**Goal**: Before touching the domain, fix the foundational OO problems in the legacy code.
This is the same journey we took with FizzBuzz — but applied to a real, messier codebase.

### Step 1.1: Single Responsibility → Unix Philosophy
**"One class, one reason to change"**

**Problem**: `ProductService` handles catalog, pricing, stock, supplier notification, and transport — all at once.

**Plan**:
- Identify every distinct concern currently living in `ProductService` and `Product`
- Extract each concern into its own class with a single focus
- Use the domain vocabulary to name each class (not `ProductCatalogManager` — just `Catalog`)

**Learning Objective**: Domain concerns are not technical layers. Each concern should map to a word the business actually uses.

---

### Step 1.2: Open/Closed → Composable
**"Extend through composition, not modification"**

**Problem**: Adding a new supplier or a new pricing region requires modifying existing classes. Pricing rules, regions, and supplier logic are all hardcoded.

**Plan**:
- Identify every place where adding a new business rule requires modifying existing code
- Replace hardcoded logic with configurable, injectable collaborators
- Design so that a new region or a new pricing rule is added without touching existing classes

**Learning Objective**: Composability is a prerequisite for working with a complex domain. If you can't add a supplier without breaking existing suppliers, the design is wrong.

---

### Step 1.3: Hidden Complexity → Predictable Behavior
**"No surprises. Behavior should be obvious from the structure."**

**Problem**: The legacy code is full of implicit rules — status transitions happen silently, notifications fire as side effects of unrelated operations, ORM lazy-loading triggers unexpected queries.

**Plan**:
- Map every implicit behavior (side effects, hidden state transitions, magic flags)
- Make each one explicit: named method, clear precondition, visible consequence
- Replace implicit null checks with explicit absence modeling

**Learning Objective**: Predictable code is code you can reason about without running it. The domain model should make the business rules visible.

---

### Step 1.4: Old Patterns → Idiomatic Modern Code
**"Code should feel natural in the language and the domain"**

**Problem**: The legacy code uses patterns from circa-2010 Java/Spring: mutable public setters, service locators, string-typed status fields, DTOs leaking into the domain.

**Plan**:
- Identify patterns that are idiomatic in the old style but actively harmful in a domain model
- Replace string-typed statuses with proper types
- Replace public setters with explicit domain methods
- Replace service locator lookups with constructor injection

**Learning Objective**: Modern idioms reduce accidental complexity. They let the domain speak without fighting the language.

---

## SHIFT 2: Strategic DDD — Find the Bounded Contexts

**Goal**: Understand the domain before redesigning it. No keyboard — think and talk first.

---

### Step 2.1: Event Storming — Let Events Emerge
**"What happens in this domain? Don't start with classes."**

**Plan**:
- Run Event Storming on the new feature stories (multiple suppliers per region, reseller pricing, catalog for frontend, deprecated products)
- Write domain events on sticky notes: orange = event, blue = command, yellow = actor
- Look for clusters of events that belong together and are owned by the same actor

**Stories to storm**:
- A supplier adds a product to the catalog, then ships it into stock
- A product is sold: supplier is advised, stock is updated
- A product is deprecated: supplier advised, stock cleared, customer advised, label changed
- A reseller configures a regional margin; frontend displays supplier price + margin + VAT on margin only

**Learning Objective**: Events reveal what the domain cares about. They expose boundaries naturally. Two teams owning different events likely belong to different bounded contexts.

---

### Step 2.2: Domain Storytelling — Name the Contexts
**"Draw the domain before you model it"**

**Plan**:
- Tell domain stories in pictographic language (actor → verb → object)
- Let the stories reveal which concepts belong together and which are foreign to each other
- Name the bounded contexts using words from the stories, not from the database schema

**Contexts expected to emerge**:

| Context | Responsibility | Core concepts |
|---|---|---|
| **Catalog** | What products exist and how they are described | Product, Slug, Image, Label |
| **Pricing** | How products are priced per region and reseller | SupplierPrice, Margin, VAT, ResellerPrice |
| **Stock** | What is available and in what quantity | StockEntry, Quantity |
| **Supplier** | Who provides products and under what terms | Supplier, ProductOffer, Region |
| **Notifications** | Who gets told when something changes | Subscriber, Event |

**Key insight**: Each context has its own model of "Product". Catalog's Product has a Slug. Pricing's Product has a price. Stock's Product has a quantity. They are not the same class.

**Learning Objective**: The ubiquitous language is not universal — it is local to a bounded context. That is not a weakness; it is the design.

---

### Step 2.3: Context Map — Draw the Relationships
**"Contexts talk to each other, but don't share their internals"**

**Plan**:
- Draw the relationships between bounded contexts: who sends events to whom, who translates what
- Identify the integration patterns: shared kernel, anti-corruption layer, open host service
- Locate the root problem in the legacy code: a single `Product` table with foreign keys to everything

**Key design decision**:
- Contexts communicate through domain events, not shared database tables
- Each context owns its own read model of what it needs from other contexts
- An Anti-Corruption Layer translates between contexts so each can evolve independently

**Learning Objective**: Context mapping makes dependencies explicit and intentional. The goal is loose coupling between contexts, strong cohesion within.

---

## SHIFT 3: Tactical DDD — Build the Domain Model

**Goal**: Apply DDD building blocks to make the domain expressive and testable inside each bounded context.

---

### Step 3.1: Primitive Obsession → Value Objects
**"If the domain has a concept, give it a type"**

**Problem**: `Product` has `decimal Price`, `decimal Margin`, `string Slug`, `string Region` — all primitives, all ambiguous, all unvalidated.

**Plan**:
- Identify every primitive that represents a domain concept
- Create a value object for each: immutable, self-validating, named after the domain concept
- Value objects encode their own rules — a `Slug` is always valid by construction, a `Money` always has a currency

**Learning Objective**: Value objects eliminate entire categories of bugs by making illegal states unconstructable. They also make the domain language explicit in the type system.

---

### Step 3.2: Anemic Model → Rich Aggregates
**"Put the behavior where the data is"**

**Problem**: `ProductService.Deprecate()` sets a field, fires notifications, and updates stock — all in one service method. The `Product` entity is just a data bag.

**Plan**:
- Move business behavior into the aggregate root
- The aggregate enforces its own invariants (e.g., cannot deprecate twice)
- The aggregate emits domain events instead of calling services directly
- The service becomes a thin orchestrator that dispatches events

**Learning Objective**: Aggregates are the unit of consistency. Business rules live in the domain, not in services.

---

### Step 3.3: Too Much Logic in Constructor → Factory Methods + IoC
**"Separate construction from validation, and validation from creation"**

**Problem**: The `Product` constructor calls a slug library, validates fields, sets defaults — it does too much and is impossible to test in isolation.

**Plan**:
- Extract construction into a factory method with explicit collaborators
- Inject external dependencies (slug generator, id generator) through interfaces
- The constructor only assigns validated values — no logic, no calls to external services

**Learning Objective**: Factories make construction explicit and testable. Injecting collaborators via interface keeps the domain model independent of infrastructure.

---

### Step 3.4: Nulls and Exceptions → Always-Valid Model
**"Make illegal states unrepresentable"**

**Problem**: The codebase returns null for missing products, throws exceptions for business rule violations, and uses string status fields that can hold any value.

**Plan**:
- Replace null returns with a result type that makes absence explicit (same Either pattern as FizzBuzz's `RuleResult`)
- Replace string statuses with proper enumerated types or state objects
- Replace exception-as-flow-control with domain-level result types

**Learning Objective**: An always-valid model means the compiler enforces the business rules. If the code compiles and the object exists, it is in a valid state.

---

### Step 3.5: ORM Entities Exposed → Ports and Adapters
**"The domain must not know about the database"**

**Problem**: The `Product` entity has ORM annotations, navigation properties, and foreign keys — infrastructure concerns embedded in the domain model.

**Plan**:
- Define repository interfaces in the domain project (ports) — no ORM references
- Implement those interfaces in an infrastructure project (adapters) — knows about ORM
- The domain is tested with in-memory adapters; the ORM is one possible implementation
- DTOs for persistence are separate from domain objects

**Learning Objective**: Hexagonal Architecture decouples the domain from infrastructure. The test pyramid becomes possible: unit tests for domain logic, integration tests for adapters only.

---

## The Thread Through All Three Shifts

The same principles reappear at each level:

| FizzBuzz concept | SHIFT 1 (OO Fix) | SHIFT 2 (Strategic) | SHIFT 3 (Tactical) |
|---|---|---|---|
| Single responsibility | One class per concern | One context per domain area | One aggregate per invariant |
| Composable | Injectable rules | Events instead of shared DB | Value objects compose into aggregates |
| Predictable | No hidden priority | Explicit context boundaries | No nulls, no hidden transitions |
| Idiomatic | Modern C# syntax | Domain language in code | Types named after domain concepts |
| Either monad | — | Events as outcomes | Result types for queries |

---

## Progression Summary

| Shift | Step | What Changes | CUTE Principle |
|---|---|---|---|
| 1 — OO Fix | 1.1 | One class per concern | Contextual |
| 1 — OO Fix | 1.2 | Composable by design | Contextual |
| 1 — OO Fix | 1.3 | Explicit behavior | Expressive |
| 1 — OO Fix | 1.4 | Modern idioms | Ubiquitous |
| 2 — Strategic | 2.1 | Events reveal boundaries | Contextual |
| 2 — Strategic | 2.2 | Contexts named from stories | Ubiquitous |
| 2 — Strategic | 2.3 | Relationships made explicit | Contextual |
| 3 — Tactical | 3.1 | Primitives → Value Objects | Expressive |
| 3 — Tactical | 3.2 | Anemic → Rich Aggregates | Testable + Expressive |
| 3 — Tactical | 3.3 | Constructor → Factory + IoC | Testable |
| 3 — Tactical | 3.4 | Nulls → Result Types | Expressive |
| 3 — Tactical | 3.5 | ORM → Ports & Adapters | Testable |

---

## Teaching Strategy (4Cs)

### C1 — Connections (15 min)
- Show the legacy `Product` class with all concerns mixed
- Ask: "How many reasons does this class have to change?"
- Ask: "Which team owns this class?"
- Connect to FizzBuzz: "We've done this before — same problem, bigger domain"

### C2 — Concepts (chunked, one shift at a time)
- Shift 1: stay in OO — fix the mess before redesigning it
- Shift 2: no keyboard — think and talk first
- Shift 3: one tactical building block per step

### C3 — Concrete Practice
- Mob programming on the OmniProduct-CoreDomain project
- One step at a time, tests first
- Test pyramid: unit tests for domain, integration tests for adapters only

### C4 — Conclusions
- Compare starting `Product` class to the final domain model across three contexts
- Which CUTE principle did each shift serve?
- What would the fourth shift be?
