using System.Collections.Immutable;
using ImmutableDomain.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShipyardPlanning.Domain.Models;
using ShipyardPlanning.Domain.ValueObjects;

namespace ShipyardPlanning.Infrastructure;

public sealed class PlanningDbContext(DbContextOptions<PlanningDbContext> options) : DbContext(options)
{
    public IImmutableEntityRepository<BlockTurnoverPlan> TurnoverPlans =>
        Set<BlockTurnoverPlan>().ToImmutableEntityRepository(this, "_operations");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TurnoverOperation>(entity =>
        {
            entity.HasKey("Id");
            entity.Property(operation => operation.OperationCode).IsRequired().HasMaxLength(64);
            entity.Property(operation => operation.Kind)
                .HasConversion(
                    kind => kind.ToString(),
                    value => Enum.Parse<TurnoverOperationKind>(value))
                .HasMaxLength(32);
            entity.Property(operation => operation.ScheduledStart);

            entity.Property(operation => operation.BlockCode)
                .HasConversion(code => code.Value, value => new BlockCode(value))
                .HasMaxLength(32)
                .HasColumnName("BlockCode");

            entity.Property(operation => operation.Duration)
                .HasConversion(duration => duration.Value, value => new WorkMinutes(value))
                .HasColumnName("DurationMinutes");

            entity.Property(operation => operation.Crane)
                .HasConversion(
                    crane => crane == null ? null : crane.Value.Value,
                    value => value == null ? null : new CraneTag(value))
                .HasMaxLength(32)
                .HasColumnName("CraneTag");

            entity.Property(operation => operation.PredecessorCodes)
                .HasConversion(
                    codes => string.Join('|', codes),
                    value => string.IsNullOrEmpty(value)
                        ? ImmutableList<string>.Empty
                        : value.Split('|', StringSplitOptions.RemoveEmptyEntries).ToImmutableList())
                .HasColumnName("PredecessorCodes");
        });

        modelBuilder.Entity<BlockTurnoverPlan>(entity =>
        {
            entity.HasKey("Id");
            entity.HasAlternateKey(plan => plan.PublicId);
            entity.HasIndex(plan => plan.PublicId).IsUnique();

            entity.Property(plan => plan.BerthName).IsRequired().HasMaxLength(64);
            entity.Property(plan => plan.PlanningHorizonStart).IsRequired();
            entity.Property(plan => plan.Status)
                .HasConversion(
                    status => status.ToString(),
                    value => Enum.Parse<TurnoverPlanStatus>(value))
                .HasMaxLength(16);

            entity.Property(plan => plan.HullNumber)
                .HasConversion(number => number.Value, value => new HullNumber(value))
                .HasMaxLength(32)
                .HasColumnName("HullNumber");

            entity.Ignore(plan => plan.Operations);
            entity.Ignore(plan => plan.CraneOutages);
            entity.Ignore(plan => plan.PlanEnd);
            entity.Ignore(plan => plan.CriticalPathLength);

            entity.HasMany<TurnoverOperation>("_operations")
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
