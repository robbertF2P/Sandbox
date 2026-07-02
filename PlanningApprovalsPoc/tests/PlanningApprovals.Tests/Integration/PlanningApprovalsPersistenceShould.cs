using Microsoft.EntityFrameworkCore;
using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.Rules;
using PlanningApprovals.Domain.Services;
using PlanningApprovals.Infrastructure;
using PlanningApprovals.Tests.Support;

namespace PlanningApprovals.Tests.Integration;

public sealed class PlanningApprovalsPersistenceShould
{
    [Fact]
    public async Task Persists_assignment_and_daily_approval_record()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"planning-approvals-{Guid.NewGuid():N}.db");
        PlanningApprovalsDbContextFactory factory = new($"Data Source={databasePath}");

        try
        {
            ActiveAssignment assignment = PlanningApprovalScenario.WeldingAssignment();
            DateTimeOffset approvedAtUtc = PlanningApprovalScenario.Today;

            AssignmentApprovalRecord record = PlanningApprovalCoordinator.RecordForemanApproval(
                assignment,
                PlanningApprovalScenario.ForemanPersonId,
                approvedAtUtc,
                existingForDay: null);

            await using (PlanningApprovalsDbContext writeContext = factory.CreateDbContext())
            {
                writeContext.ActiveAssignments.Add(assignment);
                writeContext.ApprovalRecords.Add(record);
                await writeContext.SaveChangesAsync();
            }

            await using PlanningApprovalsDbContext readContext = factory.CreateDbContext();

            ActiveAssignment loadedAssignment = await readContext.ActiveAssignments.SingleAsync();
            AssignmentApprovalRecord loadedRecord = await readContext.ApprovalRecords.SingleAsync();

            Assert.Equal(assignment.Id, loadedAssignment.Id);
            Assert.Equal(12.5m, loadedAssignment.CurrentValues.HoursToGo);
            Assert.Equal("j.doe", loadedAssignment.CurrentValues.AssignedUser.Value);
            Assert.Equal(record.Id, loadedRecord.Id);
            Assert.Equal(PlanningApprovalScenario.ForemanPersonId, loadedRecord.ApprovedBy);
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }

    [Fact]
    public async Task SameDayReapproval_UpdatesExistingRecord()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"planning-approvals-{Guid.NewGuid():N}.db");
        PlanningApprovalsDbContextFactory factory = new($"Data Source={databasePath}");

        try
        {
            ActiveAssignment assignment = PlanningApprovalScenario.FittingAssignment();
            DateTimeOffset firstApproval = PlanningApprovalScenario.Today;

            AssignmentApprovalRecord first = PlanningApprovalCoordinator.RecordForemanApproval(
                assignment,
                PlanningApprovalScenario.ForemanPersonId,
                firstApproval,
                existingForDay: null);

            await using (PlanningApprovalsDbContext writeContext = factory.CreateDbContext())
            {
                writeContext.ActiveAssignments.Add(assignment);
                writeContext.ApprovalRecords.Add(first);
                await writeContext.SaveChangesAsync();
            }

            assignment.UpdateValues(assignment.CurrentValues with { HoursToGo = 18m });

            AssignmentApprovalRecord updated = PlanningApprovalCoordinator.RecordForemanApproval(
                assignment,
                PlanningApprovalScenario.ForemanPersonId,
                firstApproval.AddHours(1),
                existingForDay: first);

            await using (PlanningApprovalsDbContext writeContext = factory.CreateDbContext())
            {
                writeContext.ActiveAssignments.Update(assignment);
                writeContext.ApprovalRecords.Update(updated);
                await writeContext.SaveChangesAsync();
            }

            await using PlanningApprovalsDbContext readContext = factory.CreateDbContext();

            Assert.Equal(1, await readContext.ApprovalRecords.CountAsync());
            AssignmentApprovalRecord loaded = await readContext.ApprovalRecords.SingleAsync();
            Assert.Equal(first.Id, loaded.Id);
            Assert.Equal(18m, loaded.ApprovedValues.HoursToGo);
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }

    [Fact]
    public async Task ChangedValues_RequireReapproval()
    {
        ActiveAssignment assignment = PlanningApprovalScenario.WeldingAssignment();
        AssignmentApprovalRecord approval = PlanningApprovalCoordinator.RecordForemanApproval(
            assignment,
            PlanningApprovalScenario.ForemanPersonId,
            PlanningApprovalScenario.Today,
            existingForDay: null);

        assignment.UpdateValues(assignment.CurrentValues with { HoursToGo = 99m });

        AssignmentApprovalState state = PlanningApprovalRules.ResolveState(
            assignment.CurrentValues,
            approval);

        Assert.Equal(AssignmentApprovalState.NotApproved, state);
    }
}
