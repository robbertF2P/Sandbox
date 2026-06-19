---
name: implementing-domain-driven-design
description: |
  Guides practical DDD implementation following Vaughn Vernon's "Implementing Domain-Driven Design"
  (Addison-Wesley). Use when:
  - Implementing aggregates, repositories, domain events, or application services in code
  - Choosing subdomain type (Core, Supporting, Generic) or sizing bounded contexts
  - Applying CQRS, event sourcing, sagas, or hexagonal/ports-and-adapters architecture with DDD
  - Integrating bounded contexts via REST, messaging, or anti-corruption layers
  - Fixing large-cluster aggregates, anemic models, or cross-aggregate transaction problems
  - Deciding whether a project warrants a DDD investment
paths:
  - "**/*.cs"
  - "**/*.csproj"
  - "**/docs/**"
metadata:
  version: 1.0.0
  source: "Vaughn Vernon — Implementing Domain-Driven Design (Addison-Wesley, 2013)"
  companion: "domain-driven-design (Eric Evans)"
---

# Implementing Domain-Driven Design

Apply this skill for **hands-on DDD implementation** — turning models into running software, choosing architecture, and avoiding common tactical mistakes. For conceptual foundations (Ubiquitous Language, strategic patterns vocabulary), combine with the `domain-driven-design` skill (Evans).

Match existing project conventions (see `dotnet-core-csharp-development`). DDD does not mandate a specific architecture — choose styles to meet **real quality demands**, not for coolness.

## When to invest in DDD

Use DDD to **simplify** a nontrivial domain, not to add complexity.

| Signal | Lean toward DDD |
|--------|-----------------|
| Domain rules are rich and change often | Yes |
| Developer + domain expert collaboration is possible | Yes |
| Generic CRUD with few invariants | No |
| Team lacks OO modeling experience and no time to learn | Cautious |

**Core Domain** gets the best people and deepest modeling. **Supporting Subdomains** are necessary but not differentiating. **Generic Subdomains** (identity, billing, email) — buy or use off-the-shelf when possible.

A Bounded Context you build as your **chief business initiative** is *your* Core Domain regardless of how downstream consumers view it.

## Subdomains and bounded contexts

- The whole **Domain** = multiple **Subdomains** (business capabilities).
- A **Bounded Context** = explicit boundary where **one model + one Ubiquitous Language** apply.
- Do **not** model the entire enterprise in one context — fused models become unmaintainable.

**Sizing:** a context can align with a team, a deployable service, or a module — but the driver is **linguistic and model consistency**, not org chart alone.

**Splinter symptoms inside a context:**

- Duplicate concepts (same idea modeled twice)
- False cognates (same word, different meaning)
- Awkward Ubiquitous Language in discussions

**Continuous Integration** (merge/build/test + relentless language alignment) applies **within** a Bounded Context, not across contexts.

## Context map (implementation view)

Draw and name every Bounded Context. Document relationships explicitly — **avoid code reuse across contexts**.

| Pattern | Use |
|---------|-----|
| **Partnership** | Two teams, coordinated success |
| **Shared Kernel** | Small shared model; joint ownership; strict CI |
| **Customer/Supplier** | Upstream/downstream; downstream defines needs |
| **Conformist** | Downstream takes upstream model as-is |
| **Anti-Corruption Layer (ACL)** | Translate foreign/legacy model at boundary |
| **Open Host Service + Published Language** | Expose integration protocol + documented interchange format |
| **Separate Ways** | No integration |

Integration always involves **translation** — never let foreign types into your domain layer.

## Architecture options (use when justified)

| Style | Role with DDD |
|-------|----------------|
| **Layers + DIP** | Domain at center; infrastructure implements abstractions |
| **Hexagonal (Ports & Adapters)** | Same idea — plug UI, DB, messaging as adapters |
| **REST** | Integrate contexts; resources as integration boundary |
| **CQRS** | Separate **command model** (writes) and **query model** (reads) when views cut across aggregates |
| **Event-Driven + Domain Events** | Decouple contexts; eventual consistency between aggregates |
| **Sagas / Long-Running Processes** | Coordinate multi-step, multi-context workflows |
| **Event Sourcing (+ Aggregates, A+ES)** | Persist state as event stream; optional for high-throughput command side |

