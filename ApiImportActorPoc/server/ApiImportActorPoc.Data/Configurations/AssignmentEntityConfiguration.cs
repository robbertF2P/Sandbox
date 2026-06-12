using ApiImportActorPoc.Data.Conversions;
using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiImportActorPoc.Data.Configurations;

public sealed class AssignmentEntityConfiguration : IEntityTypeConfiguration<AssignmentEntity>
{
    public void Configure(EntityTypeBuilder<AssignmentEntity> builder)
    {
        builder.ToTable("Assignments");
        builder.HasKey(assignment => assignment.Id);
        builder.Property(assignment => assignment.Id).UseIdentityColumn();
        builder.Property(assignment => assignment.PersonName).HasPersonNameColumn().IsRequired();
        builder.Property(assignment => assignment.Description).HasMaxLength(1024);
        builder.Property(assignment => assignment.BudgetedHours).HasHoursColumn();
    }
}

