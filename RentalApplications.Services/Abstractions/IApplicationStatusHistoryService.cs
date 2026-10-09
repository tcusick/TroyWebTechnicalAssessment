using RentalApplications.Services.Models;

namespace RentalApplications.Services.Abstractions;

/// <summary>
/// Status change audit trail for rental applications.
/// </summary>
public interface IApplicationStatusHistoryService
{
    /// <summary>
    /// Returns status history entries for an application, newest first.
    /// </summary>
    Task<IReadOnlyList<StatusHistoryRow>> GetHistoryAsync(int applicationId, CancellationToken cancellationToken = default);
}
