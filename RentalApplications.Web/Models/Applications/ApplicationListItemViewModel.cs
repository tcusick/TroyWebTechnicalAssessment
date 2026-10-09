using RentalApplications.Domain.Entities;

namespace RentalApplications.Models.Applications;

public class ApplicationListItemViewModel
{
    public int Id { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string UnitNumber { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
