namespace ControlPlane.Domain.Tenants;

public sealed record TenantBillingStub(
    string Tier,
    int SeatLimit);
