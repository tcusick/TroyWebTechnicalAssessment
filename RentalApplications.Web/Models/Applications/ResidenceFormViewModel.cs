using System.ComponentModel.DataAnnotations;
using RentalApplications.Models.Shared;
using RentalApplications.Validation;

namespace RentalApplications.Models.Applications;

public class ResidenceFormViewModel : IValidatableObject
{
    public int ApplicationId { get; set; }
    public int? Id { get; set; }

    [Required]
    public StructuredAddressViewModel Address { get; set; } = new();

    [Required, StringLength(200, MinimumLength = 2), Display(Name = "Landlord name"), RegularExpression(ValidationConstants.NonWhitespacePattern, ErrorMessage = ValidationConstants.NonWhitespaceErrorMessage)]
    public string LandlordName { get; set; } = string.Empty;

    [Required, Display(Name = "Landlord phone"), RegularExpression(ValidationConstants.PhonePattern, ErrorMessage = ValidationConstants.PhoneErrorMessage)]
    public string LandlordPhone { get; set; } = string.Empty;

    [Required, Display(Name = "Move in date"), ValidPastDate]
    public DateOnly MoveInDate { get; set; }

    [Display(Name = "Move out date")]
    public DateOnly? MoveOutDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in NestedObjectValidation.ValidateObject(Address, nameof(Address)))
        {
            yield return result;
        }

        if (MoveOutDate is not DateOnly moveOut)
        {
            yield break;
        }

        if (DateValidationRules.IsUnset(moveOut))
        {
            yield return new ValidationResult("Enter a valid move-out date.", [nameof(MoveOutDate)]);
            yield break;
        }

        if (moveOut < DateValidationRules.MinHistoricalDate)
        {
            yield return new ValidationResult($"Move-out date must be on or after {DateValidationRules.MinHistoricalDate:d}.", [nameof(MoveOutDate)]);
        }
        else if (moveOut > DateValidationRules.UtcToday)
        {
            yield return new ValidationResult("Move-out date cannot be in the future.", [nameof(MoveOutDate)]);
        }
        else if (moveOut <= MoveInDate)
        {
            yield return new ValidationResult("Move-out date must be after move-in date.", [nameof(MoveOutDate)]);
        }
    }
}
