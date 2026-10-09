namespace RentalApplications.Services.Models;

public record AvailableUnitRow(
    int UnitId,
    string PropertyName,
    string Address,
    string UnitNumber,
    int Bedrooms,
    decimal MonthlyRent,
    string UnitTypeName);
