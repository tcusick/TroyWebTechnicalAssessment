using Microsoft.EntityFrameworkCore;
using RentalApplications.Data;
using RentalApplications.Services.Abstractions;
using RentalApplications.Services.Models;

namespace RentalApplications.Services;

/// <inheritdoc cref="IAvailableUnitsService" />
public class AvailableUnitsService : IAvailableUnitsService
{
    private readonly AppDbContext _db;
    private readonly ILeaseAvailabilityService _leaseAvailability;

    public AvailableUnitsService(AppDbContext db, ILeaseAvailabilityService leaseAvailability)
    {
        _db = db;
        _leaseAvailability = leaseAvailability;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AvailableUnitRow>> GetAvailableUnitsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var units = await _db.Units
            .AsNoTracking()
            .Include(u => u.Property)
            .Include(u => u.UnitType)
            .Include(u => u.Leases)
            .OrderBy(u => u.Property.Name)
            .ThenBy(u => u.UnitNumber)
            .ToListAsync(cancellationToken);

        return units
            .Where(u => _leaseAvailability.IsUnitAvailableOnDate(u.Leases, today))
            .Select(u => new AvailableUnitRow(
                u.Id,
                u.Property.Name,
                $"{u.Property.StreetAddress}, {u.Property.City}, {u.Property.State} {u.Property.PostalCode}",
                u.UnitNumber,
                u.Bedrooms,
                u.MonthlyRent,
                u.UnitType.Name))
            .ToList();
    }
}
