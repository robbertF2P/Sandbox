using System.Security.Cryptography;
using System.Text;

namespace PlanningApprovals.Domain.ValueObjects;

public static class ContentFingerprint
{
    public static string FromParts(params string[] parts)
    {
        string payload = string.Join('\u001f', parts);
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash);
    }
}
