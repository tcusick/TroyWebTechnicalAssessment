using System.ComponentModel.DataAnnotations;
using RentalApplications.Domain;
using RentalApplications.Validation;

namespace RentalApplications.Models.Account;

public class RegisterViewModel
{
    [Required, StringLength(100, MinimumLength = 2), Display(Name = "Full Name"), RegularExpression(ValidationConstants.NonWhitespacePattern, ErrorMessage = ValidationConstants.NonWhitespaceErrorMessage)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, EmailAddress(ErrorMessage = ValidationConstants.EmailErrorMessage), RegularExpression(ValidationConstants.EmailPattern, ErrorMessage = ValidationConstants.EmailErrorMessage)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password), Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required, Display(Name = "Type")]
    public string Role { get; set; } = AppRoles.Applicant;
}
