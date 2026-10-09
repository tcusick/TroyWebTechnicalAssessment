using RentalApplications.Domain.Entities;

namespace RentalApplications.Services.Abstractions;

/// <summary>
/// Lease date calculations and overlap checks used for availability and approval.
/// </summary>
public interface ILeaseAvailabilityService
{
    /// <summary>
    /// Calculates the standard lease end date from a start date.
    /// </summary>
    DateOnly CalculateEndDate(DateOnly startDate);

    /// <summary>
    /// Returns whether a unit has no lease covering the given date.
    /// </summary>
    bool IsUnitAvailableOnDate(IEnumerable<Lease> leases, DateOnly date);

    /// <summary>
    /// Returns whether any lease is active on the given date.
    /// </summary>
    bool HasActiveLeaseOnDate(IEnumerable<Lease> leases, DateOnly date);
}
