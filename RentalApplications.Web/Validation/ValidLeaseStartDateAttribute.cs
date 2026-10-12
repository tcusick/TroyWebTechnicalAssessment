using System.ComponentModel.DataAnnotations;

namespace RentalApplications.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ValidLeaseStartDateAttribute : ValidationAttribute
{
    public ValidLeaseStartDateAttribute() : base("Lease start must be between today and {0}.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateOnly date)
        {
            return new ValidationResult("Enter a lease start date.");
        }

        if (DateValidationRules.IsUnset(date))
        {
            return new ValidationResult("Enter a lease start date.");
        }

        if (date < DateValidationRules.UtcToday)
        {
            return new ValidationResult("Lease start date cannot be in the past.");
        }

        if (date > DateValidationRules.MaxLeaseStartDate)
        {
            return new ValidationResult($"Lease start date cannot be later than {DateValidationRules.MaxLeaseStartDate:d}.");
        }

        return ValidationResult.Success;
    }

    public override string FormatErrorMessage(string name) =>
        string.Format(ErrorMessageString, DateValidationRules.MaxLeaseStartDate.ToString("d"));
}
