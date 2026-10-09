using RentalApplications.Services.Models;

namespace RentalApplications.Services.Abstractions;

/// <summary>
/// Units that applicants may apply for on the current date.
/// </summary>
public interface IAvailableUnitsService
{
    /// <summary>
    /// Returns units not blocked by an active lease today.
    /// </summary>
    Task<IReadOnlyList<AvailableUnitRow>> GetAvailableUnitsAsync(CancellationToken cancellationToken = default);
}