**CQRS rule of thumb:** apply when repository-based queries force awkward multi-fetch assembly or compromise UX. Command side: aggregates have **commands only** (no getters); repositories stripped to `Add/Save` + `GetById`. Query side: denormalized read models tuned for screens.

**Eventual consistency:** rules spanning aggregates are **not** atomically up-to-date — reconcile via domain events, batch, or async handlers within acceptable business delay.

## Domain events

**Definition:** something happened that domain experts care about — model it as a first-class domain object.

**Listen for:** "When…", "If that happens…", "Notify me if…", "An occurrence of…"

**Modeling rules:**

- Name events in **past tense** from the command that caused them (`BacklogItemCommitted`, not `CommitBacklogItem`).
- Events belong to the **originating** Bounded Context's Ubiquitous Language.
- **Do not** couple the domain model to messaging middleware — publish via lightweight in-process pub/sub; infrastructure forwards to remote subscribers.

```csharp
// Domain: record event inside aggregate, publish after state change
public sealed class BacklogItem
{
    private readonly List<IDomainEvent> _events = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _events;

    public void CommitTo(SprintId sprintId)
    {
        // ... enforce invariants ...
        _events.Add(new BacklogItemCommitted(Id, sprintId));
    }

    public void ClearDomainEvents() => _events.Clear();
}
```

## Entities — identity

Every Entity has **unique identity** stable across time and storage.

| Strategy | When |
|----------|------|
| User provides | Human-meaningful ID (username, SKU) |
| Application generates | App assigns UUID/sequence at creation |
| Persistence generates | DB auto-increment (surrogate) |
| Another context assigns | Integration hands off identity |

Prefer **surrogate identity** internally when business identifiers can change. Entities hold **behavior + invariants**, not just properties.

## Value objects — implementation

Characteristics: measures/describes, **immutable**, **conceptual whole**, compared by value, side-effect-free.

| Persistence approach | When |
|------------------------|------|
| Embedded in entity table/columns | Default — prefer colocation |
| Serialized to single column | Many simple values |
| Separate table / join | Rare; question if still a Value |

**Reject data-model leakage** — ORM convenience must not drive the domain model.

```csharp
public readonly record struct Address(string Street, string City, string PostalCode)
{
    public Address Normalize() => this with { PostalCode = PostalCode.Trim().ToUpperInvariant() };
}
```

## Domain services

Use when an operation is a **significant domain concept** but not natural on an Entity or Value:

- Named as a **verb/activity** from Ubiquitous Language
- **Stateless** — no own lifecycle
- Parameters and return types are domain objects
- Not a `*Manager` dumping ground for procedural code

Distinguish **domain services** (domain rules) from **application services** (orchestration, transactions, authorization).

## Aggregates — rules of thumb

Aggregate = **transactional consistency boundary**. Synonymous with the cluster whose invariants must hold after **one commit**.

### Rule 1: Model true invariants in consistency boundaries

Only cluster objects that **must be atomically consistent** per business rules. Ask: "What must be consistent immediately after this operation?"

- **Transactional consistency** = invariants inside the boundary
- **Eventual consistency** = rules spanning boundaries

### Rule 2: Design small aggregates

Avoid **large-cluster aggregates** (root + unbounded child collections). They hurt concurrency, memory, and performance — loading thousands of children to add one item is a design smell.

Prefer **one root + value-object parts + limited entity children** that share true invariants.

### Rule 3: Reference other aggregates by identity

Hold **ID references**, not direct object references, to other aggregate roots:

```csharp
// Prefer
public sealed class BacklogItem
{
    public ProductId ProductId { get; private set; }
}

// Avoid (invites cross-aggregate mutation in one transaction)
public sealed class BacklogItem
{
    public Product Product { get; private set; }
}
```

**Modify only one aggregate instance per transaction.** If you need to touch two roots atomically, revisit boundaries or use eventual consistency.

### Rule 4: Use eventual consistency outside the boundary

When one command affects multiple aggregates, the root publishes a **domain event**; subscribers update other aggregates asynchronously.

Ask domain experts what delay is acceptable (seconds, minutes, hours).

### Valid reasons to break rules (rare)

