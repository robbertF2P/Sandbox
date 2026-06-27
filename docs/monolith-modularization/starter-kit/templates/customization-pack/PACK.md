# Pack manifest: <pack-id>

| Field | Value |
|-------|-------|
| **packId** | `<pack-id>` |
| **packType** | `customization-ui` |
| **context** | `<Context>` |
| **client** | `<Client>` |
| **version** | `1.0.0` |
| **modulePort** | `I<Context>CustomizationPack` |
| **hostRegistration** | `Add<Client><Context>Pack()` |
| **i18nPrefix** | `packs.<pack-id>` |

## Screens

| screenId | Purpose | Extension columns |
|----------|---------|-------------------|
| `<context>-queue` | List / queue | *(none yet)* |

## Extension fields

| extensionKey | Legacy source | Role | Storage |
|--------------|---------------|------|---------|
| *(example)* `sapCostElement` | `Activity.Text3` | display | in-memory / DB table |

Roles: `display` | `filter` | `display+filter` | `workflow-guard` (→ rules pack) | `integration` (→ integration pack).

## Entitlements

```json
{
  "customizationPacks": ["<pack-id>"]
}
```

## Dependencies

- `<Context>.Application`
- `Platform.Shared.View`

## Notes

*(Tenant-specific semantics, promotion criteria, strangler mapping.)*
