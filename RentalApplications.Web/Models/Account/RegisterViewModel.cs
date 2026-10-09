using System.ComponentModel.DataAnnotations;
using RentalApplications.Domain;

namespace RentalApplications.Models.Account;

public class RegisterViewModel
{
    [Required, StringLength(100), Display(Name = "Full Name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required, Display(Name = "Type")]
    public string Role { get; set; } = AppRoles.Applicant;
}
