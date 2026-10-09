using Microsoft.EntityFrameworkCore;
using RentalApplications.Data;
using RentalApplications.Services.Abstractions;
using RentalApplications.Services.Models;

namespace RentalApplications.Services;

/// <inheritdoc cref="IApplicationStatusHistoryService" />
public class ApplicationStatusHistoryService : IApplicationStatusHistoryService
{
    private readonly AppDbContext _db;

    public ApplicationStatusHistoryService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StatusHistoryRow>> GetHistoryAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        var histories = await _db.ApplicationStatusHistories
            .AsNoTracking()
            .Where(h => h.RentalApplicationId == applicationId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync(cancellationToken);

        var userIds = histories.Select(h => h.ChangedByUserId).Distinct().ToList();
        var users = await _db.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken);

        return histories
            .Select(h => new StatusHistoryRow(
                h.FromStatus,
                h.ToStatus,
                users.GetValueOrDefault(h.ChangedByUserId, "Unknown"),
                h.ChangedAt,
                h.Comment))
            .ToList();
    }
}
