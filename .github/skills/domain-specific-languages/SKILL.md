---
name: domain-specific-languages
description: |
  Guides design and implementation of domain-specific languages following Martin Fowler's
  "Domain-Specific Languages" (Addison-Wesley). Use when:
  - Deciding whether a DSL, fluent API, or plain library API fits a problem
  - Choosing internal vs external DSL, or interpretation vs code generation
  - Designing a Semantic Model, parser, or Expression Builder layer
  - Building configuration/rule languages, state machines, build specs, or query DSLs
  - Reviewing whether code is a real DSL vs a command-query API or serialized data
  - Applying internal DSL patterns (Method Chaining, Nested Function, Object Scoping, Closure)
  - Applying external DSL patterns (BNF, Parser Generator, Syntax-Directed Translation)
  For domain modeling vocabulary and Ubiquitous Language, combine with `domain-driven-design`.
paths:
  - "**/*.cs"
  - "**/*.csproj"
  - "**/docs/**"
metadata:
  version: 1.0.0
  source: "Martin Fowler — Domain-Specific Languages (Addison-Wesley, 2011)"
  companion: "domain-driven-design (Eric Evans)"
---

# Domain-Specific Languages

Apply this skill when a **limited, fluent language** can express one aspect of a system more clearly than general-purpose code — configuration rules, state machines, pricing logic, build dependencies, validation rules, or similar focused domains.

Match existing project conventions (see `dotnet-core-csharp-development`). Fowler's examples are Java/C#; internal DSL techniques depend heavily on host-language features. C# supports Method Chaining, Nested Function (via delegates/lambdas), Object Initializers, and progressive interfaces well.

## Definition (Fowler)

A **domain-specific language** is a computer programming language of **limited expressiveness** focused on a **particular domain**, with a **language nature** — fluency comes from composing expressions, not just calling isolated methods.

Four elements:

1. **Computer programming language** — executable by a machine, designed for human readers
2. **Language nature** — expressions compose into phrases/sentences, not disconnected commands
3. **Limited expressiveness** — bare minimum features for the domain; not Turing-complete in practice
4. **Domain focus** — narrow problem scope (you cannot build a whole system in the DSL)

## Three DSL styles

| Style | What it is | Examples |
|-------|------------|----------|
| **Internal DSL** | Stylized subset of a host language; parsed by executing it | Fluent builders, Rails-style configs, JMock expectations |
| **External DSL** | Separate language with its own syntax; parsed by custom tooling | SQL, regex, Ant/Make, custom rule files |
| **Language workbench** | IDE + custom editors + generator around a meta-model | MetaEdit, MPS — promising but evolving; treat as special case |

**True distinction (internal vs external):** internal DSLs are written in an **executable** host language and parsed by **running** them; external DSLs are **read** into data structures.

## Architecture — always start here

```
DSL script  →  parse  →  Semantic Model  →  execute OR generate
                  ↑                              ↑
            optional                      optional target code
```

### Semantic Model (centerpiece)

The **Semantic Model** holds the meaningful behavior and structure. The DSL is a thin facade that populates it.

- Usually an object model (can be a data structure with behavior elsewhere)
- Often a **subset** of the application's Domain Model — not everything belongs in the DSL
- Test it **independently** via command-query API before worrying about syntax
- Enables multiple DSLs (internal + external) over the same model
- Enables interpretation **and** code generation from the same parse result

**Do not** skip the Semantic Model to parse straight to generated code except in the simplest cases.

### Internal DSL layering

```
DSL script → host language parser → Expression Builders → Semantic Model
```

Use **Expression Builders** (343) as an explicit fluent layer; do not put all DSL surface area on domain types unless deliberate.

### External DSL layering

```
DSL script → lexer/parser → (optional syntax tree) → Semantic Model
```

"Parsing" in Fowler's sense is the **whole route from input text to Semantic Model**, not just tokenizing XML.

## Internal vs external — decision guide

No universal winner. Separate **model cost** from **DSL cost** — the model usually matters more.

