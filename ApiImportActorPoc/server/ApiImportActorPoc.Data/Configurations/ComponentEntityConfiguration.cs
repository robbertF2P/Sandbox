using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiImportActorPoc.Data.Configurations;

public sealed class ComponentEntityConfiguration : IEntityTypeConfiguration<ComponentEntity>
{
    public void Configure(EntityTypeBuilder<ComponentEntity> builder)
    {
        builder.ToTable("Components");
        builder.HasKey(component => component.Id);
        builder.Property(component => component.Id).UseIdentityColumn();
        builder.Property(component => component.Name).HasMaxLength(256).IsRequired();
        builder.HasOne(component => component.ParentComponent)
            .WithMany(component => component.ChildComponents)
            .HasForeignKey(component => component.ParentComponentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(component => component.Activities)
            .WithOne(activity => activity.Component)
            .HasForeignKey(activity => activity.ComponentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
