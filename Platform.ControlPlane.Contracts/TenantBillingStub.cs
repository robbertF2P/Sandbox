namespace Platform.ControlPlane.Contracts;

public sealed record TenantBillingStub(
    string Tier,
    int SeatLimit);