1. UI convenience — prefer read models / CQRS instead
2. Lack of technical mechanisms — add events, sagas, projections
3. Global transactions — avoid; use sagas
4. Query performance — use **read model projections**, not bloated aggregates

### Implementation techniques

- **Tell, Don't Ask** — command the root; don't pull child objects and mutate externally
- **Law of Demeter** — no long navigation chains through object graphs
- **Optimistic concurrency** on roots (`Version` / row version)
- **Avoid dependency injection into entities** — pass what the operation needs as arguments or use domain services/factories

## Factories

Encapsulate complex creation/reconstitution:

- **Factory method on aggregate root** for internal parts
- **Standalone factory** or **factory interface** when construction is multi-step or cross-module
- **Factory as domain service** when creating from external identity (e.g. `CollaboratorService.authorFrom(tenant, userId)`)
- ACL boundary may host factories that translate external data → domain objects

Constructors suffice when creation is simple and invariants are minimal.

## Repositories

**Not** generic DAOs — they represent a **collection illusion** for aggregate roots.

| Type | Character |
|------|-----------|
| **Collection-oriented** | `Add`, `Remove`, `GetById` — like an in-memory set |
| **Persistence-oriented** | May align with ORM session/work unit patterns |

**Rules:**

- One repository per **aggregate root** needing global access
- Client speaks Ubiquitous Language in query criteria
- Hide all SQL/ORM/query technology
- In-memory implementation for tests

Avoid rich finder APIs that encourage bypassing aggregate design — if queries are painful, consider **CQRS read models**.

## Application layer

An **application** = finest assembly supporting a Core Domain model: UI + Application Services + domain + infrastructure.

**Application Services:**

- Orchestrate use cases — load aggregate, invoke domain behavior, save, dispatch events
- Manage transactions and authorization
- **Thin** — no business rules (those stay in domain)
- Coordinate multiple repositories and domain services

**UI options:**

- Render domain objects directly (simple cases)
- **DTOs / Presentation Model** when views need shapes the domain doesn't provide
- Never leak UI concerns into the domain

```csharp
public sealed class CommitBacklogItemHandler
{
    public async Task Handle(CommitBacklogItem command, CancellationToken ct)
    {
        var item = await _backlogItems.GetByIdAsync(command.BacklogItemId, ct)
            ?? throw new NotFoundException(...);

        item.CommitTo(command.SprintId);

        await _backlogItems.SaveAsync(item, ct);
        await _events.DispatchAsync(item.DomainEvents, ct);
        item.ClearDomainEvents();
    }
}
```

## Integrating bounded contexts

Distributed systems are **fundamentally different** — no atomic cross-system transactions.

- Exchange via **REST resources**, **messages**, or **published language** documents
- **ACL** translates external DTOs ↔ your domain at the boundary
- Domain layer never depends on foreign models

## Event sourcing (A+ES) — when to consider

Append-only **event store** as system of record; aggregates rehydrate from events; snapshots optional.

Use when:

- Audit history is intrinsic to the domain
- High write throughput on command side with temporal queries
- Combined with CQRS for scalable read/write split

Adds significant complexity — justify before adopting.

## Practical workflow for agents

1. Classify subdomain (**Core / Supporting / Generic**) — invest modeling effort accordingly.
2. Name and draw **Bounded Context** — confirm one Ubiquitous Language per context.
3. Apply **aggregate rules 1–4** — start small; identity references; one aggregate per transaction.
4. Place **domain events** at boundaries between aggregates/contexts.
5. Keep **application services** thin; push rules into entities/values/domain services.
6. Choose **architecture** (hexagonal baseline; add CQRS/ES/events only when requirements demand).
7. Integrate contexts with **ACL + context map**, not shared domain classes.

## Red flags

- Large-cluster aggregate "because Product has backlog items, releases, and sprints"
- Modifying two aggregate roots in one transaction
- Domain entities with public setters and zero behavior
- Repository with 20 finder methods feeding UI directly
- Messaging types imported in domain project
- Same `Customer` class shared by Sales and Support without explicit bounded context split
- `*Manager` classes containing all business logic

## Companion skills

- `domain-driven-design` — Evans foundations (language, strategic vocabulary, supple design)
- `functional-programming-csharp` — pure domain logic, explicit effects at boundaries
- `dotnet-core-csharp-development` — project layout and C# conventions
