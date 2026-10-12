namespace RentalApplications.Validation;

/// <summary>
/// Shared patterns for data annotation validation on forms.
/// </summary>
public static class ValidationConstants
{
    public const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$";

    public const string EmailErrorMessage = "Enter a valid email address.";

    /// <summary>North American phone numbers (10 digits, optional +1 and separators).</summary>
    public const string PhonePattern = @"^(\+?1[-.\s]?)?(\(?\d{3}\)?[-.\s]?)\d{3}[-.\s]?\d{4}$";

    public const string PhoneErrorMessage = "Enter a valid phone number (e.g. 555-123-4567).";

    public const string PostalCodePattern = @"^\d{5}(-\d{4})?$";

    public const string PostalCodeErrorMessage = "Enter a valid ZIP code (e.g. 12345 or 12345-6789).";

    public const string StatePattern = @"^[A-Za-z]{2}$";

    public const string StateErrorMessage = "Enter a two-letter state code (e.g. NY).";

    public const string NonWhitespacePattern = @"^\s*(\S+\s*)+$";

    public const string NonWhitespaceErrorMessage = "This field cannot be blank.";
}