| Factor | Lean internal | Lean external |
|--------|---------------|---------------|
| Learning curve (first time) | Easier to start; odd host-language tricks still required | Parser/grammar learning up front |
| IDE/tooling | Keep host IDE support (use Class Symbol Table if needed) | Usually text editor + syntax highlighting |
| Domain expert readability | Host syntax noise; language-dependent | Cleaner custom syntax possible |
| Mixing with host code | Natural — wafer-thin boundary | Awkward (Foreign Code, embedded strings) |
| Strong expressiveness boundary | Hard — host language always available | Easier to restrict what authors can do |
| Runtime configuration (compiled host) | Often needs interpreted companion language | Parse at runtime naturally |
| Sliding into generality | Less risk (host already general) | Risk — Ant-style feature creep |

**Pragmatic path:** build Semantic Model first; try internal DSL; add external DSL later if domain experts need clearer syntax — incremental cost is low when the model is shared.

## Interpretation vs code generation

| Approach | When |
|--------|------|
| **Interpretation** (execute Semantic Model) | Default — simplest, usually best |
| **Code generation** | Target environment cannot host parser/model; performance limits; need compile-time checking in another language; legacy platform (C, SQL, COBOL) |

Code generation styles:

- **Model-Aware Generation** (555) — generate only configuration; framework/generic code stays handwritten and testable (**prefer**)
- **Model Ignorant Generation** (567) — embed logic in generated control flow; more code to generate and maintain

Generation mechanics:

- **Transformer Generation** (533) — walk Semantic Model, emit code (usual default)
- **Templated Generation** (539) — template file with callouts; good when output is mostly static

**Rules for mixed generated/handwritten code:**

- Never hand-edit generated artifacts
- Keep generated and handwritten code clearly separate
- DSL script is the authoritative source of that behavior

## When to use a DSL

Weigh benefits against **model + DSL** cost. Separate model benefits from DSL benefits.

| Benefit | Notes |
|---------|-------|
| **Developer productivity** | Clearer intent → fewer defects, easier changes; DSL wraps awkward third-party APIs well |
| **Communication with domain experts** | Focus on **reading**, not the COBOL fallacy of business users writing all rules; visualization may suffice alone |
| **Execution context shift** | Runtime config, generate SQL for DB, deploy to constrained devices |
| **Alternative computational model** | State machines, dependency networks, production rule systems — model provides behavior; DSL makes declarative programs editable |

Typical project: **half a dozen small DSLs** in different places — many already exist as config formats.

## When not to use a DSL

- No meaningful benefit over a clear command-query API
- Domain is too simple for a model, let alone a DSL
- You would be building a **general-purpose language** (ghetto language anti-pattern)
- DSL would **blinker** the team — forcing the world into a bad abstraction

### Common problems

| Problem | Response |
|---------|----------|
| **Language cacophony** | DSLs are much smaller than GPLs; cost is incremental over learning the model |
| **Build cost** | Amortize techniques; DSL cost is on top of model cost |
| **Ghetto language** | Guard scope; never grow DSL toward Turing-completeness |
| **Sliding into generality** | Question every new feature; compose small DSLs instead of one growing language (Ant lesson) |
| **Blinkered abstraction** | Treat DSL + model as evolving; change abstraction when domain shifts |

## Internal DSL vs command-query API

| Command-query API | Internal DSL |
|-------------------|--------------|
| Methods make sense **individually** | Methods make sense **in composed phrases** |
| Vocabulary | Vocabulary + **grammar** |
| `unlockDoor(); lockPanel();` | `.actions(unlockDoor, lockPanel)` |
| Fine for simple stitching | Declarative flow; "pidgin" subset of host language |

**Red flag:** "DSL" that is only a sequence of unrelated method calls with no compositional grammar.

## Stand-alone vs fragmentary DSLs

| Form | Description | Examples |
|------|-------------|----------|
| **Stand-alone** | Whole file/script is DSL; understandable without host context | State machine config files, build files |
| **Fragmentary** | Short bursts embedded in host code | Regex, SQL strings, mock expectations, annotations |

Same DSL can serve both (SQL).

## Internal DSL patterns (C#-relevant)

