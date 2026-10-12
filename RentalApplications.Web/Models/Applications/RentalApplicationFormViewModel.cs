using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using RentalApplications.Domain.Entities;
using RentalApplications.Models.Shared;
using RentalApplications.Validation;

namespace RentalApplications.Models.Applications;

public class RentalApplicationFormViewModel
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string UnitNumber { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public ApplicationSection CurrentSection { get; set; }
    public bool IsReadOnly { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanWithdraw { get; set; }
    public bool ShowManagerHistory { get; set; }

    [ValidateNever]
    public ApplicantInformationSectionViewModel ApplicantInformation { get; set; } = new();

    [ValidateNever]
    public ResidenceHistorySectionViewModel ResidenceHistory { get; set; } = new();

    public string? Command { get; set; }
}

public class ApplicantInformationSectionViewModel : IValidatableObject
{
    [Required, StringLength(200, MinimumLength = 2), Display(Name = "Applicant name"), RegularExpression(ValidationConstants.NonWhitespacePattern, ErrorMessage = ValidationConstants.NonWhitespaceErrorMessage)]
    public string ApplicantName { get; set; } = string.Empty;

    [Required, Display(Name = "Phone"), RegularExpression(ValidationConstants.PhonePattern, ErrorMessage = ValidationConstants.PhoneErrorMessage)]
    public string Phone { get; set; } = string.Empty;

    [Required, Display(Name = "Email"), EmailAddress(ErrorMessage = ValidationConstants.EmailErrorMessage), RegularExpression(ValidationConstants.EmailPattern, ErrorMessage = ValidationConstants.EmailErrorMessage)]
    public string Email { get; set; } = string.Empty;

    [Required, Display(Name = "Current address")]
    public StructuredAddressViewModel CurrentAddress { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in NestedObjectValidation.ValidateObject(CurrentAddress, nameof(CurrentAddress)))
        {
            yield return result;
        }
    }
}

public class ResidenceHistorySectionViewModel
{
    public List<ResidenceHistoryItemViewModel> Items { get; set; } = new();
}

public class ResidenceHistoryItemViewModel
{
    public int Id { get; set; }

    public StructuredAddressViewModel Address { get; set; } = new();

    [Required, StringLength(200)]
    public string LandlordName { get; set; } = string.Empty;

    [Required, Phone]
    public string LandlordPhone { get; set; } = string.Empty;

    [Required]
    public DateOnly MoveInDate { get; set; }

    public DateOnly? MoveOutDate { get; set; }
}
