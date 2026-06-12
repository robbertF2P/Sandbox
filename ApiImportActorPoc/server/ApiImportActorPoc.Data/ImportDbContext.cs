using ApiImportActorPoc.Data.Entities;
using ApiImportActorPoc.Data.Planning.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Data;

public sealed class ImportDbContext(DbContextOptions<ImportDbContext> options) : DbContext(options)
{
    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();

    public DbSet<ComponentEntity> Components => Set<ComponentEntity>();

    public DbSet<ActivityEntity> Activities => Set<ActivityEntity>();

    public DbSet<AssignmentEntity> Assignments => Set<AssignmentEntity>();

    public DbSet<ActivityRelationEntity> ActivityRelations => Set<ActivityRelationEntity>();

    public DbSet<EntityExternalIdEntity> EntityExternalIds => Set<EntityExternalIdEntity>();

    public DbSet<HourBookingEntity> HourBookings => Set<HourBookingEntity>();

    public DbSet<ProjectPlanEntity> ProjectPlans => Set<ProjectPlanEntity>();

    public DbSet<AssignmentPlanEntity> AssignmentPlans => Set<AssignmentPlanEntity>();

    public DbSet<MilestoneEntity> Milestones => Set<MilestoneEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ImportDbContext).Assembly);
    }
}
