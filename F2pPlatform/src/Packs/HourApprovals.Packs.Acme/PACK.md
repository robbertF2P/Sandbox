# Pack manifest: acme-hour-approvals-v1

| Field | Value |
|-------|-------|
| **packId** | `acme-hour-approvals-v1` |
| **packType** | `customization-ui` |
| **context** | `HourApprovals` |
| **client** | `Acme` |
| **version** | `1.0.0` |
| **modulePort** | `IHourApprovalsCustomizationPack` |
| **hostRegistration** | `AddAcmeHourApprovalsPack()` |
| **i18nPrefix** | `packs.acme-hour-approvals-v1` |

## Screens

| screenId | Purpose | Extension columns |
|----------|---------|-------------------|
| `hour-approvals-queue` | Supervisor approval queue | `sapCostElement` |

## Extension fields

| extensionKey | Legacy source | Role | Storage |
|--------------|---------------|------|---------|
| `sapCostElement` | *(illustrative)* `Activity.Text3` | display | in-memory (POC) |

## Entitlements

```json
{
  "customizationPacks": ["acme-hour-approvals-v1"]
}
```

## Dependencies

- `HourApprovals.Application`
- `Platform.Shared.View`

## Notes

Reference pack for `platform-pack-blueprint.md`. Shows core column visibility, one extension column, and computed column visibility (`daysSinceLastSubmission`).
