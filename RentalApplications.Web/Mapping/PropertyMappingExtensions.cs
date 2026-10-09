using RentalApplications.Domain.Entities;
using RentalApplications.Models.Properties;

namespace RentalApplications.Mapping;

/// <summary>
/// Maps between <see cref="Property"/> entities and property form view models.
/// </summary>
public static class PropertyMappingExtensions
{
    /// <summary>
    /// Builds a form view model from a property entity.
    /// </summary>
    public static PropertyFormViewModel ToFormViewModel(this Property property) => new()
    {
        Id = property.Id,
        Name = property.Name,
        StreetAddress = property.StreetAddress,
        City = property.City,
        State = property.State,
        PostalCode = property.PostalCode
    };

    /// <summary>
    /// Copies posted form values onto a property entity.
    /// </summary>
    public static void ApplyForm(this Property property, PropertyFormViewModel model)
    {
        property.Name = model.Name;
        property.StreetAddress = model.StreetAddress;
        property.City = model.City;
        property.State = model.State;
        property.PostalCode = model.PostalCode;
    }
}
