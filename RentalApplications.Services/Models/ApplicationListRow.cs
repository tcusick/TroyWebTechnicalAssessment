using RentalApplications.Domain.Entities;

namespace RentalApplications.Services.Models;

public record ApplicationListRow(
    int Id,
    string ApplicantName,
    string PropertyName,
    string UnitNumber,
    ApplicationStatus Status,
    DateTimeOffset UpdatedAt);
