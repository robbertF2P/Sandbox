---
name: domain-driven-design
description: |
  Guides domain modeling and software design following Eric Evans' "Domain-Driven Design:
  Tackling Complexity in the Heart of Software". Use when:
  - Modeling a business domain, bounded context, or aggregate boundaries
  - Choosing between Entity, Value Object, Service, Repository, or Factory
  - Designing ubiquitous language, context maps, or anti-corruption layers
  - Refactoring toward deeper insight or reviewing model/implementation alignment
  - Splitting or integrating subsystems, legacy adapters, or shared kernels
  - Reviewing whether code reflects the domain or has drifted procedural/anemic
paths:
  - "**/*.cs"
  - "**/*.csproj"
  - "**/docs/**"
metadata:
  version: 1.0.0
  source: "Eric Evans — Domain-Driven Design (Addison-Wesley, 2004)"
---

# Domain-Driven Design

Apply this skill when the **heart of the software** is solving domain problems — not when the task is purely technical (build scripts, infra, generic CRUD with no domain rules). The model is the team's **selectively simplified, rigorously organized abstraction** of domain knowledge.

Match existing project conventions (see `dotnet-core-csharp-development` for C# layout). DDD patterns are conceptual; express them in idiomatic C# (records for values, clear namespaces/modules, repositories at persistence boundary).

## Three uses of the domain model

Every modeling choice should serve at least one of these (ideally all three):

1. **Model ↔ implementation** — Model and design shape each other (Model-Driven Design). Code is the primary expression of the model; diagrams and docs are secondary.
2. **Model ↔ language** — The model is the backbone of the **Ubiquitous Language** shared by developers and domain experts.
3. **Model ↔ knowledge** — The model distills what the team agrees matters most; it evolves through continuous learning and refactoring.

If a type, name, or layer does not support these, question whether it belongs in the domain model.

## Ubiquitous Language

- Use **one language per Bounded Context**, derived from the model, in conversations, code, tests, and APIs.
- **A change in language is a change in the model** — rename classes, methods, and tests together.
- Domain experts should object to awkward terms; developers should flag ambiguity and inconsistency.
- Prefer **modeling out loud** (concrete scenarios with domain terms) over table/column talk in design discussions.

**Red flag:** developers and users discuss the same feature but one side speaks in database tables/procedures while the other speaks in business concepts.

**Good:** "When the Route Specification changes, regenerate the Itinerary only if the existing Itinerary no longer satisfies the Specification."

**Bad:** "Delete rows in `cargo_bookings`, call Routing Service, set a Boolean on Cargo."

## Model-Driven Design

- The **model and implementation must stay aligned**. Refactoring the model without updating code (or vice versa) creates a false model.
- **Hands-on modelers** — people who change code should participate in modeling; analysts-only models drift.
- **Executable bedrock** — prefer a running, tested slice over static documents.
- Explanatory models (docs, alternate diagrams) may differ from the design model but must not replace it.

## Layered architecture

Keep domain logic in a **Domain Layer** isolated from UI, application orchestration, and infrastructure.

| Layer | Responsibility |
|-------|----------------|
| **User Interface** | Display info, accept commands |
| **Application** | Orchestration, transactions, no business rules |
| **Domain** | Model, domain logic, Ubiquitous Language |
| **Infrastructure** | Persistence, messaging, external APIs |

**Anti-pattern: Smart UI** — putting all logic in the UI layer. Fast initially, impossible to maintain for complex domains.

Application services coordinate; **domain rules live in entities, value objects, and domain services**.

## Building blocks — decision guide

| Concept | Identity | Mutability | Use when |
|---------|----------|------------|----------|
| **Entity** | Yes — tracked across time and storage | Usually mutable | Same thing must be distinguishable even when attributes change |
| **Value Object** | No — defined by attributes | **Immutable** | You care *what* it is, not *which* instance |
| **Aggregate** | Root entity has global identity | Root enforces invariants | Cluster of objects changed together as one unit |
| **Domain Service** | Stateless operation | N/A | Important domain behavior that is not natural on an Entity or Value |
| **Repository** | Collection illusion for aggregate roots | N/A | Global lookup/persistence of roots |
| **Factory** | Creates complex aggregates/entities | N/A | Encapsulates creation/reconstitution invariants |

### Entities

- Identity operation must be **unique within its type** and **stable** across persistence and distribution.
- Not every table row deserves an Entity — avoid artificial identities.
- Model behavior with the data; avoid anemic data holders with all logic in services.

### Value Objects

- Form a **conceptual whole** (e.g. `Address`, not separate `Street`/`City` on `Person`).
- **Immutable** — changes are full replacement, not in-place mutation.
- No bidirectional associations between Value Objects.
- In C#: prefer `readonly record struct` or `record` with init-only properties.
- If you need to search the DB for an existing Value by properties, reconsider — you may have an unrecognized Entity.

### Aggregates

An **Aggregate** is a cluster of associated objects treated as one unit for data changes.

**Rules:**

1. The **root Entity** is the only member outsiders may hold references to.
2. External objects may **not** hold references to internal aggregate members.
3. **Invariants** spanning the cluster are enforced on the root on each mutation.
4. **One transaction** commits one aggregate — design so cross-aggregate rules use eventual consistency or domain events, not one giant lock.
5. Prefer **small aggregates** — reference other roots by identity only.

**Repository rule:** provide Repositories **only for Aggregate roots** that need direct access.

### Domain Services

Use when the operation:

- Is a **significant domain concept** (verb/activity, not a "Manager" dumping ground)
- Does not fit naturally on an Entity or Value Object
- Is **stateless** (may have side effects on passed objects, but no own lifecycle state)

Name services from the Ubiquitous Language. Parameters and results are domain objects.

Distinguish **domain services** (domain layer) from **application services** (orchestration) and **infrastructure services** (email, storage).

### Factories

Encapsulate **complex creation** or **reconstitution from persistence** when constructors would leak invariants.

- Prefer constructors when the object is simple.
- Separate **Entity factories** (identity assignment) from **Value Object factories** (validation/composition).
- Invariant logic belongs in the Aggregate root or Factory — not scattered in callers.

### Repositories

- Represent a **conceptual set** of a type — like a collection with rich queries.
- **Hide** persistence technology; interface speaks the Ubiquitous Language.
- Support add, remove, and query by domain-meaningful criteria.
- Enable **in-memory test doubles** without changing domain code.

Do **not** expose generic "get anything" query APIs that let clients bypass aggregate boundaries.

## Modules (packages/namespaces)

- Reflect **domain concepts**, not technical layers only (`Billing`, `Routing`, not `Managers`, `Helpers`).
- **Low coupling** between modules; **high cohesion** within.
- Avoid infrastructure-driven packaging that obscures the model.

## Refactoring toward deeper insight

Valuable models emerge iteratively — not from upfront big design alone.

1. Implement a **naive model**, learn from domain experts and running software.
2. **Listen to language** — new terms and awkward phrases signal missing concepts.
3. Watch for **contradictions** and **awkwardness** in the model.
4. Make **implicit concepts explicit** (constraints, processes, Specifications).
5. Pursue **breakthroughs** — occasional deep model shifts that simplify many problems at once.

### Specification pattern

When rules combine criteria and are reused or composed, model them as **Specification** objects (predicates over candidates). Keeps validation logic declarative and composable.

## Supple design (domain-friendly APIs)

Apply within the domain layer to keep models easy to use correctly:

| Pattern | Intent |
|---------|--------|
| **Intention-Revealing Interfaces** | Names state purpose, not mechanism |
| **Side-Effect-Free Functions** | Queries don't mutate state |
| **Assertions** | Make pre/post conditions explicit |
| **Conceptual Contours** | Split/combine types along natural domain seams |
| **Standalone Classes** | Low coupling — minimal dependencies |
| **Closure of Operations** | Operations return types that support further operations in the domain |

## Strategic design

For large systems, multiple teams, or legacy integration.

### Bounded Context

- Explicit boundary within which **one model and one Ubiquitous Language** apply uniformly.
- Includes model objects, persistence schema driven by the model, and applications that use it.
- **Name each context** and use names in speech ("in the Booking context…").

**Splinter symptoms inside a context:**

- Duplicate concepts (same idea modeled twice)
- **False cognates** (same word, different meaning — e.g. "Customer" in sales vs support)
- Confusion in team discussions; interfaces that almost fit but not quite

**Continuous Integration** (within one Bounded Context): frequent merge/build/test **plus** relentless Ubiquitous Language alignment. Required for teams >2 in the same context.

### Context Map

Chart all Bounded Contexts and **how they relate**. Code reuse across contexts is a **hazard** — integrate through translation.

| Relationship | When |
|--------------|------|
| **Partnership** | Two contexts, one goal — coordinate closely |
| **Shared Kernel** | Small shared model subset; joint ownership, CI essential |
| **Customer/Supplier** | Upstream/downstream teams; downstream defines needs |
| **Conformist** | Downstream adopts upstream model as-is |
| **Anti-Corruption Layer (ACL)** | Protect your model from foreign/legacy semantics |
| **Open Host Service** | Published protocol for integration |
| **Published Language** | Well-documented interchange format (e.g. XML schema) |
| **Separate Ways** | No integration — cut losses |

### Anti-Corruption Layer

When integrating with legacy or external systems whose model would corrupt yours:

1. Build a **translation layer** at the boundary (facade + adapters).
2. Interface design in **your** Ubiquitous Language inward; translate to/from external representation outward.
3. Do **not** let foreign types leak into the domain layer.

### Distillation (overview)

Focus effort on the **Core Domain** — what gives competitive advantage. Generic subdomains can be bought, simplified, or outsourced. Use **Domain Vision Statement** to align stakeholders on the core.

## Practical workflow for agents

When modeling or reviewing code:

1. **Identify the Bounded Context** — which model applies here?
2. **Extract Ubiquitous Language** from user stories, existing names, and domain docs.
3. **Classify nouns** → Entity vs Value Object vs Aggregate root.
4. **Place behavior** — default to Entity/Value; use Domain Service only when unnatural elsewhere.
5. **Draw aggregate boundaries** — what must stay consistent in one transaction?
6. **Place persistence** — Repository per root that needs global access.
7. **Check alignment** — do class names, method names, and tests use the same language as domain experts?
8. **Flag strategic issues** — legacy imports, shared models between teams, missing ACL.

## C# mapping cheatsheet

| DDD concept | Typical C# expression |
|-------------|----------------------|
| Value Object | `readonly record struct Money(decimal Amount, string Currency)` |
| Entity | `class Order` with `OrderId Id` and domain methods |
| Aggregate root | Entity that owns child collections; private setters on children |
| Domain event | `record OrderPlaced(OrderId Id, DateTimeOffset At)` |
| Repository | `interface IOrderRepository { Task<Order?> GetAsync(OrderId id, …); … }` |
| Domain Service | `class RoutingService` with stateless `GenerateItinerary(RouteSpecification spec)` |
| Application Service | `class BookCargoHandler` — loads aggregate, calls domain, saves, dispatches events |
| ACL | `LegacyCargoAdapter : ICargoTrackingGateway` translating DTOs ↔ domain types |
| Specification | `interface ISpecification<T> { bool IsSatisfiedBy(T candidate); }` |

Keep `{ get; set; }` on mapping/DTO types only at **boundaries** (Excel import, HTTP) — not on core domain types unless expression-binding tooling requires it.

## Red flags (challenge the design)

- Anemic domain model — all logic in `*Service` classes, entities are bags of properties
- God aggregate — one root owns half the system
- Repository per table — persistence shape driving the model
- Same class name used differently in two modules without explicit Bounded Context
- Direct use of legacy/external DTOs inside domain logic
- Generic `Update*` methods that bypass aggregate invariants
- "Manager", "Helper", "Util" types with domain rules inside

## When DDD is overkill

Skip heavy strategic patterns when:

- The domain is trivial CRUD with no real business rules
- A single small team owns one cohesive model end-to-end
- The task is purely technical (tooling, CI, formatting)

Still apply **Ubiquitous Language** and **Entity vs Value** distinction when any non-trivial rules exist.

## Further reading

- Eric Evans — *Domain-Driven Design* (this skill's source)
- [domaindrivendesign.org](http://domaindrivendesign.org) — supplemental examples and community
- Vaughn Vernon — *Implementing Domain-Driven Design* (tactical + strategic detail, CQRS, event sourcing)
- For functional expression of domain logic in C#, combine with `functional-programming-csharp` skill
