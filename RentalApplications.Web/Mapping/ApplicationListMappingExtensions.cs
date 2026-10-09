using RentalApplications.Models.Applications;
using RentalApplications.Services.Models;

namespace RentalApplications.Mapping;

/// <summary>
/// Maps service list rows to application list view models.
/// </summary>
public static class ApplicationListMappingExtensions
{
    /// <summary>
    /// Maps a list row from the application service to the index view model.
    /// </summary>
    public static ApplicationListItemViewModel ToListItemViewModel(this ApplicationListRow row) => new()
    {
        Id = row.Id,
        ApplicantName = row.ApplicantName,
        PropertyName = row.PropertyName,
        UnitNumber = row.UnitNumber,
        Status = row.Status,
        UpdatedAt = row.UpdatedAt
    };
}
