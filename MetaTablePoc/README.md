# MetaTable POC

Reference implementation for **metadata-defined tables**: you register logical tables and columns by inserting metadata rows—no per-table SQL DDL. Supports **text**, **integer**, and **association** (foreign row) columns.

See [docs/design.md](docs/design.md) for the model, trade-offs, and when to use codegen instead of pure EAV.

## Run tests

```bash
cd MetaTablePoc/tests/MetaTable.Tests
dotnet run
```

(On .NET 10 SDK, use `dotnet run` in the test project for Microsoft.Testing.Platform.)

## Mental model

| Metadata | Runtime data |
|----------|----------------|
| `meta_tables` | `meta_rows` (one row per instance) |
| `meta_columns` | `meta_text_cells`, `meta_integer_cells`, `meta_association_cells` |

Adding a new logical table = insert one `meta_tables` row plus one `meta_columns` row per field.
