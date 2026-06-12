using ApiImportActorPoc.Contracts.Models.Import;
using ApiImportActorPoc.Core.Import;

namespace ApiImportActorPoc.Api.Tests.Import;

public sealed class ExternalIdUniquenessValidatorTests
{
    [Fact]
    public void ValidateImportPayload_ThrowsWhenSameExternalIdUsedOnDifferentEntities()
    {
        var payload = new ProjectImportPayload(
            "Vessel",
            [
                new ComponentImportPayload(
                    null,
                    "Block",
                    null,
                    [
                        new ActivityImportPayload(
                            null,
                            "Welding",
                            null,
                            null,
                            new Dictionary<string, string> { ["PLM"] = "SHARED-1" })
                    ],
                    new Dictionary<string, string> { ["PLM"] = "SHARED-1" })
            ]);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ExternalIdUniquenessValidator.ValidateImportPayload(payload));

        Assert.Contains("SHARED-1", exception.Message);
    }
}
