using System.ComponentModel.DataAnnotations;
using RentalApplications.Domain;
using RentalApplications.Validation;

namespace RentalApplications.Models.Applications;

public class ReviewApplicationViewModel : IValidatableObject
{
    public int ApplicationId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string UnitSummary { get; set; } = string.Empty;

    [Required, Display(Name = "Decision")]
    public ReviewOutcome Outcome { get; set; }

    [StringLength(2000)]
    public string? Comment { get; set; }

    [Required, Display(Name = "Lease start date"), ValidLeaseStartDate]
    public DateOnly LeaseStartDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ApplicationReviewPolicy.RequiresComment(Outcome) && string.IsNullOrWhiteSpace(Comment))
        {
            yield return new ValidationResult("A comment is required for this outcome.", [nameof(Comment)]);
        }
    }
}