| Pattern | Use when |
|---------|----------|
| **Method Chaining** (373) | Modifier methods return host for fluent sequences |
| **Expression Builder** (343) | Dedicated fluent objects over command-query model API |
| **Nested Function** (357) | Structure via nested calls; C# lambdas/local functions |
| **Nested Closure** (403) | Statement blocks as delegate arguments |
| **Function Sequence** (351) | Linear calls; simulate hierarchy with Context Variable (175) |
| **Object Scoping** (385) | Bare names resolve to one context object |
| **Object Initializers** | C# native — good for nested configuration trees |
| **Progressive interfaces** | Different return types per stage for compile-time checking |
| **Literal Extension** (481) | Extend literals (limited in C#; extension methods on wrappers) |
| **Annotation** (445) | Fragmentary metadata DSLs |
| **Parse Tree Manipulation** (455) | Post-process or transform parsed structure |
| **Dynamic Reception** (427) | `dynamic` for loose property-style DSLs (use sparingly) |

**C# tips:**

- Prefer **progressive interfaces** or **generic chaining** for compile-time safety
- Use **object initializers** and **collection initializers** before exotic tricks
- **Static factory + private constructors** on Expression Builders control valid phrases
- For IDE support with symbolic names, consider **enums** or **Class Symbol Table** (467)

## External DSL patterns

| Pattern | Use when |
|---------|----------|
| **Delimiter-Directed Translation** (201) | Line/chunk-oriented languages; simple formats |
| **Syntax-Directed Translation** (219) | Grammar-driven parsing — default for non-trivial external DSLs |
| **BNF** (229) | Formal grammar definition |
| **Regex Table Lexer** (239) | Tokenize via ordered regex rules |
| **Recursive Descent Parser** (245) | Hand-written top-down parser |
| **Parser Combinator** (255) | Compose parsers as objects/functions |
| **Parser Generator** (269) | ANTLR-style grammar → parser (ANTLR, similar tools) |
| **Tree Construction** (281) | Explicit parse tree data structure |
| **Embedded Translation** (299) | Actions during parse populate Semantic Model |
| **Embedded Interpretation** (305) | Execute during parse |
| **Foreign Code** (309) | Embed host-language fragments in external DSL |
| **Nested Operator Expression** (327) | Arithmetic/Boolean expressions |
| **Notification** (193) | Collect errors; don't abort on first mistake |

**XML as carrier syntax:** valid external DSL, but often high syntactic noise; custom syntax usually more readable. JSON/YAML are data serializations — poor fluency for true DSLs.

## Alternative computational models

When imperative code fights the problem, model differently:

| Model | Examples |
|-------|----------|
| **State Machine** (527) | Security controllers, workflows |
| **Dependency Network** (505) | Make, Rake, build pipelines |
| **Production Rule System** (513) | Forward-chaining rules |
| **Decision Table** (495) | Combinations of conditions in tabular form |
| **Adaptive Model** (487) | Data/config drives behavior; DSL gives "programming" feel |

DSL value is highest when the Semantic Model is an **adaptive** or **declarative** model whose population *is* the program.

## DSL lifecycle (practical order)

1. **Identify repeated family of behavior** with common engine, varying configuration
2. **Build Semantic Model** — test with command-query API (TDD on model first)
3. **Sketch pseudo-DSL** with domain experts on representative scenarios
4. **Implement thin DSL** — internal Expression Builders or external parser
5. **Iterate syntax** with readers (developers and/or domain experts)
6. **Version-control all DSL scripts** as source code
7. **Add visualization** from Semantic Model (Graphviz, tables) — cheap once model exists

Approaches:

- **Model-first** — framework exists; DSL added when configuration hurts
- **Language-seeded** — pseudo-DSL scenarios drive implementation atop stable model
- **Model-seeded** — add fluent methods to model, extract Expression Builders (gradual internal DSL)

Prefer **thin end-to-end slices** over big-bang framework-then-DSL.

## Good DSL design principles

- **Clarity for the reader** is the overriding goal
- Use **domain jargon** when readers know it
- Follow **familiar conventions** (`//` comments, `{` `}` blocks in C#/Java-like DSLs)
- **Iterate** — try alternatives on the target audience; reject missteps
- **Do not** make DSL read like natural language (AppleScript trap) — terseness and precision matter
- Prefer **small DSLs** composed with general-purpose code over one growing language

## Testing strategy

1. **Test Semantic Model** directly — no parser involved
2. **Test parser/DSL** — scripts produce expected model state
3. **Test end-to-end** — model execution or generated output
4. Use **Notification** (193) for usable diagnostics — errors are hard in language tooling

## Migration and evolution

- Keep Semantic Model stable; evolve syntax and model somewhat independently
- Multiple DSLs can populate one model during migration
- When changing DSL syntax, compare by **equivalent Semantic Model population**
- Guard against DSL features that drag the language toward general-purpose power

## Practical workflow for agents

When asked to design or review a DSL:

1. **Name the domain slice** — one aspect only; what is out of scope?
2. **Check limited expressiveness** — could this grow into a GPL? Stop that early.
3. **Design Semantic Model first** — types, invariants, execution; tests without syntax.
4. **Choose style** — internal vs external using decision table above.
5. **Choose execution** — interpret model vs generate code (default: interpret).
6. **Sketch 3–5 real scripts** — stand-alone or fragmentary? Who reads them?
7. **Implement minimal parser/builder** — one scenario end-to-end.
8. **Review fluency** — sentences, not command lists; domain expert readability if required.
9. **Add diagnostics and version control** for script files.
10. **Flag composition** — will this DSL need to mix with host code or stay restricted?

## C# mapping cheatsheet

| Concept | Typical C# expression |
|---------|----------------------|
| Semantic Model | Domain types with behavior (`StateMachine`, `Transition`, `Rule`) |
| Command-query API | Plain methods on model/builders for tests |
| Expression Builder | `StateMachineBuilder` with fluent methods returning `this` or staged interfaces |
| Internal DSL script | Subclass or lambda configuring via builder in `Define()` |
| Fragmentary DSL | `@"regex"`, interpolated SQL (with parameters), `[Attribute]` metadata |
| External DSL | `.rules` file + ANTLR/parser → populates model at startup |
| Interpretation | Load scripts at runtime → populate model → execute |
| Code generation | Roslyn/Source Generator or template emitting C# from model |
| Notification | `List<ParseDiagnostic>` collected during parse, returned with partial model |

## Red flags (challenge the design)

- No Semantic Model — parser emits strings or codegen directly
- "DSL" without language nature — property bag or XML serialization only
- One DSL accreting loops, variables, and general logic (Ant path)
- Building a custom general-purpose language for the whole application
- Internal DSL with heavy reflection/dynamic tricks no one on the team understands
- External DSL with no tests on the Semantic Model
- Hand-edited generated code
- DSL syntax designed before any representative examples exist
- Confusing model benefits with DSL benefits in the business case

## When a DSL is overkill

- Single fixed configuration with no variation
- A few parameters — constructor or options object suffices
- Team will never read scripts — only machines consume the format
- Domain experts are served equally well by a form UI or diagram on the Semantic Model

Still consider a **fluent API** or **clear model** when configuration complexity grows.

## Relationship to DDD

- DSL populates a **Semantic Model** that may overlap the **Domain Model**
- **Ubiquitous Language** should appear in DSL keywords and model type names
- Bounded Context may define its own DSL for context-specific rules
- Do not let foreign DSL concepts leak across contexts without translation

Combine with `domain-driven-design` for strategic/tactical modeling and `implementing-domain-driven-design` for implementation structure.

## Further reading

- Martin Fowler — *Domain-Specific Languages* (this skill's source)
- Martin Fowler — *Patterns of Enterprise Application Architecture* (Domain Model, framework patterns)
- Eric Evans — *Domain-Driven Design* (`domain-driven-design` skill)
- Book pattern catalog: Semantic Model, Expression Builder, Method Chaining, Parser Generator, State Machine, Notification, and 40+ others in Part II–V
