using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Contracts.Models.Planning;
using ApiImportActorPoc.Data;
using ApiImportActorPoc.Data.Entities;
using ApiImportActorPoc.Data.Planning.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiImportActorPoc.Core.Planning;

public sealed class PlanningService(IDbContextFactory<ImportDbContext> dbContextFactory)
{
    private static readonly DateOnly DefaultStartDate = new(2026, 1, 6);

    public async Task<GanttProjectPlanDto?> GetPlanAsync(int projectId, CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var context = await LoadContextAsync(db, projectId, cancellationToken);
        return context is null ? null : await CalculateAndStampAsync(db, context, cancellationToken);
    }

    public async Task<GanttProjectPlanDto?> SetProjectStartAsync(
        int projectId,
        DateOnly plannedStartDate,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var context = await LoadContextAsync(db, projectId, cancellationToken);
        if (context is null)
        {
            return null;
        }

        var plan = await db.ProjectPlans.FirstOrDefaultAsync(entity => entity.ProjectId == projectId, cancellationToken);
        if (plan is null)
        {
            db.ProjectPlans.Add(new ProjectPlanEntity
            {
                ProjectId = projectId,
                PlannedStartDate = plannedStartDate,
                LastCalculatedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            plan.PlannedStartDate = plannedStartDate;
        }

        await db.SaveChangesAsync(cancellationToken);
        context = context with { PlannedStartDate = plannedStartDate };
        return await CalculateAndStampAsync(db, context, cancellationToken);
    }

    public async Task<GanttProjectPlanDto?> SetAssignmentDurationAsync(
        int assignmentId,
        decimal durationDays,
        CancellationToken cancellationToken = default)
    {
        if (durationDays <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationDays), "Duration must be positive.");
        }

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var assignment = await db.Assignments
            .AsNoTracking()
            .Include(entity => entity.Activity)
                .ThenInclude(activity => activity.Component)
            .FirstOrDefaultAsync(entity => entity.Id == assignmentId, cancellationToken);

        if (assignment is null)
        {
            return null;
        }

        var projectId = assignment.Activity.Component.ProjectId;
        var plan = await db.AssignmentPlans.FirstOrDefaultAsync(entity => entity.AssignmentId == assignmentId, cancellationToken);
        if (plan is null)
        {
            db.AssignmentPlans.Add(new AssignmentPlanEntity
            {
                AssignmentId = assignmentId,
                DurationDays = durationDays
            });
        }
        else
        {
            plan.DurationDays = durationDays;
        }

        await db.SaveChangesAsync(cancellationToken);

        var context = await LoadContextAsync(db, projectId, cancellationToken);
        return context is null ? null : await CalculateAndStampAsync(db, context, cancellationToken);
    }

    public async Task<GanttMilestoneDto?> AddMilestoneAsync(
        int projectId,
        CreateMilestoneRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var projectExists = await db.Projects.AnyAsync(project => project.Id == projectId, cancellationToken);
        if (!projectExists)
        {
            return null;
        }

        if (request.ActivityId is int activityId)
        {
            var activityBelongsToProject = await db.Activities
                .AnyAsync(
                    activity => activity.Id == activityId && activity.Component.ProjectId == projectId,
                    cancellationToken);

            if (!activityBelongsToProject)
            {
                throw new InvalidOperationException($"Activity {activityId} does not belong to project {projectId}.");
            }
        }

        var milestone = new MilestoneEntity
        {
            ProjectId = projectId,
            Name = request.Name.Trim(),
            TargetDate = request.TargetDate,
            ActivityId = request.ActivityId
        };

        db.Milestones.Add(milestone);
        await db.SaveChangesAsync(cancellationToken);

        return new GanttMilestoneDto(milestone.Id, milestone.Name, milestone.TargetDate, milestone.ActivityId);
    }

