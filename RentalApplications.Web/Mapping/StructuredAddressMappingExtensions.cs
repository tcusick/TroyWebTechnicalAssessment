using RentalApplications.Models.Shared;

namespace RentalApplications.Mapping;

public static class StructuredAddressMappingExtensions
{
    public static StructuredAddressViewModel ToAddressViewModel(
        string streetAddress,
        string city,
        string state,
        string postalCode) => new()
    {
        StreetAddress = streetAddress,
        City = city,
        State = state,
        PostalCode = postalCode
    };
}
