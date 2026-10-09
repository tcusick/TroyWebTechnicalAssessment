using RentalApplications.Domain;
using RentalApplications.Domain.Entities;
using RentalApplications.Services.Abstractions;

namespace RentalApplications.Services;

/// <inheritdoc cref="ILeaseAvailabilityService" />
public class LeaseAvailabilityService : ILeaseAvailabilityService
{
    /// <inheritdoc />
    public DateOnly CalculateEndDate(DateOnly startDate) =>
        startDate.AddMonths(LeaseTerms.StandardLeaseMonths);

    /// <inheritdoc />
    public bool IsUnitAvailableOnDate(IEnumerable<Lease> leases, DateOnly date) =>
        !leases.Any(l => l.StartDate <= date && l.EndDate >= date);

    /// <inheritdoc />
    public bool HasActiveLeaseOnDate(IEnumerable<Lease> leases, DateOnly date) =>
        !IsUnitAvailableOnDate(leases, date);
}
