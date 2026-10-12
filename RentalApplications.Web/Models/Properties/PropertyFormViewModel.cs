using System.ComponentModel.DataAnnotations;
using RentalApplications.Validation;

namespace RentalApplications.Models.Properties;

public class PropertyFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(200, MinimumLength = 2), RegularExpression(ValidationConstants.NonWhitespacePattern, ErrorMessage = ValidationConstants.NonWhitespaceErrorMessage)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(300, MinimumLength = 3), Display(Name = "Street address"), RegularExpression(ValidationConstants.NonWhitespacePattern, ErrorMessage = ValidationConstants.NonWhitespaceErrorMessage)]
    public string StreetAddress { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2), RegularExpression(ValidationConstants.NonWhitespacePattern, ErrorMessage = ValidationConstants.NonWhitespaceErrorMessage)]
    public string City { get; set; } = string.Empty;

    [Required, StringLength(2, MinimumLength = 2), Display(Name = "State"), RegularExpression(ValidationConstants.StatePattern, ErrorMessage = ValidationConstants.StateErrorMessage)]
    public string State { get; set; } = string.Empty;

    [Required, StringLength(20), Display(Name = "Postal code"), RegularExpression(ValidationConstants.PostalCodePattern, ErrorMessage = ValidationConstants.PostalCodeErrorMessage)]
    public string PostalCode { get; set; } = string.Empty;
}
