using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalApplications.Data;
using RentalApplications.Domain;
using RentalApplications.Domain.Entities;
using RentalApplications.Services.Abstractions;

namespace RentalApplications.Services;

/// <inheritdoc cref="IPropertyManagementService" />
public class PropertyManagementService : IPropertyManagementService
{
    private readonly AppDbContext _db;
    private readonly ILogger<PropertyManagementService> _logger;

    public PropertyManagementService(AppDbContext db, ILogger<PropertyManagementService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Property>> GetPropertiesWithUnitsAsync(CancellationToken cancellationToken = default) =>
        await _db.Properties
            .AsNoTracking()
            .Include(p => p.Units)
            .ThenInclude(u => u.UnitType)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<Property?> GetPropertyAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.Properties.FindAsync([id], cancellationToken);

    /// <inheritdoc />
    public async Task<int> SavePropertyAsync(Property property, CancellationToken cancellationToken = default)
    {
        if (property.Id == 0)
        {
            _db.Properties.Add(property);
        }
        else
        {
            _db.Properties.Update(property);
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Saved property {PropertyId} ({PropertyName})", property.Id, property.Name);
        return property.Id;
    }

    /// <inheritdoc />
    public async Task<string?> TryDeletePropertyAsync(int id, CancellationToken cancellationToken = default)
    {
        var property = await _db.Properties
            .Include(p => p.Units)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (property is null)
        {
            _logger.LogWarning("Delete property failed: property {PropertyId} not found", id);
            return "Property not found.";
        }

        if (property.Units.Count > 0)
        {
            _logger.LogWarning("Delete property {PropertyId} blocked: {UnitCount} units remain", id, property.Units.Count);
            return "Remove units before deleting a property.";
        }

        _db.Properties.Remove(property);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted property {PropertyId}", id);
        return null;
    }

    /// <inheritdoc />
    public async Task<Unit?> GetUnitAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.Units.FindAsync([id], cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Unit>> GetUnitsForPropertyAsync(int propertyId, CancellationToken cancellationToken = default) =>
        await _db.Units
            .AsNoTracking()
            .Include(u => u.UnitType)
            .Where(u => u.PropertyId == propertyId)
            .OrderBy(u => u.UnitNumber)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<UnitType>> GetSelectableUnitTypesAsync(int? existingUnitId, CancellationToken cancellationToken = default)
    {
        Unit? existing = existingUnitId is int id ? await GetUnitAsync(id, cancellationToken) : null;
        var types = await _db.UnitTypes.AsNoTracking().OrderBy(t => t.Name).ToListAsync(cancellationToken);
        return types
            .Where(t => t.IsActive || (existing is not null && existing.UnitTypeId == t.Id))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<(bool Success, string? UnitTypeError)> ValidateUnitTypeAsync(int unitTypeId, int? existingUnitId, CancellationToken cancellationToken = default)
    {
        var unitType = await _db.UnitTypes.FindAsync([unitTypeId], cancellationToken);
        if (unitType is null)
        {
            return (false, "Unit type is required.");
        }

        Unit? existing = existingUnitId is int id ? await GetUnitAsync(id, cancellationToken) : null;
        if (!UnitTypePolicy.CanAssignUnitType(unitType, existing))
        {
            return (false, "Inactive unit types cannot be selected.");
        }

        return (true, null);
    }

    /// <inheritdoc />
    public async Task<int> SaveUnitAsync(Unit unit, CancellationToken cancellationToken = default)
    {
        if (unit.Id == 0)
        {
            _db.Units.Add(unit);
        }
        else
        {
            _db.Units.Update(unit);
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Saved unit {UnitId} on property {PropertyId}", unit.Id, unit.PropertyId);
        return unit.Id;
    }

    /// <inheritdoc />
    public async Task<(bool Success, string? Error, int PropertyId)> TryDeleteUnitAsync(int id, CancellationToken cancellationToken = default)
    {
        var unit = await _db.Units
            .Include(u => u.Leases)
            .Include(u => u.Applications)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (unit is null)
        {
            _logger.LogWarning("Delete unit failed: unit {UnitId} not found", id);
            return (false, "Unit not found.", 0);
        }

        // Historical applications and leases are kept; delete is only allowed when none exist.
        if (unit.Leases.Count > 0 || unit.Applications.Count > 0)
        {
            _logger.LogWarning(
                "Delete unit {UnitId} blocked: {LeaseCount} leases, {ApplicationCount} applications",
                id,
                unit.Leases.Count,
                unit.Applications.Count);
            return (false, "This unit has leases or applications and cannot be removed.", unit.PropertyId);
        }

        _db.Units.Remove(unit);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted unit {UnitId} from property {PropertyId}", id, unit.PropertyId);
        return (true, null, unit.PropertyId);
    }
}
