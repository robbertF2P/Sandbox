using AkkaAspirePoc.Domain.Entities;
using AkkaAspirePoc.Domain.Services;

namespace AkkaAspirePoc.Tests.Domain;

public class TodoRulesShould
{
    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public async Task IsValidTitle_RejectsBlankTitles(string? title)
    {
        await Assert.That(TodoRules.IsValidTitle(title)).IsFalse();
    }

    [Test]
    public async Task IsValidTitle_AcceptsReasonableTitle()
    {
        await Assert.That(TodoRules.IsValidTitle("Ship Akka Aspire POC")).IsTrue();
    }

    [Test]
    public async Task IsValidTitle_RejectsTitleOverMaxLength()
    {
        var title = new string('x', TodoRules.MaxTitleLength + 1);

        await Assert.That(TodoRules.IsValidTitle(title)).IsFalse();
    }
}
