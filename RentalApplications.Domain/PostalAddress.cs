namespace RentalApplications.Domain;

/// <summary>
/// Helpers for structured street addresses stored on entities.
/// </summary>
public static class PostalAddress
{
    public static string Format(string streetAddress, string city, string state, string postalCode) =>
        $"{streetAddress}, {city}, {state} {postalCode}";
}