    public async Task<bool> DeleteMilestoneAsync(int milestoneId, CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var milestone = await db.Milestones.FirstOrDefaultAsync(entity => entity.Id == milestoneId, cancellationToken);
        if (milestone is null)
        {
            return false;
        }

        db.Milestones.Remove(milestone);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private sealed record PlanningContext(
        int ProjectId,
        string ProjectName,
        DateOnly PlannedStartDate,
        IReadOnlyList<PlanningActivitySnapshot> Activities,
        IReadOnlyList<GanttMilestoneDto> Milestones);

    private async Task<PlanningContext?> LoadContextAsync(
        ImportDbContext db,
        int projectId,
        CancellationToken cancellationToken)
    {
        var project = await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == projectId, cancellationToken);

        if (project is null)
        {
            return null;
        }

        var plannedStart = await db.ProjectPlans
            .AsNoTracking()
            .Where(plan => plan.ProjectId == projectId)
            .Select(plan => (DateOnly?)plan.PlannedStartDate)
            .FirstOrDefaultAsync(cancellationToken) ?? DefaultStartDate;

        var components = await db.Components
            .AsNoTracking()
            .Where(component => component.ProjectId == projectId)
            .Include(component => component.Activities)
                .ThenInclude(activity => activity.Assignments)
            .Include(component => component.Activities)
                .ThenInclude(activity => activity.OutgoingRelations)
            .ToListAsync(cancellationToken);

        var assignmentIds = components
            .SelectMany(component => component.Activities)
            .SelectMany(activity => activity.Assignments)
            .Select(assignment => assignment.Id)
            .ToHashSet();

        var assignmentPlans = assignmentIds.Count == 0
            ? new Dictionary<int, decimal>()
            : await db.AssignmentPlans
                .AsNoTracking()
                .Where(plan => assignmentIds.Contains(plan.AssignmentId))
                .ToDictionaryAsync(plan => plan.AssignmentId, plan => plan.DurationDays, cancellationToken);

        var milestones = await db.Milestones
            .AsNoTracking()
            .Where(milestone => milestone.ProjectId == projectId)
            .OrderBy(milestone => milestone.TargetDate)
            .Select(milestone => new GanttMilestoneDto(
                milestone.Id,
                milestone.Name,
                milestone.TargetDate,
                milestone.ActivityId))
            .ToListAsync(cancellationToken);

        var activities = components
            .SelectMany(component => component.Activities.Select(activity => ToSnapshot(component, activity, assignmentPlans)))
            .ToList();

        return new PlanningContext(projectId, project.Name, plannedStart, activities, milestones);
    }

    private static PlanningActivitySnapshot ToSnapshot(
        ComponentEntity component,
        ActivityEntity activity,
        IReadOnlyDictionary<int, decimal> assignmentPlans)
    {
        var assignments = activity.Assignments
            .Select(assignment =>
            {
                assignmentPlans.TryGetValue(assignment.Id, out var plannedDuration);
                var label = string.IsNullOrWhiteSpace(assignment.PersonName)
                    ? assignment.Description ?? $"Assignment #{assignment.Id}"
                    : assignment.PersonName;
                return new PlanningAssignmentSnapshot(
                    assignment.Id,
                    label,
                    PlanningCalculator.ResolveDurationDays(
                        plannedDuration > 0 ? plannedDuration : null,
                        assignment.BudgetedHours));
            })
            .ToList();

        var relations = activity.OutgoingRelations
            .Select(relation => new ActivityRelationModel(
                relation.TargetActivityId,
                Enum.Parse<ActivityRelationType>(relation.RelationType, ignoreCase: true),
                relation.LagDays))
            .ToList();

        return new PlanningActivitySnapshot(
            activity.Id,
            activity.Name,
            component.Name,
            assignments,
            relations);
    }

    private static async Task<GanttProjectPlanDto> CalculateAndStampAsync(
        ImportDbContext db,
        PlanningContext context,
        CancellationToken cancellationToken)
    {
        var calculatedAt = DateTimeOffset.UtcNow;
        var plan = PlanningCalculator.Calculate(
            context.ProjectId,
            context.ProjectName,
            context.PlannedStartDate,
            context.Activities,
            context.Milestones,
            calculatedAt);

        var projectPlan = await db.ProjectPlans
            .FirstOrDefaultAsync(entity => entity.ProjectId == context.ProjectId, cancellationToken);

        if (projectPlan is null)
        {
            db.ProjectPlans.Add(new ProjectPlanEntity
            {
                ProjectId = context.ProjectId,
                PlannedStartDate = context.PlannedStartDate,
                LastCalculatedAt = calculatedAt
            });
        }
        else
        {
            projectPlan.PlannedStartDate = context.PlannedStartDate;
            projectPlan.LastCalculatedAt = calculatedAt;
        }

        await db.SaveChangesAsync(cancellationToken);
        return plan;
    }
}
