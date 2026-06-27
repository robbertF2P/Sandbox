using HourApprovals.Application.Persistence;
using HourApprovals.Domain.Models;
using HourApprovals.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Platform.Shared.Domain;

namespace HourApprovals.Infrastructure.Persistence;

internal sealed class EfHourApprovalsRepository(HourApprovalsDbContext dbContext) : IHourApprovalsRepository
{
    public async Task<IReadOnlyList<ActiveTask>> ListTasksAsync(CancellationToken cancellationToken)
    {
        List<ActiveTaskEntity> entities = await dbContext.ActiveTasks
            .AsNoTracking()
            .OrderBy(task => task.Title)
            .ToListAsync(cancellationToken);

        return entities.Select(HourApprovalsDomainMapper.ToDomain).ToList();
    }

    public async Task<ActiveTask?> GetTaskAsync(TaskId taskId, CancellationToken cancellationToken)
    {
        ActiveTaskEntity? entity = await dbContext.ActiveTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(task => task.Id == taskId.Value, cancellationToken);

        return entity is null ? null : HourApprovalsDomainMapper.ToDomain(entity);
    }

    public async Task SaveTaskAsync(ActiveTask task, CancellationToken cancellationToken)
    {
        ActiveTaskEntity? entity = await dbContext.ActiveTasks
            .FirstOrDefaultAsync(existing => existing.Id == task.Id.Value, cancellationToken);

        if (entity is null)
        {
            entity = new ActiveTaskEntity { Id = task.Id.Value };
            dbContext.ActiveTasks.Add(entity);
        }

        HourApprovalsDomainMapper.ApplyDomain(entity, task);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AppendApprovalRecordAsync(ApprovalRecord record, CancellationToken cancellationToken)
    {
        dbContext.ApprovalRecords.Add(HourApprovalsDomainMapper.ToEntity(record));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ApprovalRecord?> GetLatestApprovalAsync(TaskId taskId, CancellationToken cancellationToken)
    {
        List<ApprovalRecordEntity> entities = await dbContext.ApprovalRecords
            .AsNoTracking()
            .Where(record => record.TaskId == taskId.Value)
            .ToListAsync(cancellationToken);

        ApprovalRecordEntity? entity = entities
            .OrderByDescending(record => record.ApprovedAtUtc)
            .FirstOrDefault();

        return entity is null ? null : HourApprovalsDomainMapper.ToDomain(entity);
    }
}
