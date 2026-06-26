# Change handler migration standard

**Purpose:** Strategy for replacing `Floor2PlanDbContext` SaveChanges handler chains with explicit domain events and tested actor pipelines during bounded-context extraction.

**Audience:** Engineers and AI agents working on module extraction in the F2P monolith.

**Related:**

| Document | Role |
|----------|------|
| `foundation-and-pilot-plan.md` | Extraction phases and gate definitions |
| `platform-actor-standard.md` | Actor workflow model; persist actor as the EF boundary |
| `ai-assisted-delivery-quality-framework.md` | AC test gates; anti-slop rules |
| `analysis-instructions.md` | Phase 1 entry-point inventory (source of handler catalog) |

---

## Core principle

> **Handlers are implementation details; use-case AC tests are the contract.**

The 55+ SaveChanges handlers in `Floor2PlanDbContext` are leftover stored-procedure logic — they implement observable behavior but do not own it. The migration replaces the mechanism (handler chain) with explicit domain events and actor pipelines while the top-level acceptance criteria tests remain the regression net throughout.

**Do not** treat handlers as the unit of work. Extract them as a side effect of extracting the bounded context that owns the triggering entity.

---

## When to migrate handlers

Handlers on a given entity family are migrated **when that entity's bounded context is extracted** — not earlier, not as a standalone task.

| State | Action |
|-------|--------|
| Entity owned by an extracted context | Migrate handlers for that entity in the same extraction PR |
| Entity still shared across contexts | Leave handlers untouched until ownership is resolved |
| Handler effect crosses context boundaries | Convert to an integration event (see below) |

---

## Migration steps (per entity family)

### 1. Inventory (Phase B — analysis artifact)

During Phase 1 / Phase 2 analysis, produce a `handler-inventory.md` (or YAML rows in `contexts/<slug>/handlers.yaml`) for each bounded context. For every handler:

| Field | Contents |
|-------|----------|
| `trigger` | Entity type + operation (`Activity INSERT`, `Assignment UPDATE`) |
| `effect` | Observable output — DB rows written, Hangfire job enqueued, notification sent |
| `owning_context` | Which bounded context produces this effect |
| `cross_context` | `true` if the effect touches another context's tables |
| `ac_coverage` | UC-### / AC-### ids whose tests exercise this handler end-to-end |
| `status` | `pending` / `migrated` / `retired` |

Mark `[NEEDS REVIEW]` for any handler whose owning context is ambiguous.

**Agent prompt:**

```text
Scan Floor2PlanDbContext for SaveChanges interceptors, domain event dispatchers, and
OnModelCreating side-effect hooks. For each, record trigger entity, downstream DB/queue
effect, and whether the effect touches tables outside the triggering entity's schema.
Cite file:line for every row. Mark [NEEDS REVIEW] if ownership is unclear.
```

### 2. Characterize at the use-case boundary

Before touching any handler, write AC tests that assert the **observable output** of the handler chain at the use-case boundary — not the handler internals:

- DB rows that exist after the operation
- Hangfire jobs enqueued
- Notifications or integration events dispatched

These are the regression net. A test that directly asserts `HandlerX.WasCalled` is not a characterization test.

**Gate G3 prerequisite:** at least one AC test covering each `[NEEDS REVIEW]` handler effect before that handler is migrated.

### 3. Introduce domain events on the entity

In the extracted context's Domain layer, add explicit domain events for the operations that previously triggered handlers:

```csharp
// Domain/Activities/Activity.cs
public class Activity : Entity<ActivityId>
{
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _events;

    public void Assign(UserId assignee)
    {
        // ... domain rule ...
        _events.Add(new ActivityAssigned(Id, assignee, ...));
    }
}
```

Domain events are plain records in `<Context>.Domain` — no framework dependencies.

### 4. Raise and dispatch from the persist actor

The persist actor (sole EF boundary for the workflow) raises domain events after `SaveChangesAsync`:

```csharp
// Infrastructure/Persistence/PersistActor.cs
protected override void OnReceive(PersistActivityCommand cmd)
{
    // ... EF save ...
    await _dbContext.SaveChangesAsync();
    foreach (var evt in entity.DomainEvents)
        _eventDispatcher.Dispatch(evt);  // routes to subscribers or outbox
}
```

This replaces the SaveChanges handler hook with an explicit post-save step owned by the context.

### 5. Replace handler effects with subscribers

For effects **within the same context**, add a domain event subscriber in Application:

```csharp
// Application/Activities/Handlers/ActivityAssignedHandler.cs
public class ActivityAssignedHandler : IDomainEventHandler<ActivityAssigned>
{
    public Task Handle(ActivityAssigned evt, CancellationToken ct)
    {
        // previously done in SaveChanges handler
    }
}
```

For effects that **cross context boundaries**, publish an integration event instead. The consuming context subscribes; it does not receive the domain event directly.

```csharp
// Infrastructure/Integration/ActivityAssignedPublisher.cs
// subscribes to ActivityAssigned domain event → publishes ActivityAssignedIntegrationEvent
```

### 6. Delete the handler and verify

Once the subscriber is in place and AC tests pass:

1. Delete the SaveChanges handler.
2. Run full AC test suite for the affected use cases.
3. Run integration tests to confirm the downstream DB state / queue entries still match.
4. Update `handler-inventory.md`: set `status: migrated`.

The PR description must reference the UC-### / AC-### ids covered and note the deleted handler.

---

## Sunset rule

A handler may be deleted when all of the following are true:

- [ ] A named domain event exists for the trigger operation
- [ ] At least one subscriber implements the equivalent effect
- [ ] AC tests for every `ac_coverage` id pass unchanged (before and after deletion)
- [ ] No other context depends on the handler's side effect without a replacement integration event path
- [ ] `handler-inventory.md` row updated to `status: retired`

---

## Cross-context handler effects

When a handler on entity A writes to a table owned by context B:

1. Context A publishes an **integration event** after save (`ActivityAssignedIntegrationEvent` in `<ContextA>.Contracts`).
2. Context B subscribes via its own actor or application handler.
3. Neither context references the other's Domain or Infrastructure directly.
4. The integration event shape is agreed by both context owners before implementation.

Mark these `cross_context: true` in the inventory and flag for G1 context map review — cross-context handler effects are often the signal that a context boundary needs adjustment.

---

## What not to do

| Anti-pattern | Instead |
|--------------|---------|
| Migrate handlers before extracting the entity's context | Extract context first; handlers follow |
| Write tests that assert handler internals | Characterize observable output at AC level |
| Route cross-context effects as direct service calls | Integration events via `*.Contracts` |
| Migrate all handlers in one PR | One entity family per extraction PR |
| Leave a handler "temporarily" alongside its replacement | Delete immediately once AC tests pass; dual-write is a liability |

---

## Handler inventory artifact (template)

Add this file to the monolith at `docs/modularization/contexts/<slug>/handler-inventory.md`:

```markdown
# Handler inventory — <Context>

| Handler class | Trigger | Effect | Cross-context | AC coverage | Status |
|---------------|---------|--------|--------------|-------------|--------|
| ActivitySyncHandler | Activity UPDATE | Enqueues sync job | false | UC-012 AC-3 | pending |
| AssignmentNotifyHandler | Assignment INSERT | Writes Notification row | true → Notifications context | UC-031 AC-1 | pending |
```

AI agents produce the initial draft; a human gate confirms ownership and AC coverage before migration starts.

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-25 | Initial standard — handler inventory, per-entity-family migration, cross-context integration events |
