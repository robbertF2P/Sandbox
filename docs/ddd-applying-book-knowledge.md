# Applying DDD book knowledge beyond skill files

Saved for follow-up (Evans *Domain-Driven Design*, Vernon *Implementing Domain-Driven Design*).

Skills in `.cursor/skills/` (and Copilot copies) encode book knowledge for AI assistants. Other high-leverage ways to use the same material:

---

## 1. Make the knowledge live in the code

Skills advise the AI; **the model is the real artifact**.

- Name types and methods in **Ubiquitous Language** (`RouteSpecification`, `Itinerary`, not `ShipmentTableRow`)
- Enforce **aggregate boundaries** in code (small roots, ID references, one transaction per root)
- Split **Bounded Contexts** into projects/namespaces with explicit integration at edges (ACL, DTOs)
- Keep a **glossary** in-repo (`docs/glossary.md`) — terms from Evans/Vernon, not generic IT speak

`PrimaveraExcelReader` is already a tactical example: fluent profiles, typed mapping, issue collection instead of abort-on-error.

---

## 2. Architecture governance (automated)

Turn principles into **checks that fail the build**:

- **ArchUnit / NetArchTest** — domain must not reference infrastructure; repositories only in application/infrastructure
- **Dependency rules** — no cross-context references except through defined integration projects
- **Custom analyzers or Roslyn rules** — e.g. flag public setters on aggregate roots, `*Manager` classes with domain logic

Books become **enforceable constraints**, not shelf decoration.

---

## 3. Decision records and maps

Skills compress; **ADRs and context maps** preserve *why*:

- **Context map** (Mermaid or whiteboard → committed diagram) — upstream/downstream, ACL vs conformist
- **ADRs** — “Why CQRS here?”, “Why eventual consistency between Order and Inventory?”
- **Domain vision statement** (Evans) — one page on Core Domain and what you’re *not* building

---

## 4. Workshops and deliberate practice

- **Event Storming** — domain events, aggregates emerge from collaboration
- **Scenario walkthroughs** — Evans cargo routing; Vernon SaaSOvation backlog/sprint mistakes
- **Refactoring katas** — anemic CRUD → rich domain + small aggregates
- **Book club + lab** — one chapter, one small change in a real module

Skills help AI; workshops align **humans** on the same language.

---

## 5. Reference implementations and templates

- Minimal **Core Domain slice**: Aggregate + Factory + Repository + Application Service + Domain Events
- **CQRS sample** (command side + read model) for one bounded context
- **`dotnet new` template** or `examples/ddd-cargo/` mirroring Evans Chapter 7

People learn by reading code that follows the book.

---

## 6. Review and quality gates

- **PR checklist** — one aggregate per transaction? Identity refs only? Domain events for cross-aggregate rules?
- **Copilot/Cursor review instructions** — shorter than full skills, always-on for PRs
- **Review agent** — “review this diff for DDD smells” (anemic model, large-cluster aggregate, leaked legacy DTOs)

Skills = on-demand tutor; review rules = **gatekeeper at merge time**.

---

## 7. Onboarding and teaching

- **Internal DDD playbook** — Entity vs Value Object, aggregate sizing, integration patterns (linked to book chapters)
- **Pairing rubric** — junior implements; senior checks against Evans/Vernon checklist
- **Hiring bar** — “explain bounded context vs subdomain”, “when is a domain service justified?”

---

## 8. Other AI surfaces (beyond SKILL.md)

| Surface | Role |
|--------|------|
| `.github/copilot-instructions.md` | Always-on standards (in repo) |
| `.cursor/rules/*.mdc` | Always-on in Cursor (e.g. no domain → EF references) |
| `.github/prompts/` | Reusable “design this aggregate”, “draw context map” workflows |
| Custom agents | Strategic design reviewer, aggregate boundary reviewer |
| MCP / glossary tool | Lookup Ubiquitous Language and bounded context boundaries |

Skills = deep, occasional knowledge; rules and prompts = frequent, narrow nudges.

---

## 9. Apply to real boundaries in this repo

Highest ROI: **one painful boundary**, not more docs:

- Import/API gateway ↔ core domain → **ACL** (Excel import vs domain DTOs)
- Akka actors ↔ domain model → actors orchestrate, **domain holds rules**
- Legacy Primavera/ERP export ↔ your model → translation layer, not shared entities

---

## Suggested stack for this repo

1. **Skills** — AI assistance (done: `.cursor/skills/`, `.github/skills/`, sync script)
2. **`docs/context-map.md` + glossary** — human alignment
3. **NetArchTest** — layer/boundary enforcement
4. **PR checklist** — merge-time DDD review
5. **One reference slice** — e.g. extend `PrimaveraExcelReader` with application service + import ACL naming the bounded context

---

## Related repo paths

- `.cursor/skills/domain-driven-design/` — Evans
- `.cursor/skills/implementing-domain-driven-design/` — Vernon
- `scripts/sync-agent-skills.sh` — refresh Copilot copies
- `AGENTS.md`, `.github/copilot-instructions.md`
- `PrimaveraExcelReader/` — tactical import/mapping POC

---

## Monday follow-up ideas

- [ ] Pick one system for a context map
- [ ] Draft `docs/glossary.md` from existing domain terms
- [ ] Add NetArchTest project or PR checklist
- [ ] Choose one integration boundary for an ACL spike
