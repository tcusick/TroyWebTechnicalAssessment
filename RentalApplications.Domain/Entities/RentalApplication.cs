namespace RentalApplications.Domain.Entities;

public class RentalApplication
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public string ApplicantUserId { get; set; } = string.Empty;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;
    public ApplicationSection CurrentSection { get; set; } = ApplicationSection.ApplicantInformation;

    public bool ApplicantInformationSaved { get; set; }
    public bool ResidenceHistorySaved { get; set; }

    public string ApplicantName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CurrentStreetAddress { get; set; } = string.Empty;
    public string CurrentCity { get; set; } = string.Empty;
    public string CurrentState { get; set; } = string.Empty;
    public string CurrentPostalCode { get; set; } = string.Empty;

    public ICollection<ResidenceHistory> ResidenceHistories { get; set; } = new List<ResidenceHistory>();
    public ICollection<ApplicationStatusHistory> StatusHistories { get; set; } = new List<ApplicationStatusHistory>();
    public Lease? Lease { get; set; }
}
