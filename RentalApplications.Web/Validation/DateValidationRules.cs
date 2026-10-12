namespace RentalApplications.Validation;

public static class DateValidationRules
{
    public static readonly DateOnly MinHistoricalDate = new(1900, 1, 1);

    public static DateOnly UtcToday => DateOnly.FromDateTime(DateTime.UtcNow);

    public static DateOnly MaxLeaseStartDate => UtcToday.AddYears(2);

    public static bool IsUnset(DateOnly date) => date == default;

    public static bool IsValidPastDate(DateOnly date) =>
        !IsUnset(date) && date >= MinHistoricalDate && date <= UtcToday;

    public static bool IsValidLeaseStartDate(DateOnly date) =>
        !IsUnset(date) && date >= UtcToday && date <= MaxLeaseStartDate;
}
