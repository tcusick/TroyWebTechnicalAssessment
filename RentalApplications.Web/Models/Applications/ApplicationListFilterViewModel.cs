using RentalApplications.Domain.Entities;

namespace RentalApplications.Models.Applications;

public class ApplicationListFilterViewModel
{
    public ApplicationStatus? Status { get; set; }
    public int? PropertyId { get; set; }
}
