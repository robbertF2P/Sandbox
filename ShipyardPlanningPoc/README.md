# Shipyard Planning POC

Immutable **block turnover planning** for shipyard MES — a hull block's path from fit-up through weld, NDT, and crane turnover on the berth.

Demonstrates the [`immutable-domain-ef-core`](../../.cursor/skills/immutable-domain-ef-core/SKILL.md) skill: immutable aggregates with `With*` copy helpers, forward-pass schedule rippling, and EF Core graph reconciliation via [immutable-domain-tools](https://github.com/zoran-horvat/immutable-domain-tools).

## Scenario

RoRo ferry **HULL-4721** at **Berth 7**: blocks A12 and B03 share crane **GOLIATH**. When the crane breaks down during A12's lift, block B's turnover slides — without mutating the loaded plan in memory.

## Run

```bash
cd ShipyardPlanningPoc
dotnet run --project tests/ShipyardPlanning.Tests
```

## Projects

| Project | Role |
|---------|------|
| `ShipyardPlanning.Domain` | `BlockTurnoverPlan` aggregate, `TurnoverScheduleRippler` |
| `ShipyardPlanning.Infrastructure` | EF Core `PlanningDbContext` + immutable repositories |
| `ShipyardPlanning.Tests` | High-level domain scenarios + persistence characterization |
