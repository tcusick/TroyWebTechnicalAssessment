using RentalApplications.Domain.Entities;
using RentalApplications.Models.Properties;

namespace RentalApplications.Mapping;

/// <summary>
/// Maps between <see cref="Unit"/> entities and unit form view models.
/// </summary>
public static class UnitMappingExtensions
{
    /// <summary>
    /// Builds a form view model from a unit entity.
    /// </summary>
    public static UnitFormViewModel ToFormViewModel(this Unit unit) => new()
    {
        Id = unit.Id,
        PropertyId = unit.PropertyId,
        UnitNumber = unit.UnitNumber,
        Bedrooms = unit.Bedrooms,
        MonthlyRent = unit.MonthlyRent,
        UnitTypeId = unit.UnitTypeId
    };

    /// <summary>
    /// Copies posted form values onto a unit entity.
    /// </summary>
    public static void ApplyForm(this Unit unit, UnitFormViewModel model)
    {
        unit.UnitNumber = model.UnitNumber;
        unit.Bedrooms = model.Bedrooms;
        unit.MonthlyRent = model.MonthlyRent;
        unit.UnitTypeId = model.UnitTypeId;
    }

    /// <summary>
    /// Maps a unit type to a dropdown option view model.
    /// </summary>
    public static UnitTypeOptionViewModel ToOptionViewModel(this UnitType unitType) => new()
    {
        Id = unitType.Id,
        Name = unitType.Name,
        IsActive = unitType.IsActive
    };
}
