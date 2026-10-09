using RentalApplications.Domain.Entities;

namespace RentalApplications.Domain;

/// <summary>
/// Rules for assigning active and inactive unit types to units.
/// </summary>
public static class UnitTypePolicy
{
    /// <summary>
    /// Returns whether the unit type may be selected for a new or existing unit.
    /// </summary>
    public static bool CanAssignUnitType(UnitType unitType, Unit? existingUnit)
    {
        if (unitType.IsActive)
        {
            return true;
        }

        // Inactive types may remain on a unit that already uses them; they cannot be newly assigned.
        return existingUnit is not null && existingUnit.UnitTypeId == unitType.Id;
    }
}
