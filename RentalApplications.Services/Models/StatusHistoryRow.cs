using RentalApplications.Domain.Entities;

namespace RentalApplications.Services.Models;

public record StatusHistoryRow(
    ApplicationStatus FromStatus,
    ApplicationStatus ToStatus,
    string ChangedByName,
    DateTimeOffset ChangedAt,
    string? Comment);
