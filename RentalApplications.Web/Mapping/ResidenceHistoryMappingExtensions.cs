using RentalApplications.Domain.Entities;
using RentalApplications.Models.Applications;
using RentalApplications.Models.Shared;

namespace RentalApplications.Mapping;

/// <summary>
/// Maps between <see cref="ResidenceHistory"/> entities and residence view models.
/// </summary>
public static class ResidenceHistoryMappingExtensions
{
    private static StructuredAddressViewModel ToAddressViewModel(this ResidenceHistory residence) =>
        StructuredAddressMappingExtensions.ToAddressViewModel(
            residence.StreetAddress,
            residence.City,
            residence.State,
            residence.PostalCode);

    /// <summary>
    /// Maps a residence to a list item on the application wizard.
    /// </summary>
    public static ResidenceHistoryItemViewModel ToItemViewModel(this ResidenceHistory residence) => new()
    {
        Id = residence.Id,
        Address = residence.ToAddressViewModel(),
        LandlordName = residence.LandlordName,
        LandlordPhone = residence.LandlordPhone,
        MoveInDate = residence.MoveInDate,
        MoveOutDate = residence.MoveOutDate
    };

    /// <summary>
    /// Maps a residence to the modal edit form view model.
    /// </summary>
    public static ResidenceFormViewModel ToFormViewModel(this ResidenceHistory residence) => new()
    {
        ApplicationId = residence.RentalApplicationId,
        Id = residence.Id,
        Address = residence.ToAddressViewModel(),
        LandlordName = residence.LandlordName,
        LandlordPhone = residence.LandlordPhone,
        MoveInDate = residence.MoveInDate,
        MoveOutDate = residence.MoveOutDate
    };

    /// <summary>
    /// Copies posted form values onto a residence entity.
    /// </summary>
    public static void ApplyForm(this ResidenceHistory residence, ResidenceFormViewModel model)
    {
        residence.StreetAddress = model.Address.StreetAddress;
        residence.City = model.Address.City;
        residence.State = model.Address.State;
        residence.PostalCode = model.Address.PostalCode;
        residence.LandlordName = model.LandlordName;
        residence.LandlordPhone = model.LandlordPhone;
        residence.MoveInDate = model.MoveInDate;
        residence.MoveOutDate = model.MoveOutDate;
    }
}
