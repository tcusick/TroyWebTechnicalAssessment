using System.ComponentModel.DataAnnotations;
using RentalApplications.Validation;

namespace RentalApplications.Models.Account;

public class LoginViewModel
{
    [Required, EmailAddress(ErrorMessage = ValidationConstants.EmailErrorMessage), RegularExpression(ValidationConstants.EmailPattern, ErrorMessage = ValidationConstants.EmailErrorMessage)]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
