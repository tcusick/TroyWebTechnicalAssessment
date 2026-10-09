using RentalApplications.Domain.Entities;

namespace RentalApplications.Services.Abstractions;

/// <summary>
/// CRUD and validation for properties, units, and unit type assignment.
/// </summary>
public interface IPropertyManagementService
{
    /// <summary>
    /// Loads all properties with their units for the manager index page.
    /// </summary>
    Task<IReadOnlyList<Property>> GetPropertiesWithUnitsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a single property by id.
    /// </summary>
    Task<Property?> GetPropertyAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates a property and returns its id.
    /// </summary>
    Task<int> SavePropertyAsync(Property property, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a property when it has no units; returns an error message when deletion is blocked.
    /// </summary>
    Task<string?> TryDeletePropertyAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a unit with its property and unit type.
    /// </summary>
    Task<Unit?> GetUnitAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads units belonging to a property.
    /// </summary>
    Task<IReadOnlyList<Unit>> GetUnitsForPropertyAsync(int propertyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns unit types that may be selected in the unit form (active types plus the current type when editing).
    /// </summary>
    Task<IReadOnlyList<UnitType>> GetSelectableUnitTypesAsync(int? existingUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the chosen unit type may be assigned to the unit.
    /// </summary>
    Task<(bool Success, string? UnitTypeError)> ValidateUnitTypeAsync(int unitTypeId, int? existingUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates a unit and returns its id.
    /// </summary>
    Task<int> SaveUnitAsync(Unit unit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a unit when allowed; returns success, error message, and parent property id.
    /// </summary>
    Task<(bool Success, string? Error, int PropertyId)> TryDeleteUnitAsync(int id, CancellationToken cancellationToken = default);
}
