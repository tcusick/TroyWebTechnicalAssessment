using RentalApplications.Domain.Entities;

namespace RentalApplications.Services.Abstractions;

/// <summary>
/// Prior residence rows attached to a rental application.
/// </summary>
public interface IResidenceHistoryService
{
    /// <summary>
    /// Loads a residence row with its parent application.
    /// </summary>
    Task<ResidenceHistory?> GetAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates a residence row and returns its id.
    /// </summary>
    Task<int> SaveAsync(ResidenceHistory residence, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a residence row when the parent application is editable.
    /// </summary>
    Task<(bool Success, int ApplicationId)> TryDeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the applicant may modify residence data on the given application.
    /// </summary>
    Task<bool> CanApplicantEditApplicationAsync(int applicationId, string userId, CancellationToken cancellationToken = default);
}
