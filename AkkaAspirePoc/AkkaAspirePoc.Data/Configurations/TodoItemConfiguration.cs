using AkkaAspirePoc.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkkaAspirePoc.Data.Configurations;

public sealed class TodoItemConfiguration : IEntityTypeConfiguration<TodoItemEntity>
{
    public void Configure(EntityTypeBuilder<TodoItemEntity> builder)
    {
        builder.ToTable("TodoItems");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.IsCompleted)
            .IsRequired();

        builder.Property(t => t.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(t => t.IsCompleted);
    }
}
