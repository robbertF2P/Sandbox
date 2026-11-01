using MetaTable.Domain.Enums;
using MetaTable.Domain.Models;
using MetaTable.Domain.Rules;

namespace MetaTable.Tests.Domain;

public sealed class MetaSchemaRulesShould
{
    [Fact]
    public void Reject_association_column_without_target_table()
    {
        MetaTableEntity owning = new() { Id = Guid.NewGuid(), Key = "employee", DisplayName = "Employee" };
        MetaColumnEntity column = new()
        {
            Id = Guid.NewGuid(),
            TableId = owning.Id,
            Name = "Department",
            Kind = MetaColumnKind.Association,
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => MetaSchemaRules.ValidateNewColumn(column, owning, associationTarget: null));

        Assert.Contains("AssociationTargetTableId", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Reject_unknown_text_column_on_insert()
    {
        MetaColumnEntity column = new()
        {
            Id = Guid.NewGuid(),
            TableId = Guid.NewGuid(),
            Name = "Name",
            Kind = MetaColumnKind.Text,
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            MetaSchemaRules.ValidateRowValues(
                [column],
                textValues: new Dictionary<string, string?> { ["NotDeclared"] = "x" },
                integerValues: new Dictionary<string, int?>(),
                associationValues: new Dictionary<string, Guid?>()));

        Assert.Contains("NotDeclared", exception.Message, StringComparison.Ordinal);
    }
}
