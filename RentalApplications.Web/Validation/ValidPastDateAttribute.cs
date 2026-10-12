using System.ComponentModel.DataAnnotations;

namespace RentalApplications.Validation;

/// <summary>
/// Validates a required calendar date in the past (e.g. residence move-in / move-out).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ValidPastDateAttribute : ValidationAttribute
{
    public ValidPastDateAttribute() : base("Enter a valid date between {0} and today.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return new ValidationResult(FormatErrorMessage(DateValidationRules.MinHistoricalDate.ToString("d")));
        }

        if (value is not DateOnly date)
        {
            return new ValidationResult("Enter a valid date.");
        }

        if (DateValidationRules.IsUnset(date))
        {
            return new ValidationResult("Enter a valid date.");
        }

        if (date < DateValidationRules.MinHistoricalDate)
        {
            return new ValidationResult($"Date must be on or after {DateValidationRules.MinHistoricalDate:d}.");
        }

        if (date > DateValidationRules.UtcToday)
        {
            return new ValidationResult("Date cannot be in the future.");
        }

        return ValidationResult.Success;
    }

    public override string FormatErrorMessage(string name) =>
        string.Format(ErrorMessageString, DateValidationRules.MinHistoricalDate.ToString("d"));
}
