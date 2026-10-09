using RentalApplications.Models.AvailableUnits;
using RentalApplications.Services.Models;

namespace RentalApplications.Mapping;

/// <summary>
/// Maps available unit query rows to browse view models.
/// </summary>
public static class AvailableUnitMappingExtensions
{
    /// <summary>
    /// Maps an available unit row to the browse units view model.
    /// </summary>
    public static AvailableUnitViewModel ToViewModel(this AvailableUnitRow row) => new()
    {
        UnitId = row.UnitId,
        PropertyName = row.PropertyName,
        Address = row.Address,
        UnitNumber = row.UnitNumber,
        Bedrooms = row.Bedrooms,
        MonthlyRent = row.MonthlyRent,
        UnitTypeName = row.UnitTypeName
    };
}
