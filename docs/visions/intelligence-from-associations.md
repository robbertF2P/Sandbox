# Vision note: intelligence from associations

**Recorded:** 2026-07-27  
**Status:** Seed — next vision to be expanded  
**Trigger:** Reading on how intelligence emerges less from isolated facts and more from **relationships between** pieces of knowledge (association, context, *when*).

---

## Insight (from the article)

Traditional views treat “knowing” as holding discrete facts. The article’s angle: **intelligence emerges from how knowledge connects** — associations between things, temporal or situational links (*when* something applies), and the structure of those links rather than any single node in isolation.

| Isolated knowledge | Relational knowledge |
|--------------------|----------------------|
| “What is X?” | “What is X **in relation to** Y?” |
| Static fact | **When** / **under what conditions** |
| Single node | **Graph of associations** |

This matches how humans reason (cue → context → action), how memory works (spreading activation), and how many modern systems (retrieval, agents, knowledge graphs) outperform pure lookup.

---

## Idea sparked: next vision

**Working title:** *Platform intelligence lives in the relationships, not the encyclopedia.*

High-level direction (to refine):

1. **First-class associations** — Treat links between domain objects, events, tenants, packs, and workflows as durable, queryable artifacts—not only the entities at the endpoints.
2. **When, not only what** — Model time, phase, role, and tenant context as part of “knowing,” not as afterthought metadata.
3. **Emergent behavior** — Orchestration (actors, integrations, UI) should **traverse and compose** associations rather than hard-code one-off paths per customer.
4. **AI alignment** — Agents and retrieval work better when the system exposes **structured relationships** (who approved what for which assignment under which pack rule) instead of dumping raw tables.

This is intentionally parallel to Platform 2.0 themes already in flight: bounded contexts, packs, correlation/use-case tracing, and actor pipelines—all of which are partly about **wiring** and **context**, not only storing state.

---

## Open questions

- What was the article title / author / URL? *(Add when available.)*
- Is the “next vision” primarily **product UX**, **data model**, **AI/agent layer**, or **all three**?
- What is the smallest proof (POC) that would demonstrate “association-first” vs “entity-first”?

---

## Related repo pointers

- Platform composition and packs: `docs/monolith-modularization/platform-architecture-overview.md`
- Use-case tracing across boundaries: `docs/monolith-modularization/platform-correlation-standard.md`
- Actor orchestration: `docs/monolith-modularization/platform-actor-standard.md`

---

## Revision log

| Date | Change |
|------|--------|
| 2026-07-27 | Initial capture from founder reflection on association-based intelligence |
