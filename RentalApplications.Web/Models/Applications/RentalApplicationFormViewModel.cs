using System.ComponentModel.DataAnnotations;
using RentalApplications.Domain.Entities;

namespace RentalApplications.Models.Applications;

public class RentalApplicationFormViewModel
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string UnitNumber { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public ApplicationSection CurrentSection { get; set; }
    public bool IsReadOnly { get; set; }
    public bool CanSubmit { get; set; }
    public bool CanWithdraw { get; set; }
    public bool ShowManagerHistory { get; set; }

    public ApplicantInformationSectionViewModel ApplicantInformation { get; set; } = new();
    public ResidenceHistorySectionViewModel ResidenceHistory { get; set; } = new();

    public string? Command { get; set; }
}

public class ApplicantInformationSectionViewModel
{
    [Required, StringLength(200)]
    public string ApplicantName { get; set; } = string.Empty;

    [Required, Phone]
    public string Phone { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string CurrentAddress { get; set; } = string.Empty;
}

public class ResidenceHistorySectionViewModel
{
    public List<ResidenceHistoryItemViewModel> Items { get; set; } = new();
}

public class ResidenceHistoryItemViewModel
{
    public int Id { get; set; }

    [Required, StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string LandlordName { get; set; } = string.Empty;

    [Required, Phone]
    public string LandlordPhone { get; set; } = string.Empty;

    [Required]
    public DateOnly MoveInDate { get; set; }

    public DateOnly? MoveOutDate { get; set; }
}
