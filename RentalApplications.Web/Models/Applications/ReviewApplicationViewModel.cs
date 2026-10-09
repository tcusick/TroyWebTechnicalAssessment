using System.ComponentModel.DataAnnotations;
using RentalApplications.Domain;

namespace RentalApplications.Models.Applications;

public class ReviewApplicationViewModel
{
    public int ApplicationId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string UnitSummary { get; set; } = string.Empty;

    [Required]
    public ReviewOutcome Outcome { get; set; }

    [StringLength(2000)]
    public string? Comment { get; set; }

    [Required]
    public DateOnly LeaseStartDate { get; set; }
}
